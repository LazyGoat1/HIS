using HIS.Common.Enums;
using HIS.Models.Entities;
using HIS.Models.QueryModels;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class BedInfoRepository : BaseRepository<BedInfo>, IBedInfoRepository
    {
        public BedInfoRepository(HisDbContext context) : base(context) { }

        public async Task<(List<BedInfo> List, int Total)> GetListAsync(BedQueryModel query)
        {
            var q = _dbSet.Include(b => b.Department).AsQueryable();

            if (!string.IsNullOrEmpty(query.Keyword))
                q = q.Where(b => b.BedNo.Contains(query.Keyword) || b.RoomNo.Contains(query.Keyword));

            if (query.DepartmentId.HasValue)
                q = q.Where(b => b.DepartmentId == query.DepartmentId.Value);

            if (query.Status.HasValue)
                q = q.Where(b => b.Status == query.Status.Value);

            if (query.BedType.HasValue)
                q = q.Where(b => b.BedType == query.BedType.Value);

            var total = await q.CountAsync();
            var list = await q.OrderBy(b => b.DepartmentId).ThenBy(b => b.RoomNo).ThenBy(b => b.BedNo)
                .Skip((query.PageIndex - 1) * query.PageSize).Take(query.PageSize).ToListAsync();
            return (list, total);
        }

        public async Task<List<BedInfo>> GetAvailableBedsAsync(long departmentId)
        {
            return await _dbSet
                .Where(b => b.DepartmentId == departmentId && b.Status == (int)BedStatusEnum.Available)
                .OrderBy(b => b.RoomNo).ThenBy(b => b.BedNo)
                .ToListAsync();
        }

        public async Task<BedInfo?> GetByBedNoAsync(string bedNo, long departmentId)
        {
            return await _dbSet.FirstOrDefaultAsync(b =>
                b.BedNo == bedNo && b.DepartmentId == departmentId);
        }
    }
}
