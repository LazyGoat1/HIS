using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    public interface IDrugCategoryService
    {
        Task<List<DrugCategoryDto>> GetTreeAsync();
        Task<List<DrugCategoryDto>> GetAllAsync();
        Task<List<DrugCategoryDto>> GetHierarchicalAsync();
        Task<DrugCategoryDto?> GetByIdAsync(long id);
        Task<(bool Success, string Message)> CreateAsync(DrugCategoryDto dto);
        Task<(bool Success, string Message)> UpdateAsync(DrugCategoryDto dto);
        Task<(bool Success, string Message)> DeleteAsync(long id);
    }
}
