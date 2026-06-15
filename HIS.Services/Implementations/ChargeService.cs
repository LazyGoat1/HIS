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
    public class ChargeService : IChargeService
    {
        private readonly IChargeRecordRepository _chargeRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IPrescriptionRepository _prescriptionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public ChargeService(IChargeRecordRepository chargeRepo, IPatientRepository patientRepo,
            IPrescriptionRepository prescriptionRepo, IUnitOfWork unitOfWork)
        {
            _chargeRepo = chargeRepo;
            _patientRepo = patientRepo;
            _prescriptionRepo = prescriptionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<ChargeRecordDto> List, int Total)> GetListAsync(ChargeQueryModel query)
        {
            var (list, total) = await _chargeRepo.GetListAsync(query);
            return (list.Select(MapToDto).ToList(), total);
        }

        public async Task<ChargeRecordDto?> GetByIdAsync(long id)
        {
            var c = await _chargeRepo.GetDetailAsync(id);
            return c == null ? null : MapToDto(c);
        }

        public async Task<(bool Success, string Message)> CreateAsync(ChargeCreateDto dto, long userId)
        {
            if (dto == null) return (false, "请求数据为空");
            if (dto.PatientId <= 0) return (false, "请选择患者");
            if (dto.TotalAmount <= 0) return (false, "应收金额不能为0");
            if (dto.PaidAmount <= 0) return (false, "实收金额不能为0");

            var patient = await _patientRepo.GetByIdAsync(dto.PatientId);
            if (patient == null) return (false, "患者不存在");

            // 门诊收费：关联处方，收完费自动更新处方状态为"已收费"
            if (dto.ChargeType == (int)ChargeTypeEnum.Outpatient && dto.RelatedId.HasValue)
            {
                var prescription = await _prescriptionRepo.GetByIdAsync(dto.RelatedId.Value);
                if (prescription == null) return (false, "关联处方不存在");
                if (prescription.Status != (int)PrescriptionStatusEnum.Issued)
                    return (false, "该处方状态不允许收费");

                prescription.Status = (int)PrescriptionStatusEnum.Charged;
                _prescriptionRepo.Update(prescription);
            }

            var chargeNo = GenerateNoHelper.GenerateChargeNo();

            var record = new ChargeRecord
            {
                ChargeNo = chargeNo,
                PatientId = dto.PatientId,
                ChargeType = dto.ChargeType,
                RelatedId = dto.RelatedId,
                TotalAmount = dto.TotalAmount,
                PaidAmount = dto.PaidAmount,
                PaymentMethod = dto.PaymentMethod,
                Status = (int)ChargeStatusEnum.Charged,
                CreateUserId = userId,
                Remark = dto.Remark,
                CreateTime = DateTime.Now
            };

            await _chargeRepo.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"收费成功！单号：{chargeNo}，实收 ¥{dto.PaidAmount:F2}");
        }

        public async Task<(bool Success, string Message)> RefundAsync(long id)
        {
            var record = await _chargeRepo.GetByIdAsync(id);
            if (record == null) return (false, "收费记录不存在");
            if (record.Status == (int)ChargeStatusEnum.Refunded) return (false, "已退费，不能重复操作");

            record.Status = (int)ChargeStatusEnum.Refunded;
            _chargeRepo.Update(record);
            await _unitOfWork.SaveChangesAsync();
            return (true, "退费成功");
        }

        private static ChargeRecordDto MapToDto(ChargeRecord c) => new()
        {
            Id = c.Id, ChargeNo = c.ChargeNo,
            PatientId = c.PatientId, PatientName = c.Patient?.Name,
            ChargeType = c.ChargeType, RelatedId = c.RelatedId,
            TotalAmount = c.TotalAmount, PaidAmount = c.PaidAmount,
            PaymentMethod = c.PaymentMethod, Status = c.Status,
            CreateUserId = c.CreateUserId, CreateUserName = c.CreateUser?.RealName,
            Remark = c.Remark, CreateTime = c.CreateTime
        };
    }
}
