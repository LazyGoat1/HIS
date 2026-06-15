using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class ChargeRecordRepository : BaseRepository<ChargeRecord>, IChargeRecordRepository
    {
        public ChargeRecordRepository(HisDbContext context) : base(context) { }

        public async Task<(List<ChargeRecord> List, int Total)> GetListAsync(ChargeQueryModel query)
        {
            var q = _dbSet.Include(c => c.Patient).Include(c => c.CreateUser).AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
                q = q.Where(c => c.ChargeNo.Contains(query.Keyword) ||
                    (c.Patient != null && c.Patient.Name.Contains(query.Keyword)));

            if (query.ChargeType.HasValue) q = q.Where(c => c.ChargeType == query.ChargeType.Value);
            if (query.Status.HasValue) q = q.Where(c => c.Status == query.Status.Value);
            if (query.PaymentMethod.HasValue) q = q.Where(c => c.PaymentMethod == query.PaymentMethod.Value);
            if (query.StartDate.HasValue) q = q.Where(c => c.CreateTime >= query.StartDate.Value);
            if (query.EndDate.HasValue) q = q.Where(c => c.CreateTime <= query.EndDate.Value.AddDays(1));

            var total = await q.CountAsync();
            var list = await q.OrderByDescending(c => c.CreateTime)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return (list, total);
        }

        public async Task<ChargeRecord?> GetByChargeNoAsync(string chargeNo)
        {
            return await _dbSet.FirstOrDefaultAsync(c => c.ChargeNo == chargeNo);
        }

        public async Task<ChargeRecord?> GetDetailAsync(long id)
        {
            return await _dbSet.Include(c => c.Patient).Include(c => c.CreateUser)
                .FirstOrDefaultAsync(c => c.Id == id);
        }
    }
}
