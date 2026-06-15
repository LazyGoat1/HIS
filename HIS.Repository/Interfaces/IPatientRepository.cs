using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 患者信息仓储接口
    /// 继承基础仓储，扩展患者特有的查询方法
    /// </summary>
    public interface IPatientRepository : IBaseRepository<PatientInfo>
    {
        /// <summary>
        /// 分页查询患者列表（支持多条件模糊搜索）
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="keyword">搜索关键词：匹配姓名/手机号/身份证号</param>
        /// <returns>患者列表 + 总记录数</returns>
        Task<(List<PatientInfo> Patients, int Total)> GetPatientListAsync(
            int pageIndex, int pageSize, string? keyword = null);

        /// <summary>
        /// 根据身份证号查找患者（用于查重）
        /// </summary>
        Task<PatientInfo?> GetByIdCardAsync(string idCard);

        /// <summary>
        /// 根据手机号查找患者（用于查重）
        /// </summary>
        Task<PatientInfo?> GetByPhoneAsync(string phone);

        /// <summary>
        /// 根据患者编号查找
        /// </summary>
        Task<PatientInfo?> GetByPatientNoAsync(string patientNo);
    }
}
