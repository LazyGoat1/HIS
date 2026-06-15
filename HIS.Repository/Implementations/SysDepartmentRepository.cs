using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class SysDepartmentRepository : BaseRepository<SysDepartment>, ISysDepartmentRepository
    {
        public SysDepartmentRepository(HisDbContext context) : base(context) { }

        public async Task<(List<SysDepartment> Departments, int Total)> GetDepartmentListAsync(
            int pageIndex, int pageSize, string? keyword = null)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(d => d.DeptName.Contains(keyword) || d.DeptCode.Contains(keyword));

            var total = await query.CountAsync();
            var departments = await query.OrderBy(d => d.SortOrder).ThenByDescending(d => d.CreateTime)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return (departments, total);
        }

        public async Task<List<SysDepartment>> GetAllEnabledAsync()
        {
            return await _dbSet
                .Where(d => d.Status == 1)
                .OrderBy(d => d.SortOrder)
                .ToListAsync();
        }

        public async Task<List<SysDepartment>> GetDepartmentTreeAsync()
        {
            return await _dbSet
                .Where(d => d.Status == 1)
                .OrderBy(d => d.SortOrder)
                .ToListAsync();
        }
    }
}
