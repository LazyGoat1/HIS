using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    public interface IChargeItemService
    {
        Task<List<ChargeItemDto>> GetAllAsync();
        Task<(List<ChargeItemDto> List, int Total)> GetPagedAsync(int pageIndex, int pageSize, string? keyword = null);
        Task<List<ChargeItemDto>> GetByCategoryAsync(string category);
        Task<List<string>> GetCategoriesAsync();
        Task<ChargeItemDto?> GetByIdAsync(long id);
        Task<(bool Success, string Message)> CreateAsync(ChargeItemDto dto);
        Task<(bool Success, string Message)> UpdateAsync(ChargeItemDto dto);
        Task<(bool Success, string Message)> DeleteAsync(long id);
    }
}
