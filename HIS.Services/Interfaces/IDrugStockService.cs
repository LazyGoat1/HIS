using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    public interface IDrugStockService
    {
        Task<(List<DrugStockLogDto> List, int Total)> GetListAsync(DrugStockQueryModel query);
        Task<(bool Success, string Message)> StockInAsync(StockInDto dto, long userId);
        Task<(bool Success, string Message)> StockOutAsync(StockOutDto dto, long userId);
        Task<(bool Success, string Message)> StockCheckAsync(long drugId, int actualQty, long userId);
        Task<List<DrugStockLogDto>> GetByDrugIdAsync(long drugId);
    }
}
