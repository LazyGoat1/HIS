using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    public interface ISysDepartmentRepository : IBaseRepository<SysDepartment>
    {
        /// <summary>获取科室列表</summary>
        Task<(List<SysDepartment> Departments, int Total)> GetDepartmentListAsync(int pageIndex, int pageSize, string? keyword = null);

        /// <summary>获取所有启用科室</summary>
        Task<List<SysDepartment>> GetAllEnabledAsync();

        /// <summary>获取科室树</summary>
        Task<List<SysDepartment>> GetDepartmentTreeAsync();
    }
}
