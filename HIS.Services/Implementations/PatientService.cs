using HIS.Common.Helpers;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 患者信息服务实现
    /// 核心业务逻辑：
    /// 1. 新增时自动生成患者编号
    /// 2. 身份证号/手机号唯一性校验
    /// 3. 根据出生日期自动计算年龄
    /// 4. 删除时检查是否有关联的挂号/门诊/住院记录
    /// </summary>
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IBaseRepository<Registration> _regRepository;
        private readonly IBaseRepository<InpatientRecord> _inpatientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PatientService(
            IPatientRepository patientRepository,
            IBaseRepository<Registration> regRepository,
            IBaseRepository<InpatientRecord> inpatientRepository,
            IUnitOfWork unitOfWork)
        {
            _patientRepository = patientRepository;
            _regRepository = regRepository;
            _inpatientRepository = inpatientRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 分页查询患者列表
        /// </summary>
        public async Task<(List<PatientDto> Patients, int Total)> GetPatientListAsync(
            int pageIndex, int pageSize, string? keyword = null)
        {
            var (patients, total) = await _patientRepository.GetPatientListAsync(pageIndex, pageSize, keyword);
            var dtos = patients.Select(MapToDto).ToList();
            return (dtos, total);
        }

        /// <summary>
        /// 根据ID获取患者详情
        /// </summary>
        public async Task<PatientDto?> GetPatientByIdAsync(long id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            return patient == null ? null : MapToDto(patient);
        }

        /// <summary>
        /// 新增患者
        /// 业务规则：
        /// - 自动生成全局唯一的患者编号
        /// - 身份证号不能与已有患者重复（非空时校验）
        /// - 手机号不能与已有患者重复（非空时校验）
        /// - 提供出生日期时自动计算年龄
        /// </summary>
        public async Task<(bool Success, string Message)> CreatePatientAsync(PatientDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            // 校验：身份证号唯一性（仅当填写了身份证时检查）
            if (!string.IsNullOrEmpty(dto.IdCard))
            {
                var existByIdCard = await _patientRepository.GetByIdCardAsync(dto.IdCard);
                if (existByIdCard != null)
                    return (false, $"身份证号 {dto.IdCard} 已存在，对应患者：{existByIdCard.Name}");
            }

            // 校验：手机号唯一性（仅当填写了手机号时检查）
            if (!string.IsNullOrEmpty(dto.Phone))
            {
                var existByPhone = await _patientRepository.GetByPhoneAsync(dto.Phone);
                if (existByPhone != null)
                    return (false, $"手机号 {dto.Phone} 已存在，对应患者：{existByPhone.Name}");
            }

            // 自动生成患者编号
            var patientNo = GenerateNoHelper.GeneratePatientNo();

            // 自动计算年龄
            int? age = null;
            if (dto.Birthday.HasValue)
            {
                age = CalculateAge(dto.Birthday.Value);
            }

            var patient = new PatientInfo
            {
                PatientNo = patientNo,
                Name = dto.Name,
                Gender = dto.Gender,
                Birthday = dto.Birthday,
                Age = age,
                IdCard = dto.IdCard,
                Phone = dto.Phone,
                Address = dto.Address,
                BloodType = dto.BloodType,
                AllergyHistory = dto.AllergyHistory,
                CreateTime = DateTime.Now
            };

            await _patientRepository.AddAsync(patient);
            await _unitOfWork.SaveChangesAsync();

            return (true, "患者建档成功");
        }

        /// <summary>
        /// 更新患者信息
        /// 校验：身份证号/手机号不能与其他患者冲突
        /// </summary>
        public async Task<(bool Success, string Message)> UpdatePatientAsync(PatientDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            if (dto.Id <= 0) return (false, "患者ID无效");
            var patient = await _patientRepository.GetByIdAsync(dto.Id);
            if (patient == null)
                return (false, "患者不存在");

            // 身份证号唯一性校验（排除自身）
            if (!string.IsNullOrEmpty(dto.IdCard))
            {
                var existByIdCard = await _patientRepository.GetByIdCardAsync(dto.IdCard);
                if (existByIdCard != null && existByIdCard.Id != dto.Id)
                    return (false, $"身份证号 {dto.IdCard} 已被其他患者使用");
            }

            // 手机号唯一性校验（排除自身）
            if (!string.IsNullOrEmpty(dto.Phone))
            {
                var existByPhone = await _patientRepository.GetByPhoneAsync(dto.Phone);
                if (existByPhone != null && existByPhone.Id != dto.Id)
                    return (false, $"手机号 {dto.Phone} 已被其他患者使用");
            }

            // 更新基本信息
            patient.Name = dto.Name;
            patient.Gender = dto.Gender;
            patient.Birthday = dto.Birthday;
            patient.Age = dto.Birthday.HasValue ? CalculateAge(dto.Birthday.Value) : patient.Age;
            patient.IdCard = dto.IdCard;
            patient.Phone = dto.Phone;
            patient.Address = dto.Address;
            patient.BloodType = dto.BloodType;
            patient.AllergyHistory = dto.AllergyHistory;

            _patientRepository.Update(patient);
            await _unitOfWork.SaveChangesAsync();

            return (true, "患者信息更新成功");
        }

        /// <summary>
        /// 删除患者
        /// 业务规则：必须检查是否有关联的挂号/住院记录，有则禁止删除
        /// </summary>
        public async Task<(bool Success, string Message)> DeletePatientAsync(long id)
        {
            var patient = await _patientRepository.GetByIdAsync(id);
            if (patient == null)
                return (false, "患者不存在");

            // 检查是否有关联挂号记录
            var hasRegistration = await _regRepository.AnyAsync(r => r.PatientId == id);
            if (hasRegistration)
                return (false, "该患者存在挂号记录，无法删除。请先处理关联的挂号数据");

            // 检查是否有关联住院记录
            var hasInpatient = await _inpatientRepository.AnyAsync(r => r.PatientId == id);
            if (hasInpatient)
                return (false, "该患者存在住院记录，无法删除。请先处理关联的住院数据");

            _patientRepository.Delete(patient);
            await _unitOfWork.SaveChangesAsync();

            return (true, "患者信息已删除");
        }

        #region 私有辅助方法

        /// <summary>
        /// 将实体映射为DTO
        /// </summary>
        private static PatientDto MapToDto(PatientInfo p) => new()
        {
            Id = p.Id,
            PatientNo = p.PatientNo,
            Name = p.Name,
            Gender = p.Gender,
            Birthday = p.Birthday,
            Age = p.Age,
            IdCard = p.IdCard,
            Phone = p.Phone,
            Address = p.Address,
            BloodType = p.BloodType,
            AllergyHistory = p.AllergyHistory
        };

        /// <summary>
        /// 根据出生日期计算年龄（周岁）
        /// </summary>
        private static int CalculateAge(DateTime birthday)
        {
            var today = DateTime.Today;
            var age = today.Year - birthday.Year;
            // 如果今年生日还没过，年龄减1
            if (birthday.Date > today.AddYears(-age))
                age--;
            return age;
        }

        #endregion
    }
}
