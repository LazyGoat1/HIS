using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    public interface IMedicalOrderService
    {
        Task<(List<MedicalOrderDto> List, int Total)> GetListAsync(MedicalOrderQueryModel query);
        Task<List<MedicalOrderDto>> GetByInpatientIdAsync(long inpatientId);
        Task<MedicalOrderDto?> GetByIdAsync(long id);
        Task<(bool Success, string Message)> CreateAsync(MedicalOrderDto dto, long userId);
        Task<(bool Success, string Message)> ExecuteAsync(long id, long executorUserId);
        Task<(bool Success, string Message)> CompleteAsync(long id);
        Task<(bool Success, string Message)> StopAsync(long id);
    }
}
