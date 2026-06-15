using HIS.Common.Helpers;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 医生信息服务实现
    /// 核心业务逻辑：
    /// 1. 新增时自动生成医生工号（YS0001 格式递增）
    /// 2. 自动创建系统登录账号（用户名=工号，默认密码123456）
    /// 3. 关联科室信息用于出诊排班
    /// 4. 删除前检查是否有关联挂号/门诊记录
    /// </summary>
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;
        private readonly ISysUserRepository _userRepository;
        private readonly IBaseRepository<Registration> _regRepository;
        private readonly IBaseRepository<OutpatientRecord> _outpatientRepository;
        private readonly IBaseRepository<InpatientRecord> _inpatientRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DoctorService(
            IDoctorRepository doctorRepository,
            ISysUserRepository userRepository,
            IBaseRepository<Registration> regRepository,
            IBaseRepository<OutpatientRecord> outpatientRepository,
            IBaseRepository<InpatientRecord> inpatientRepository,
            IUnitOfWork unitOfWork)
        {
            _doctorRepository = doctorRepository;
            _userRepository = userRepository;
            _regRepository = regRepository;
            _outpatientRepository = outpatientRepository;
            _inpatientRepository = inpatientRepository;
            _unitOfWork = unitOfWork;
        }

        /// <summary>
        /// 分页查询医生列表
        /// </summary>
        public async Task<(List<DoctorDto> Doctors, int Total)> GetDoctorListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? departmentId = null)
        {
            var (doctors, total) = await _doctorRepository.GetDoctorListAsync(
                pageIndex, pageSize, keyword, departmentId);
            var dtos = doctors.Select(MapToDto).ToList();
            return (dtos, total);
        }

        /// <summary>
        /// 根据ID获取医生详情
        /// </summary>
        public async Task<DoctorDto?> GetDoctorByIdAsync(long id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            return doctor == null ? null : MapToDto(doctor);
        }

        /// <summary>
        /// 获取在岗医生列表（挂号/门诊选择医生时使用）
        /// </summary>
        public async Task<List<DoctorDto>> GetAvailableDoctorsAsync(long? departmentId = null)
        {
            var doctors = await _doctorRepository.GetAvailableDoctorsAsync(departmentId);
            return doctors.Select(MapToDto).ToList();
        }

        /// <summary>
        /// 新增医生
        /// 业务规则：
        /// - 自动生成工号（格式：YS + 4位流水号）
        /// - 工号全局唯一
        /// - 可选择性关联系统用户账号
        /// - 设置初始状态为在岗
        /// </summary>
        public async Task<(bool Success, string Message)> CreateDoctorAsync(DoctorDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            // 生成工号
            var currentCount = await _doctorRepository.CountAsync();
            var doctorNo = GenerateNoHelper.GenerateDoctorNo(currentCount);

            var existByNo = await _doctorRepository.GetByDoctorNoAsync(doctorNo);
            if (existByNo != null)
                return (false, $"工号生成冲突，请重试");

            // 自动创建系统登录账号（用户名=工号，默认密码=123456，角色=医生 RoleId=2）
            var salt = EncryptionHelper.GenerateSalt();
            var sysUser = new SysUser
            {
                UserName = doctorNo,
                Password = EncryptionHelper.HashPassword("123456", salt),
                Salt = salt,
                RealName = dto.Name,
                Gender = dto.Gender,
                Phone = null,
                RoleId = 2, // doctor 角色（DbInitializer 种子数据固定 ID=2）
                DepartmentId = dto.DepartmentId,
                Status = 1,
                CreateTime = DateTime.Now
            };

            await _userRepository.AddAsync(sysUser);
            await _unitOfWork.SaveChangesAsync(); // 先保存用户以获取 Id

            var doctor = new DoctorInfo
            {
                DoctorNo = doctorNo,
                Name = dto.Name,
                Gender = dto.Gender,
                DepartmentId = dto.DepartmentId,
                Title = dto.Title,
                Specialty = dto.Specialty,
                MaxDailyPatients = dto.MaxDailyPatients,
                ConsultationFee = dto.ConsultationFee,
                Status = 1,
                UserId = sysUser.Id, // 绑定刚创建的系统账号
                CreateTime = DateTime.Now
            };

            await _doctorRepository.AddAsync(doctor);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"医生添加成功！工号：{doctorNo}，登录账号：{doctorNo}，默认密码：123456");
        }

        /// <summary>
        /// 更新医生信息
        /// </summary>
        public async Task<(bool Success, string Message)> UpdateDoctorAsync(DoctorDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var doctor = await _doctorRepository.GetByIdAsync(dto.Id);
            if (doctor == null)
                return (false, "医生不存在");

            // 如果修改了关联用户，检查冲突
            if (dto.UserId.HasValue && dto.UserId.Value > 0 && dto.UserId != doctor.UserId)
            {
                var existByUser = await _doctorRepository.GetByUserIdAsync(dto.UserId.Value);
                if (existByUser != null && existByUser.Id != dto.Id)
                    return (false, "该系统用户已绑定其他医生");
            }

            doctor.Name = dto.Name;
            doctor.Gender = dto.Gender;
            doctor.DepartmentId = dto.DepartmentId;
            doctor.Title = dto.Title;
            doctor.Specialty = dto.Specialty;
            doctor.MaxDailyPatients = dto.MaxDailyPatients;
            doctor.ConsultationFee = dto.ConsultationFee;
            doctor.UserId = dto.UserId;

            _doctorRepository.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            return (true, "医生信息更新成功");
        }

        /// <summary>
        /// 删除医生
        /// 业务规则：检查是否有关联的挂号/门诊/住院记录
        /// </summary>
        public async Task<(bool Success, string Message)> DeleteDoctorAsync(long id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null)
                return (false, "医生不存在");

            // 检查是否有关联的挂号记录
            if (await _regRepository.AnyAsync(r => r.DoctorId == id))
                return (false, "该医生存在挂号记录，无法删除");

            // 检查是否有关联的门诊记录
            if (await _outpatientRepository.AnyAsync(r => r.DoctorId == id))
                return (false, "该医生存在门诊诊疗记录，无法删除");

            // 检查是否有负责的住院患者
            if (await _inpatientRepository.AnyAsync(r => r.DoctorId == id))
                return (false, "该医生存在住院负责记录，无法删除");

            _doctorRepository.Delete(doctor);
            await _unitOfWork.SaveChangesAsync();

            return (true, "医生信息已删除");
        }

        /// <summary>
        /// 切换医生在岗/休假状态
        /// </summary>
        public async Task<(bool Success, string Message)> ToggleStatusAsync(long id)
        {
            var doctor = await _doctorRepository.GetByIdAsync(id);
            if (doctor == null)
                return (false, "医生不存在");

            // 1→0（在岗→休假）, 0→1（休假→在岗）
            doctor.Status = doctor.Status == 1 ? 0 : 1;
            _doctorRepository.Update(doctor);
            await _unitOfWork.SaveChangesAsync();

            var statusText = doctor.Status == 1 ? "在岗" : "休假";
            return (true, $"医生状态已切换为：{statusText}");
        }

        #region 私有辅助方法

        /// <summary>
        /// 实体→DTO映射
        /// 包含科室名称的额外信息
        /// </summary>
        private static DoctorDto MapToDto(DoctorInfo d) => new()
        {
            Id = d.Id,
            UserId = d.UserId,
            DoctorNo = d.DoctorNo,
            Name = d.Name,
            Gender = d.Gender,
            DepartmentId = d.DepartmentId,
            DepartmentName = d.Department?.DeptName,
            Title = d.Title,
            Specialty = d.Specialty,
            MaxDailyPatients = d.MaxDailyPatients,
            ConsultationFee = d.ConsultationFee,
            Status = d.Status,
            CreateTime = d.CreateTime
        };

        #endregion
    }
}
