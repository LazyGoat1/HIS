using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    public interface IChargeItemRepository : IBaseRepository<ChargeItem>
    {
        Task<List<ChargeItem>> GetAllEnabledAsync();
        Task<(List<ChargeItem> List, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null);
        Task<List<ChargeItem>> GetByCategoryAsync(string category);
        Task<List<string>> GetCategoriesAsync();
    }
}
