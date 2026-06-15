using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 药品信息仓储接口
    /// </summary>
    public interface IDrugRepository : IBaseRepository<DrugInfo>
    {
        /// <summary>
        /// 分页查询药品列表（含分类导航属性）
        /// </summary>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">每页条数</param>
        /// <param name="keyword">搜索关键词：药品名称/编码/通用名</param>
        /// <param name="categoryId">药品分类筛选</param>
        Task<(List<DrugInfo> Drugs, int Total)> GetDrugListAsync(
            int pageIndex, int pageSize, string? keyword = null, long? categoryId = null);

        /// <summary>
        /// 根据药品编码查找
        /// </summary>
        Task<DrugInfo?> GetByDrugCodeAsync(string drugCode);

        /// <summary>
        /// 获取库存预警药品列表（库存量 ≤ 最低库存）
        /// </summary>
        Task<List<DrugInfo>> GetLowStockDrugsAsync();

        /// <summary>
        /// 更新药品库存数量
        /// </summary>
        Task UpdateStockAsync(long drugId, int quantity);
    }
}
