using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    public interface IInpatientRecordRepository : IBaseRepository<InpatientRecord>
    {
        Task<(List<InpatientRecord> List, int Total)> GetListAsync(InpatientQueryModel query);
        Task<InpatientRecord?> GetByInpatientNoAsync(string inpatientNo);
        Task<InpatientRecord?> GetDetailAsync(long id);
        Task<bool> HasActiveRecordAsync(long patientId);
    }
}
