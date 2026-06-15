using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    /// <summary>
    /// 医生信息仓储实现
    /// </summary>
    public class DoctorRepository : BaseRepository<DoctorInfo>, IDoctorRepository
    {
        public DoctorRepository(HisDbContext context) : base(context) { }

        /// <summary>
        /// 分页查询医生列表
        /// 预加载科室导航属性以显示科室名称
        /// </summary>
        public async Task<(List<DoctorInfo> Doctors, int Total)> GetDoctorListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? departmentId = null)
        {
            // 预加载科室信息，用于列表显示科室名
            var query = _dbSet.Include(d => d.Department).AsQueryable();

            // 关键字搜索：医生姓名或工号
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d =>
                    d.Name.Contains(keyword) ||
                    d.DoctorNo.Contains(keyword));
            }

            // 按科室筛选
            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(d => d.DepartmentId == departmentId.Value);
            }

            var total = await query.CountAsync();
            var doctors = await query
                .OrderBy(d => d.DepartmentId)
                .ThenBy(d => d.DoctorNo)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (doctors, total);
        }

        /// <summary>
        /// 根据系统用户ID查找医生
        /// </summary>
        public async Task<DoctorInfo?> GetByUserIdAsync(long userId)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.UserId == userId);
        }

        /// <summary>
        /// 根据医生工号查找
        /// </summary>
        public async Task<DoctorInfo?> GetByDoctorNoAsync(string doctorNo)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.DoctorNo == doctorNo);
        }

        /// <summary>
        /// 获取所有在岗医生
        /// 用于挂号/门诊等场景的医生下拉选择
        /// </summary>
        public async Task<List<DoctorInfo>> GetAvailableDoctorsAsync(long? departmentId = null)
        {
            var query = _dbSet
                .Include(d => d.Department)
                .Where(d => d.Status == 1)  // 仅显示在岗医生
                .AsQueryable();

            if (departmentId.HasValue && departmentId.Value > 0)
            {
                query = query.Where(d => d.DepartmentId == departmentId.Value);
            }

            return await query.OrderBy(d => d.DepartmentId).ThenBy(d => d.DoctorNo).ToListAsync();
        }
    }
}
