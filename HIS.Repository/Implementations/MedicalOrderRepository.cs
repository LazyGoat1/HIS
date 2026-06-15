using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class MedicalOrderRepository : BaseRepository<MedicalOrder>, IMedicalOrderRepository
    {
        public MedicalOrderRepository(HisDbContext context) : base(context) { }

        public async Task<(List<MedicalOrder> List, int Total)> GetListAsync(MedicalOrderQueryModel query)
        {
            var q = _dbSet
                .Include(o => o.Patient).Include(o => o.Doctor)
                .Include(o => o.InpatientRecord)
                .AsQueryable();

            if (query.InpatientId.HasValue)
                q = q.Where(o => o.InpatientId == query.InpatientId.Value);

            if (!string.IsNullOrEmpty(query.Keyword))
                q = q.Where(o => o.OrderContent.Contains(query.Keyword) ||
                    (o.Patient != null && o.Patient.Name.Contains(query.Keyword)));

            if (query.OrderType.HasValue)
                q = q.Where(o => o.OrderType == query.OrderType.Value);

            if (query.Status.HasValue)
                q = q.Where(o => o.Status == query.Status.Value);

            if (query.DoctorId.HasValue)
                q = q.Where(o => o.DoctorId == query.DoctorId.Value);

            var total = await q.CountAsync();
            var list = await q.OrderByDescending(o => o.CreateTime)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return (list, total);
        }

        public async Task<List<MedicalOrder>> GetByInpatientIdAsync(long inpatientId)
        {
            return await _dbSet
                .Include(o => o.Doctor).Include(o => o.Patient)
                .Where(o => o.InpatientId == inpatientId)
                .OrderByDescending(o => o.CreateTime).ToListAsync();
        }

        public async Task<MedicalOrder?> GetDetailAsync(long id)
        {
            return await _dbSet
                .Include(o => o.Patient).Include(o => o.Doctor)
                .Include(o => o.InpatientRecord)
                .FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
