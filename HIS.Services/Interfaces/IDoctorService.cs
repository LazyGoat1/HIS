using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 医生信息服务接口
    /// </summary>
    public interface IDoctorService
    {
        /// <summary>分页查询医生列表</summary>
        Task<(List<DoctorDto> Doctors, int Total)> GetDoctorListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? departmentId = null);

        /// <summary>根据ID获取医生详情</summary>
        Task<DoctorDto?> GetDoctorByIdAsync(long id);

        /// <summary>获取在岗医生列表（用于下拉选择）</summary>
        Task<List<DoctorDto>> GetAvailableDoctorsAsync(long? departmentId = null);

        /// <summary>新增医生（自动生成工号）</summary>
        Task<(bool Success, string Message)> CreateDoctorAsync(DoctorDto dto);

        /// <summary>更新医生信息</summary>
        Task<(bool Success, string Message)> UpdateDoctorAsync(DoctorDto dto);

        /// <summary>删除医生（检查关联数据）</summary>
        Task<(bool Success, string Message)> DeleteDoctorAsync(long id);

        /// <summary>切换医生在岗/休假状态</summary>
        Task<(bool Success, string Message)> ToggleStatusAsync(long id);
    }
}
