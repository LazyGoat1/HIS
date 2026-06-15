using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    public interface IChargeService
    {
        Task<(List<ChargeRecordDto> List, int Total)> GetListAsync(ChargeQueryModel query);
        Task<ChargeRecordDto?> GetByIdAsync(long id);
        Task<(bool Success, string Message)> CreateAsync(ChargeCreateDto dto, long userId);
        Task<(bool Success, string Message)> RefundAsync(long id);
    }
}
