using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    /// <summary>
    /// 药品信息仓储实现
    /// </summary>
    public class DrugRepository : BaseRepository<DrugInfo>, IDrugRepository
    {
        public DrugRepository(HisDbContext context) : base(context) { }

        /// <summary>
        /// 分页查询药品列表
        /// 预加载药品分类导航属性，用于显示分类名称
        /// </summary>
        public async Task<(List<DrugInfo> Drugs, int Total)> GetDrugListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? categoryId = null)
        {
            var query = _dbSet
                .Include(d => d.Category)  // 预加载分类信息
                .AsQueryable();

            // 关键字搜索：药品名称 / 编码 / 通用名 / 分类名称
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(d =>
                    d.DrugName.Contains(keyword) ||
                    d.DrugCode.Contains(keyword) ||
                    (d.GenericName != null && d.GenericName.Contains(keyword)) ||
                    (d.Category != null && d.Category.CategoryName.Contains(keyword)));
            }

            // 按分类筛选（包含所有子分类）
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                var allCategories = await _context.DrugCategories.ToListAsync();
                var descendantIds = GetDescendantIds(allCategories, categoryId.Value);
                descendantIds.Add(categoryId.Value); // 包含自身
                query = query.Where(d => d.CategoryId.HasValue && descendantIds.Contains(d.CategoryId.Value));
            }

            var total = await query.CountAsync();
            var drugs = await query
                .OrderBy(d => d.CategoryId)
                .ThenBy(d => d.DrugCode)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (drugs, total);
        }

        /// <summary>
        /// 根据药品编码查找（编码唯一性校验）
        /// </summary>
        public async Task<DrugInfo?> GetByDrugCodeAsync(string drugCode)
        {
            return await _dbSet.FirstOrDefaultAsync(d => d.DrugCode == drugCode);
        }

        /// <summary>
        /// 获取库存预警药品列表
        /// 条件：当前库存量 ≤ 最低库存预警值
        /// </summary>
        public async Task<List<DrugInfo>> GetLowStockDrugsAsync()
        {
            return await _dbSet
                .Where(d => d.Status == 1 && d.StockQuantity <= d.MinStock)
                .OrderBy(d => d.StockQuantity)
                .ToListAsync();
        }

        /// <summary>
        /// 原子更新药品库存数量
        /// 使用 ExecuteUpdate 避免并发问题
        /// </summary>
        public async Task UpdateStockAsync(long drugId, int quantity)
        {
            await _dbSet
                .Where(d => d.Id == drugId)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(d => d.StockQuantity, quantity));
        }

        /// <summary>递归获取指定分类的所有子孙分类ID</summary>
        private static List<long> GetDescendantIds(List<DrugCategory> all, long parentId)
        {
            var ids = new List<long>();
            var children = all.Where(c => c.ParentId == parentId).ToList();
            foreach (var child in children)
            {
                ids.Add(child.Id);
                ids.AddRange(GetDescendantIds(all, child.Id));
            }
            return ids;
        }
    }
}
