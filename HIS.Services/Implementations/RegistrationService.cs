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
    /// 挂号服务实现
    /// 核心业务逻辑：
    /// 1. 自动生成挂号单号（GH+日期+流水）
    /// 2. 自动生成排队序号
    /// 3. 同一患者同一天同一科室同一医生只能挂一次
    /// 4. 根据挂号类型和医生设置自动计算挂号费
    /// 5. 退号校验：已接诊的不能退号
    /// </summary>
    public class RegistrationService : IRegistrationService
    {
        private readonly IRegistrationRepository _regRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IBaseRepository<OutpatientRecord> _outpatientRepository;
        private readonly IBaseRepository<ChargeRecord> _chargeRepository;
        private readonly IUnitOfWork _unitOfWork;

        public RegistrationService(
            IRegistrationRepository regRepository,
            IDoctorRepository doctorRepository,
            IPatientRepository patientRepository,
            IBaseRepository<OutpatientRecord> outpatientRepository,
            IBaseRepository<ChargeRecord> chargeRepository,
            IUnitOfWork unitOfWork)
        {
            _regRepository = regRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _outpatientRepository = outpatientRepository;
            _chargeRepository = chargeRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 分页查询挂号列表
        /// </summary>
        public async Task<(List<RegistrationDto> List, int Total)> GetListAsync(
            RegistrationQueryModel query)
        {
            var (list, total) = await _regRepository.GetListAsync(query);
            var dtos = list.Select(MapToDto).ToList();
            return (dtos, total);
        }

        /// <summary>
        /// 根据ID获取挂号详情
        /// </summary>
        public async Task<RegistrationDto?> GetByIdAsync(long id)
        {
            var reg = await _regRepository.GetDetailAsync(id);
            return reg == null ? null : MapToDto(reg);
        }

        /// <summary>
        /// 新增挂号
        /// 业务规则：
        /// - 自动生成挂号单号
        /// - 自动生成排队号（该科室当天最大号+1）
        /// - 检查重复挂号
        /// - 根据挂号类型和医生自动计算费用
        /// </summary>
        public async Task<(bool Success, string Message)> CreateAsync(
            RegistrationDto dto, long createUserId)
        {
            if (dto == null) return (false, "请求数据为空");
            // 校验患者存在
            var patient = await _patientRepository.GetByIdAsync(dto.PatientId);
            if (patient == null)
                return (false, "患者不存在");

            // 校验医生存在且在岗
            var doctor = await _doctorRepository.GetByIdAsync(dto.DoctorId);
            if (doctor == null)
                return (false, "医生不存在");
            if (doctor.Status != 1)
                return (false, "该医生当前不在岗，请选择其他医生");

            // 校验是否重复挂号（同一天同一科室同一医生）
            var visitDate = dto.VisitDate.Date;
            var exists = await _regRepository.ExistsTodayAsync(
                dto.PatientId, dto.DepartmentId, dto.DoctorId, visitDate);
            if (exists)
                return (false, "该患者今日已在此科室挂过该医生的号，请勿重复挂号");

            // 自动生成挂号单号
            var regNo = GenerateNoHelper.GenerateRegistrationNo();

            // 自动生成排队号
            var maxQueue = await _regRepository.GetMaxQueueNumberAsync(
                dto.DepartmentId, visitDate);
            var queueNumber = maxQueue + 1;

            // 计算挂号费：优先使用医生设置的诊疗费，否则按挂号类型计算
            var fee = doctor.ConsultationFee > 0
                ? doctor.ConsultationFee
                : dto.RegistrationType switch
                {
                    (int)RegistrationTypeEnum.Expert => 20m,
                    (int)RegistrationTypeEnum.Emergency => 15m,
                    _ => 10m  // 普通
                };

            var reg = new Registration
            {
                RegistrationNo = regNo,
                PatientId = dto.PatientId,
                DepartmentId = dto.DepartmentId,
                DoctorId = dto.DoctorId,
                RegistrationType = dto.RegistrationType,
                RegistrationFee = fee,
                Status = (int)RegistrationStatusEnum.Registered,
                VisitDate = visitDate,
                QueueNumber = queueNumber,
                CreateUserId = createUserId,
                CreateTime = DateTime.Now
            };

            await _regRepository.AddAsync(reg);
            // 挂号费自动生成收费记录
            await _chargeRepository.AddAsync(new ChargeRecord
            {
                ChargeNo = GenerateNoHelper.GenerateChargeNo(),
                PatientId = dto.PatientId,
                ChargeType = (int)ChargeTypeEnum.Registration,
                RelatedId = reg.Id,
                TotalAmount = fee,
                PaidAmount = fee,
                PaymentMethod = (int)PaymentMethodEnum.Cash,
                Status = (int)ChargeStatusEnum.Charged,
                CreateUserId = createUserId,
                CreateTime = DateTime.Now,
                Remark = $"挂号费 - {regNo}"
            });
            await _unitOfWork.SaveChangesAsync();

            return (true, $"挂号成功！挂号单号：{regNo}，排队号：{queueNumber}");
        }

        /// <summary>
        /// 退号
        /// 业务规则：已接诊的挂号不能退
        /// </summary>
        public async Task<(bool Success, string Message)> RefundAsync(long id)
        {
            var reg = await _regRepository.GetByIdAsync(id);
            if (reg == null)
                return (false, "挂号记录不存在");

            if (reg.Status == (int)RegistrationStatusEnum.Refunded)
                return (false, "该挂号已退号，请勿重复操作");

            if (reg.Status == (int)RegistrationStatusEnum.Consulted)
                return (false, "该挂号已接诊，无法退号");

            reg.Status = (int)RegistrationStatusEnum.Refunded;
            _regRepository.Update(reg);

            // 关联的挂号费也标记退费
            var charge = await _chargeRepository.FirstOrDefaultAsync(
                c => c.ChargeType == (int)ChargeTypeEnum.Registration && c.RelatedId == id);
            if (charge != null && charge.Status == (int)ChargeStatusEnum.Charged)
            {
                charge.Status = (int)ChargeStatusEnum.Refunded;
                _chargeRepository.Update(charge);
            }

            await _unitOfWork.SaveChangesAsync();

            return (true, "退号成功，挂号费已退");
        }

        /// <summary>
        /// 接诊（医生确认接诊，状态：已挂号→已接诊）
        /// </summary>
        public async Task<(bool Success, string Message)> AcceptAsync(long id, long doctorId)
        {
            var reg = await _regRepository.GetByIdAsync(id);
            if (reg == null)
                return (false, "挂号记录不存在");

            if (reg.Status != (int)RegistrationStatusEnum.Registered)
                return (false, "该挂号状态不允许接诊");

            if (reg.DoctorId != doctorId)
                return (false, "该挂号不属于当前医生");

            reg.Status = (int)RegistrationStatusEnum.Consulted;
            _regRepository.Update(reg);
            await _unitOfWork.SaveChangesAsync();

            return (true, "接诊成功");
        }

        /// <summary>
        /// 获取今日挂号列表（医生叫号/接诊用）
        /// </summary>
        public async Task<List<RegistrationDto>> GetTodayListAsync(
            long? doctorId = null, long? departmentId = null)
        {
            var query = new RegistrationQueryModel
            {
                VisitDateStart = DateTime.Today,
                VisitDateEnd = DateTime.Today,
                DoctorId = doctorId,
                DepartmentId = departmentId,
                Status = (int)RegistrationStatusEnum.Registered,  // 只看待接诊
                PageIndex = 1,
                PageSize = 200  // 一天内最多200号
            };

            var (list, _) = await _regRepository.GetListAsync(query);
            return list.Select(MapToDto).ToList();
        }

        #region 私有辅助方法

        private static RegistrationDto MapToDto(Registration r) => new()
        {
            Id = r.Id,
            RegistrationNo = r.RegistrationNo,
            PatientId = r.PatientId,
            PatientName = r.Patient?.Name,
            DepartmentId = r.DepartmentId,
            DepartmentName = r.Department?.DeptName,
            DoctorId = r.DoctorId,
            DoctorName = r.Doctor?.Name,
            RegistrationType = r.RegistrationType,
            RegistrationFee = r.RegistrationFee,
            Status = r.Status,
            VisitDate = r.VisitDate,
            QueueNumber = r.QueueNumber
        };

        #endregion
    }
}
