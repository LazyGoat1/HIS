using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    public interface IChargeRecordRepository : IBaseRepository<ChargeRecord>
    {
        Task<(List<ChargeRecord> List, int Total)> GetListAsync(ChargeQueryModel query);
        Task<ChargeRecord?> GetByChargeNoAsync(string chargeNo);
        Task<ChargeRecord?> GetDetailAsync(long id);
    }
}
