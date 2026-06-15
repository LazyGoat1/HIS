using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 挂号服务接口
    /// </summary>
    public interface IRegistrationService
    {
        /// <summary>分页查询挂号列表</summary>
        Task<(List<RegistrationDto> List, int Total)> GetListAsync(RegistrationQueryModel query);

        /// <summary>根据ID获取挂号详情</summary>
        Task<RegistrationDto?> GetByIdAsync(long id);

        /// <summary>新增挂号（自动生成挂号单号、排队号）</summary>
        Task<(bool Success, string Message)> CreateAsync(RegistrationDto dto, long createUserId);

        /// <summary>退号</summary>
        Task<(bool Success, string Message)> RefundAsync(long id);

        /// <summary>接诊（状态改为已接诊）</summary>
        Task<(bool Success, string Message)> AcceptAsync(long id, long doctorId);

        /// <summary>获取今日挂号列表（医生接诊列表）</summary>
        Task<List<RegistrationDto>> GetTodayListAsync(long? doctorId = null, long? departmentId = null);
    }
}
