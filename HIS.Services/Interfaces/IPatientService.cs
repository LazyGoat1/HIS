using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 患者信息服务接口
    /// </summary>
    public interface IPatientService
    {
        /// <summary>分页查询患者列表</summary>
        Task<(List<PatientDto> Patients, int Total)> GetPatientListAsync(
            int pageIndex, int pageSize, string? keyword = null);

        /// <summary>根据ID获取患者详细信息</summary>
        Task<PatientDto?> GetPatientByIdAsync(long id);

        /// <summary>
        /// 新增患者
        /// 自动生成患者编号，校验身份证/手机号唯一性
        /// </summary>
        Task<(bool Success, string Message)> CreatePatientAsync(PatientDto dto);

        /// <summary>更新患者信息</summary>
        Task<(bool Success, string Message)> UpdatePatientAsync(PatientDto dto);

        /// <summary>删除患者（物理删除，需检查关联数据）</summary>
        Task<(bool Success, string Message)> DeletePatientAsync(long id);
    }
}
