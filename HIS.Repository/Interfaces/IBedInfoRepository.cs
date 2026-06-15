using HIS.Models.Entities;
using HIS.Models.QueryModels;

namespace HIS.Repository.Interfaces
{
    public interface IBedInfoRepository : IBaseRepository<BedInfo>
    {
        Task<(List<BedInfo> List, int Total)> GetListAsync(BedQueryModel query);
        Task<List<BedInfo>> GetAvailableBedsAsync(long departmentId);
        Task<BedInfo?> GetByBedNoAsync(string bedNo, long departmentId);
    }
}
