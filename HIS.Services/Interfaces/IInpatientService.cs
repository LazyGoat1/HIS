using HIS.Models.DTOs;
using HIS.Models.QueryModels;

namespace HIS.Services.Interfaces
{
    public interface IInpatientService
    {
        Task<(List<InpatientRecordDto> List, int Total)> GetListAsync(InpatientQueryModel query);
        Task<InpatientRecordDto?> GetByIdAsync(long id);
        Task<(bool Success, string Message)> AdmitAsync(InpatientRecordDto dto, long userId);
        Task<(bool Success, string Message)> DischargeAsync(long id, string dischargeDiagnosis);
    }
}
