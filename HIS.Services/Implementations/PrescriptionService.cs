using HIS.Common.Enums;
using HIS.Common.Helpers;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 处方服务实现
    /// 核心业务逻辑：
    /// 1. 自动生成处方号
    /// 2. 计算处方总金额（明细金额累加）
    /// 3. 关联门诊记录和患者
    /// 4. 支持西药/中药/检查三种处方类型
    /// 5. 开具后自动扣减药品库存
    /// </summary>
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IOutpatientRecordRepository _outpatientRepository;
        private readonly IRegistrationRepository _regRepository;
        private readonly IDrugRepository _drugRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IDrugStockRepository _drugStockRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PrescriptionService(
            IPrescriptionRepository prescriptionRepository,
            IOutpatientRecordRepository outpatientRepository,
            IRegistrationRepository regRepository,
            IDrugRepository drugRepository,
            IDoctorRepository doctorRepository,
            IDrugStockRepository drugStockRepository,
            IUnitOfWork unitOfWork)
        {
            _prescriptionRepository = prescriptionRepository;
            _outpatientRepository = outpatientRepository;
            _regRepository = regRepository;
            _drugRepository = drugRepository;
            _doctorRepository = doctorRepository;
            _drugStockRepository = drugStockRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 分页查询处方列表
        /// </summary>
        public async Task<(List<PrescriptionDto> List, int Total)> GetListAsync(
            PrescriptionQueryModel query)
        {
            var (list, total) = await _prescriptionRepository.GetListAsync(query);
            var dtos = list.Select(MapToDto).ToList();
            return (dtos, total);
        }

        /// <summary>
        /// 根据ID获取处方详情
        /// </summary>
        public async Task<PrescriptionDto?> GetByIdAsync(long id)
        {
            var prescription = await _prescriptionRepository.GetDetailAsync(id);
            return prescription == null ? null : MapToDetailDto(prescription);
        }

        /// <summary>
        /// 根据门诊记录ID获取处方列表
        /// </summary>
        public async Task<List<PrescriptionDto>> GetByOutpatientRecordIdAsync(long outpatientRecordId)
        {
            var list = await _prescriptionRepository.FindAsync(
                p => p.OutpatientRecordId == outpatientRecordId);
            return list.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 开具处方
        /// 业务规则：
        /// - 必须有门诊诊疗记录
        /// - 必须有至少一个处方明细
        /// - 自动计算总金额
        /// - 可选择性扣减药品库存
        /// </summary>
        public async Task<(bool Success, string Message)> CreateAsync(
            PrescriptionDto dto, long userId)
        {
            if (dto == null) return (false, "请求数据为空");
            // 通过系统用户ID查找对应的医生信息
            var doctor = await _doctorRepository.GetByUserIdAsync(userId);
            if (doctor == null)
                return (false, "当前用户不是医生，无法开具处方");

            // 校验门诊记录
            var outpatientRecord = await _outpatientRepository.GetByIdAsync(dto.OutpatientRecordId);
            if (outpatientRecord == null)
                return (false, "门诊诊疗记录不存在，请先完成接诊");

            // 校验处方明细
            if (dto.Details == null || dto.Details.Count == 0)
                return (false, "请至少添加一个处方项目");

            // 自动生成处方号
            var prescriptionNo = GenerateNoHelper.GeneratePrescriptionNo();

            // 计算总金额并构建明细
            decimal totalAmount = 0;
            var details = new List<PrescriptionDetail>();

            foreach (var item in dto.Details)
            {
                // 如果是药品，校验药品存在并获取零售价
                if (item.ItemType == (int)PrescriptionItemTypeEnum.Drug)
                {
                    var drug = await _drugRepository.GetByIdAsync(item.ItemId);
                    if (drug == null)
                        return (false, $"药品ID {item.ItemId} 不存在");

                    if (drug.StockQuantity < item.Quantity)
                        return (false, $"药品 {drug.DrugName} 库存不足（当前库存：{drug.StockQuantity}）");

                    item.UnitPrice = drug.RetailPrice;
                    item.ItemName = drug.DrugName;
                    item.Specification = drug.Specification;
                    item.Unit = drug.Unit;
                }

                item.Amount = item.UnitPrice * item.Quantity;
                totalAmount += item.Amount;

                details.Add(new PrescriptionDetail
                {
                    ItemType = item.ItemType,
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    Specification = item.Specification,
                    Unit = item.Unit,
                    UnitPrice = item.UnitPrice,
                    Quantity = item.Quantity,
                    Amount = item.Amount,
                    Usage = item.Usage,
                    Dosage = item.Dosage,
                    Frequency = item.Frequency,
                    Days = item.Days
                });
            }

            var prescription = new Prescription
            {
                PrescriptionNo = prescriptionNo,
                RegistrationId = outpatientRecord.RegistrationId,
                OutpatientRecordId = dto.OutpatientRecordId,
                PatientId = outpatientRecord.PatientId,
                DoctorId = doctor.Id,
                PrescriptionType = dto.PrescriptionType,
                TotalAmount = totalAmount,
                Status = (int)PrescriptionStatusEnum.Issued,
                Remark = dto.Remark,
                Details = details,
                CreateTime = DateTime.Now
            };

            await _prescriptionRepository.AddAsync(prescription);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"处方开具成功！处方号：{prescriptionNo}，合计：{totalAmount:F2}元");
        }

        /// <summary>获取待发药处方（仅药品类，默认状态=已收费；检查类不走药房）</summary>
        public async Task<(List<PrescriptionDto> List, int Total)> GetPendingDispenseAsync(PrescriptionQueryModel query)
        {
            if (!query.Status.HasValue) query.Status = (int)PrescriptionStatusEnum.Charged;
            var (list, total) = await _prescriptionRepository.GetListAsync(query);
            // 过滤掉检查处方（检查不走药房发药流程）
            var drugPrescriptions = list
                .Where(p => p.PrescriptionType != (int)PrescriptionTypeEnum.Examination)
                .ToList();
            return (drugPrescriptions.Select(MapToDto).ToList(), total);
        }

        /// <summary>确认发药：审方 → 扣库存 → 改处方状态</summary>
        public async Task<(bool Success, string Message)> DispenseAsync(long prescriptionId, long userId)
        {
            var prescription = await _prescriptionRepository.GetDetailAsync(prescriptionId);
            if (prescription == null) return (false, "处方不存在");
            if (prescription.Status != (int)PrescriptionStatusEnum.Charged)
                return (false, "处方未收费，请先完成收费再发药");

            // 逐一校验药品库存并扣减
            foreach (var detail in prescription.Details)
            {
                if (detail.ItemType != (int)PrescriptionItemTypeEnum.Drug) continue;

                var drug = await _drugRepository.GetByIdAsync(detail.ItemId);
                if (drug == null) return (false, $"药品 {detail.ItemName} 不存在");
                if (drug.StockQuantity < detail.Quantity)
                    return (false, $"药品 {drug.DrugName} 库存不足（需要{detail.Quantity}，库存{drug.StockQuantity}）");

                var beforeQty = drug.StockQuantity;
                drug.StockQuantity -= detail.Quantity;
                _drugRepository.Update(drug);

                // 记录库存日志
                await _drugStockRepository.AddAsync(new DrugStockLog
                {
                    DrugId = drug.Id,
                    ChangeType = 3, // 发药
                    ChangeQuantity = -detail.Quantity,
                    BeforeQuantity = beforeQty,
                    AfterQuantity = drug.StockQuantity,
                    RelatedNo = prescription.PrescriptionNo,
                    CreateUserId = userId,
                    CreateTime = DateTime.Now
                });
            }

            prescription.Status = (int)PrescriptionStatusEnum.Dispensed;
            _prescriptionRepository.Update(prescription);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"发药成功！处方号：{prescription.PrescriptionNo}");
        }

        /// <summary>退药：已发药 → 库存回加 → 改状态为已退方</summary>
        public async Task<(bool Success, string Message)> ReturnAsync(long prescriptionId, long userId)
        {
            var prescription = await _prescriptionRepository.GetDetailAsync(prescriptionId);
            if (prescription == null) return (false, "处方不存在");
            if (prescription.Status != (int)PrescriptionStatusEnum.Dispensed)
                return (false, "该处方未发药，无法退药");

            foreach (var detail in prescription.Details)
            {
                if (detail.ItemType != (int)PrescriptionItemTypeEnum.Drug) continue;
                var drug = await _drugRepository.GetByIdAsync(detail.ItemId);
                if (drug == null) return (false, $"药品 {detail.ItemName} 不存在");

                var beforeQty = drug.StockQuantity;
                drug.StockQuantity += detail.Quantity;
                _drugRepository.Update(drug);

                await _drugStockRepository.AddAsync(new DrugStockLog
                {
                    DrugId = drug.Id, ChangeType = 4, // 退药
                    ChangeQuantity = detail.Quantity, BeforeQuantity = beforeQty,
                    AfterQuantity = drug.StockQuantity, RelatedNo = prescription.PrescriptionNo,
                    CreateUserId = userId, CreateTime = DateTime.Now
                });
            }

            prescription.Status = (int)PrescriptionStatusEnum.Refunded;
            _prescriptionRepository.Update(prescription);
            await _unitOfWork.SaveChangesAsync();
            return (true, $"退药成功！处方号：{prescription.PrescriptionNo}");
        }

        #region 私有辅助方法

        private static PrescriptionDto MapToDto(Prescription p) => new()
        {
            Id = p.Id,
            PrescriptionNo = p.PrescriptionNo,
            PatientId = p.PatientId,
            PatientName = p.Patient?.Name,
            DoctorId = p.DoctorId,
            DoctorName = p.Doctor?.Name,
            PrescriptionType = p.PrescriptionType,
            TotalAmount = p.TotalAmount,
            Status = p.Status,
            CreateTime = p.CreateTime
        };

        private static PrescriptionDto MapToDetailDto(Prescription p) => new()
        {
            Id = p.Id,
            PrescriptionNo = p.PrescriptionNo,
            PatientId = p.PatientId,
            PatientName = p.Patient?.Name,
            DoctorId = p.DoctorId,
            DoctorName = p.Doctor?.Name,
            PrescriptionType = p.PrescriptionType,
            TotalAmount = p.TotalAmount,
            Status = p.Status,
            CreateTime = p.CreateTime,
            Details = p.Details.Select(d => new PrescriptionDetailDto
            {
                Id = d.Id,
                PrescriptionId = d.PrescriptionId,
                ItemType = d.ItemType,
                ItemId = d.ItemId,
                ItemName = d.ItemName,
                Specification = d.Specification,
                Unit = d.Unit,
                UnitPrice = d.UnitPrice,
                Quantity = d.Quantity,
                Amount = d.Amount,
                Usage = d.Usage,
                Dosage = d.Dosage,
                Frequency = d.Frequency,
                Days = d.Days
            }).ToList()
        };

        #endregion
    }
}
