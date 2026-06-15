using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 挂号记录仓储接口
    /// </summary>
    public interface IRegistrationRepository : IBaseRepository<Registration>
    {
        /// <summary>
        /// 分页查询挂号列表（支持多条件筛选）
        /// </summary>
        Task<(List<Registration> Registrations, int Total)> GetListAsync(RegistrationQueryModel query);

        /// <summary>
        /// 根据挂号单号查找
        /// </summary>
        Task<Registration?> GetByRegistrationNoAsync(string registrationNo);

        /// <summary>
        /// 获取患者今日是否已挂号（同一科室同一医生只能挂一次）
        /// </summary>
        Task<bool> ExistsTodayAsync(long patientId, long departmentId, long doctorId, DateTime visitDate);

        /// <summary>
        /// 获取指定日期的最大排队号
        /// </summary>
        Task<int> GetMaxQueueNumberAsync(long departmentId, DateTime visitDate);

        /// <summary>
        /// 获取挂号详情（包含患者/科室/医生信息）
        /// </summary>
        Task<Registration?> GetDetailAsync(long id);
    }
}
