using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    public interface IDrugCategoryRepository : IBaseRepository<DrugCategory>
    {
        Task<List<DrugCategory>> GetAllOrderedAsync();
        Task<List<DrugCategory>> GetTreeAsync();
        Task<bool> HasChildrenAsync(long id);
        Task<bool> HasDrugsAsync(long id);
    }
}
