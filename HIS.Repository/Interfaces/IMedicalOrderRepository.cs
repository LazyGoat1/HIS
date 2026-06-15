using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    public interface IMedicalOrderRepository : IBaseRepository<MedicalOrder>
    {
        Task<(List<MedicalOrder> List, int Total)> GetListAsync(MedicalOrderQueryModel query);
        Task<List<MedicalOrder>> GetByInpatientIdAsync(long inpatientId);
        Task<MedicalOrder?> GetDetailAsync(long id);
    }
}
