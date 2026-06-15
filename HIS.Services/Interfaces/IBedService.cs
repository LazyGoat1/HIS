using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    public interface IBedService
    {
        Task<(List<BedInfoDto> List, int Total)> GetListAsync(BedQueryModel query);
        Task<List<BedInfoDto>> GetAvailableBedsAsync(long departmentId);
        Task<BedInfoDto?> GetByIdAsync(long id);
        Task<(bool Success, string Message)> CreateAsync(BedInfoDto dto);
        Task<(bool Success, string Message)> UpdateAsync(BedInfoDto dto);
        Task<(bool Success, string Message)> DeleteAsync(long id);
    }
}
