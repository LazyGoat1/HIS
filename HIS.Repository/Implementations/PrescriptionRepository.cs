using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    /// <summary>
    /// 处方仓储实现
    /// </summary>
    public class PrescriptionRepository : BaseRepository<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(HisDbContext context) : base(context) { }

        /// <summary>
        /// 分页查询处方列表
        /// </summary>
        public async Task<(List<Prescription> Prescriptions, int Total)> GetListAsync(
            PrescriptionQueryModel query)
        {
            var q = _dbSet
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
            {
                q = q.Where(p =>
                    p.PrescriptionNo.Contains(query.Keyword) ||
                    (p.Patient != null && p.Patient.Name.Contains(query.Keyword)));
            }

            if (query.PrescriptionType.HasValue)
                q = q.Where(p => p.PrescriptionType == query.PrescriptionType.Value);

            if (query.Status.HasValue)
                q = q.Where(p => p.Status == query.Status.Value);

            if (query.StartDate.HasValue)
                q = q.Where(p => p.CreateTime >= query.StartDate.Value);

            if (query.EndDate.HasValue)
                q = q.Where(p => p.CreateTime <= query.EndDate.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(p => p.DoctorId == query.DoctorId.Value);

            var total = await q.CountAsync();
            var list = await q
                .OrderByDescending(p => p.CreateTime)
                .Skip((query.PageIndex - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            return (list, total);
        }

        /// <summary>
        /// 根据处方号查找
        /// </summary>
        public async Task<Prescription?> GetByPrescriptionNoAsync(string prescriptionNo)
        {
            return await _dbSet.FirstOrDefaultAsync(p => p.PrescriptionNo == prescriptionNo);
        }

        /// <summary>
        /// 获取处方详情（包含明细）
        /// </summary>
        public async Task<Prescription?> GetDetailAsync(long id)
        {
            return await _dbSet
                .Include(p => p.Patient)
                .Include(p => p.Doctor)
                .Include(p => p.Details)
                .Include(p => p.Registration)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// 更新处方状态（原子操作）
        /// </summary>
        public async Task UpdateStatusAsync(long id, int status)
        {
            var p = await _dbSet.FirstOrDefaultAsync(p => p.Id == id);
            if (p != null) { p.Status = status; await _context.SaveChangesAsync(); }
        }
    }
}
