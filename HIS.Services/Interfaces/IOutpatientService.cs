using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 门诊诊疗服务接口
    /// </summary>
    public interface IOutpatientService
    {
        /// <summary>分页查询门诊记录列表</summary>
        Task<(List<OutpatientRecordDto> Records, int Total)> GetListAsync(OutpatientQueryModel query);

        /// <summary>根据ID获取门诊记录详情</summary>
        Task<OutpatientRecordDto?> GetByIdAsync(long id);

        /// <summary>根据挂号ID获取门诊记录</summary>
        Task<OutpatientRecordDto?> GetByRegistrationIdAsync(long registrationId);

        /// <summary>创建门诊诊疗记录（医生接诊后填写）</summary>
        Task<(bool Success, string Message)> CreateAsync(OutpatientRecordCreateDto dto, long userId);
    }
}
