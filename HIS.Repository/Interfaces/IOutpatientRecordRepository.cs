using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 门诊诊疗记录仓储接口
    /// </summary>
    public interface IOutpatientRecordRepository : IBaseRepository<OutpatientRecord>
    {
        /// <summary>
        /// 分页查询门诊记录列表
        /// </summary>
        Task<(List<OutpatientRecord> Records, int Total)> GetListAsync(OutpatientQueryModel query);

        /// <summary>
        /// 根据挂号ID获取门诊记录
        /// </summary>
        Task<OutpatientRecord?> GetByRegistrationIdAsync(long registrationId);

        /// <summary>
        /// 获取门诊记录详情（包含患者/医生/挂号信息）
        /// </summary>
        Task<OutpatientRecord?> GetDetailAsync(long id);
    }
}
