using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class DrugStockRepository : BaseRepository<DrugStockLog>, IDrugStockRepository
    {
        public DrugStockRepository(HisDbContext context) : base(context) { }

        public async Task<(List<DrugStockLog> List, int Total)> GetListAsync(DrugStockQueryModel query)
        {
            var q = _dbSet.Include(s => s.Drug).AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
                q = q.Where(s => (s.Drug != null && s.Drug.DrugName.Contains(query.Keyword))
                    || s.RelatedNo!.Contains(query.Keyword));

            if (query.DrugId.HasValue)
                q = q.Where(s => s.DrugId == query.DrugId.Value);

            if (query.ChangeType.HasValue)
                q = q.Where(s => s.ChangeType == query.ChangeType.Value);

            if (query.StartDate.HasValue)
                q = q.Where(s => s.CreateTime >= query.StartDate.Value);
            if (query.EndDate.HasValue)
                q = q.Where(s => s.CreateTime <= query.EndDate.Value.AddDays(1));

            var total = await q.CountAsync();
            var list = await q.OrderByDescending(s => s.CreateTime)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return (list, total);
        }
    }
}
