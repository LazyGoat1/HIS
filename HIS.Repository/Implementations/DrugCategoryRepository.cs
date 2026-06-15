using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class DrugCategoryRepository : BaseRepository<DrugCategory>, IDrugCategoryRepository
    {
        public DrugCategoryRepository(HisDbContext context) : base(context) { }

        public async Task<List<DrugCategory>> GetAllOrderedAsync()
        {
            return await _dbSet.Include(c => c.Parent)
                .OrderBy(c => c.ParentId).ThenBy(c => c.SortOrder).ThenBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<List<DrugCategory>> GetTreeAsync()
        {
            // 一次性加载全部 → EF Core 自动 fixup 填充 Children 导航属性
            var all = await _dbSet.Include(c => c.Drugs)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName)
                .ToListAsync();

            // 只返回根节点，EF 的 relationship fixup 已自动建立完整父子树
            return all.Where(c => c.ParentId == null).ToList();
        }

        public async Task<bool> HasChildrenAsync(long id)
        {
            return await _dbSet.AnyAsync(c => c.ParentId == id);
        }

        public async Task<bool> HasDrugsAsync(long id)
        {
            return await _context.Set<DrugInfo>().AnyAsync(d => d.CategoryId == id);
        }
    }
}
