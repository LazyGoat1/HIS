using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    /// <summary>
    /// 门诊诊疗记录仓储实现
    /// </summary>
    public class OutpatientRecordRepository : BaseRepository<OutpatientRecord>, IOutpatientRecordRepository
    {
        public OutpatientRecordRepository(HisDbContext context) : base(context) { }

        /// <summary>
        /// 分页查询门诊记录列表
        /// </summary>
        public async Task<(List<OutpatientRecord> Records, int Total)> GetListAsync(
            OutpatientQueryModel query)
        {
            var q = _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Registration)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                q = q.Where(r =>
                    (r.Patient != null && r.Patient.Name.Contains(query.Keyword)) ||
                    (r.Registration != null && r.Registration.RegistrationNo.Contains(query.Keyword)));
            }

            if (query.VisitDateStart.HasValue)
                q = q.Where(r => r.VisitTime >= query.VisitDateStart.Value);

            if (query.VisitDateEnd.HasValue)
                q = q.Where(r => r.VisitTime <= query.VisitDateEnd.Value);

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
        /// 根据挂号ID获取门诊记录
        /// </summary>
        public async Task<OutpatientRecord?> GetByRegistrationIdAsync(long registrationId)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .FirstOrDefaultAsync(r => r.RegistrationId == registrationId);
        }

        /// <summary>
        /// 获取门诊记录详情
        /// </summary>
        public async Task<OutpatientRecord?> GetDetailAsync(long id)
        {
            return await _dbSet
                .Include(r => r.Patient)
                .Include(r => r.Doctor)
                .Include(r => r.Registration)
                .ThenInclude(reg => reg!.Department)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}
