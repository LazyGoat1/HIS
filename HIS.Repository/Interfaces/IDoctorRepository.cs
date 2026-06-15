using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 医生信息仓储接口
    /// </summary>
    public interface IDoctorRepository : IBaseRepository<DoctorInfo>
    {
        /// <summary>
        /// 分页查询医生列表（含科室导航属性）
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="keyword">搜索关键词：医生姓名/工号</param>
        /// <param name="departmentId">科室筛选（可选）</param>
        Task<(List<DoctorInfo> Doctors, int Total)> GetDoctorListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? departmentId = null);

        /// <summary>
        /// 根据系统用户ID查找医生
        /// </summary>
        Task<DoctorInfo?> GetByUserIdAsync(long userId);

        /// <summary>
        /// 根据医生工号查找
        /// </summary>
        Task<DoctorInfo?> GetByDoctorNoAsync(string doctorNo);

        /// <summary>
        /// 获取所有在岗医生（用于下拉选择）
        /// </summary>
        Task<List<DoctorInfo>> GetAvailableDoctorsAsync(long? departmentId = null);
    }
}
