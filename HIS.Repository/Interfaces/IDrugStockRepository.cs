using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    public interface IDrugStockRepository : IBaseRepository<DrugStockLog>
    {
        Task<(List<DrugStockLog> List, int Total)> GetListAsync(DrugStockQueryModel query);
    }
}
