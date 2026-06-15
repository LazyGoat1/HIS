using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class ChargeItemRepository : BaseRepository<ChargeItem>, IChargeItemRepository
    {
        public ChargeItemRepository(HisDbContext context) : base(context) { }

        public async Task<List<ChargeItem>> GetAllEnabledAsync()
        {
            return await _dbSet.Where(c => c.Status == 1)
                .OrderByDescending(c => c.CreateTime).ToListAsync();
        }

        public async Task<(List<ChargeItem> List, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null)
        {
            var q = _dbSet.Where(c => c.Status == 1).AsQueryable();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(c => c.ItemName.Contains(keyword) || c.Category.Contains(keyword));
            var total = await q.CountAsync();
            var list = await q.OrderByDescending(c => c.CreateTime)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();
            return (list, total);
        }

        public async Task<List<ChargeItem>> GetByCategoryAsync(string category)
        {
            return await _dbSet.Where(c => c.Category == category && c.Status == 1)
                .OrderBy(c => c.ItemName).ToListAsync();
        }

        public async Task<List<string>> GetCategoriesAsync()
        {
            return await _dbSet.Where(c => c.Status == 1)
                .Select(c => c.Category).Distinct().OrderBy(c => c).ToListAsync();
        }
    }
}
