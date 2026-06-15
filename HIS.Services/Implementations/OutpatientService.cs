using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 门诊诊疗服务实现
    /// 核心业务逻辑：
    /// 1. 基于挂号记录创建诊疗记录
    /// 2. 同一挂号只能创建一次诊疗记录
    /// 3. 记录主诉、现病史、诊断等信息
    /// 4. 诊疗完成后可开具处方
    /// </summary>
    public class OutpatientService : IOutpatientService
    {
        private readonly IOutpatientRecordRepository _outpatientRepository;
        private readonly IRegistrationRepository _regRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OutpatientService(
            IOutpatientRecordRepository outpatientRepository,
            IRegistrationRepository regRepository,
            IDoctorRepository doctorRepository,
            IUnitOfWork unitOfWork)
        {
            _outpatientRepository = outpatientRepository;
            _regRepository = regRepository;
            _doctorRepository = doctorRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 分页查询门诊记录列表
        /// </summary>
        public async Task<(List<OutpatientRecordDto> Records, int Total)> GetListAsync(
            OutpatientQueryModel query)
        {
            var (records, total) = await _outpatientRepository.GetListAsync(query);
            var dtos = records.Select(MapToDto).ToList();
            return (dtos, total);
        }

        /// <summary>
        /// 根据ID获取门诊记录详情
        /// </summary>
        public async Task<OutpatientRecordDto?> GetByIdAsync(long id)
        {
            var record = await _outpatientRepository.GetDetailAsync(id);
            return record == null ? null : MapToDto(record);
        }

        /// <summary>
        /// 根据挂号ID获取门诊记录
        /// </summary>
        public async Task<OutpatientRecordDto?> GetByRegistrationIdAsync(long registrationId)
        {
            var record = await _outpatientRepository.GetByRegistrationIdAsync(registrationId);
            return record == null ? null : MapToDto(record);
        }

        /// <summary>
        /// 创建门诊诊疗记录
        /// 业务规则：
        /// - 通过系统用户ID查找对应的医生信息
        /// - 挂号必须存在且状态有效
        /// - 同一挂号只能创建一次诊疗记录
        /// - 医生必须匹配挂号医生
        /// </summary>
        public async Task<(bool Success, string Message)> CreateAsync(
            OutpatientRecordCreateDto dto, long userId)
        {
            if (dto == null) return (false, "请求数据为空");
            // 通过系统用户ID查找对应的医生信息
            var doctor = await _doctorRepository.GetByUserIdAsync(userId);
            if (doctor == null)
                return (false, "当前用户不是医生，无法接诊");

            // 校验挂号记录存在
            var reg = await _regRepository.GetByIdAsync(dto.RegistrationId);
            if (reg == null)
                return (false, "挂号记录不存在");

            // 校验是否已创建过诊疗记录
            var existing = await _outpatientRepository.GetByRegistrationIdAsync(dto.RegistrationId);
            if (existing != null)
                return (false, "该挂号已创建过诊疗记录，请勿重复创建");

            // 校验接诊医生
            if (reg.DoctorId != doctor.Id)
                return (false, "只有挂号指定的医生才能接诊");

            var record = new OutpatientRecord
            {
                RegistrationId = dto.RegistrationId,
                PatientId = reg.PatientId,
                DoctorId = doctor.Id,
                ChiefComplaint = dto.ChiefComplaint,
                PresentIllness = dto.PresentIllness,
                PastHistory = dto.PastHistory,
                PhysicalExamination = dto.PhysicalExamination,
                PreliminaryDiagnosis = dto.PreliminaryDiagnosis,
                Advice = dto.Advice,
                VisitTime = DateTime.Now,
                CreateTime = DateTime.Now
            };

            await _outpatientRepository.AddAsync(record);
            await _unitOfWork.SaveChangesAsync();

            return (true, "诊疗记录保存成功");
        }

        #region 私有辅助方法

        private static OutpatientRecordDto MapToDto(OutpatientRecord r) => new()
        {
            Id = r.Id,
            RegistrationId = r.RegistrationId,
            RegistrationNo = r.Registration?.RegistrationNo,
            PatientId = r.PatientId,
            PatientName = r.Patient?.Name,
            DoctorId = r.DoctorId,
            DoctorName = r.Doctor?.Name,
            ChiefComplaint = r.ChiefComplaint,
            PresentIllness = r.PresentIllness,
            PastHistory = r.PastHistory,
            PhysicalExamination = r.PhysicalExamination,
            PreliminaryDiagnosis = r.PreliminaryDiagnosis,
            Advice = r.Advice,
            VisitTime = r.VisitTime,
            CreateTime = r.CreateTime
        };

        #endregion
    }
}
