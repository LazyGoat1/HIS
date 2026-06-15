using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    /// <summary>
    /// 挂号记录仓储实现
    /// </summary>
    public class RegistrationRepository : BaseRepository<Registration>, IRegistrationRepository
    {
        public RegistrationRepository(HisDbContext context) : base(context) { }

        /// <summary>
        /// 分页查询挂号列表
        /// </summary>
        public async Task<(List<Registration> Registrations, int Total)> GetListAsync(
            RegistrationQueryModel query)
        {
            var q = _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Department)
                .Include(r => r.Doctor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                q = q.Where(r =>
                    r.RegistrationNo.Contains(query.Keyword) ||
                    (r.Patient != null && r.Patient.Name.Contains(query.Keyword)));
            }

            if (query.VisitDateStart.HasValue)
                q = q.Where(r => r.VisitDate >= query.VisitDateStart.Value);

            if (query.VisitDateEnd.HasValue)
                q = q.Where(r => r.VisitDate <= query.VisitDateEnd.Value);

            if (query.RegistrationType.HasValue)
                q = q.Where(r => r.RegistrationType == query.RegistrationType.Value);

            if (query.Status.HasValue)
                q = q.Where(r => r.Status == query.Status.Value);

            if (query.DepartmentId.HasValue)
                q = q.Where(r => r.DepartmentId == query.DepartmentId.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(r => r.DoctorId == query.DoctorId.Value);

            var total = await q.CountAsync();
            var list = await q
                .OrderByDescending(r => r.CreateTime)
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (list, total);
        }

        /// <summary>
        /// 根据挂号单号查找
        /// </summary>
        public async Task<Registration?> GetByRegistrationNoAsync(string registrationNo)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.RegistrationNo == registrationNo);
        }

        /// <summary>
        /// 检查同一患者当天是否已在同一科室同一医生处挂号
        /// </summary>
        public async Task<bool> ExistsTodayAsync(
            long patientId, long departmentId, long doctorId, DateTime visitDate)
        {
            return await _dbSet.AnyAsync(r =>
                r.PatientId == patientId &&
                r.DepartmentId == departmentId &&
                r.DoctorId == doctorId &&
                r.VisitDate.Date == visitDate.Date &&
                r.Status != (int)HIS.Common.Enums.RegistrationStatusEnum.Refunded);
        }

        /// <summary>
        /// 获取指定日期科室的最大排队号
        /// </summary>
        public async Task<int> GetMaxQueueNumberAsync(long departmentId, DateTime visitDate)
        {
            var max = await _dbSet
                .Where(r => r.DepartmentId == departmentId && r.VisitDate.Date == visitDate.Date)
                .MaxAsync(r => (int?)r.QueueNumber) ?? 0;
            return max;
        }

        /// <summary>
        /// 获取挂号详情（包含所有导航属性）
        /// </summary>
        public async Task<Registration?> GetDetailAsync(long id)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Department)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
