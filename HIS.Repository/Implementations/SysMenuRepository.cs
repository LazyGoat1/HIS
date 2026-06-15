using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class SysMenuRepository : BaseRepository<SysMenu>, ISysMenuRepository
    {
        public SysMenuRepository(HisDbContext context) : base(context) { }

        public async Task<List<SysMenu>> GetAllMenusAsync()
        {
            return await _dbSet
                .Where(m => m.Status == 1)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<List<SysMenu>> GetMenusByRoleIdAsync(long roleId)
        {
            return await _context.SysRoleMenus
                .Where(rm => rm.RoleId == roleId)
                .Include(rm => rm.Menu)
                .Where(rm => rm.Menu!.Status == 1)
                .Select(rm => rm.Menu!)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<List<SysMenu>> GetChildMenusAsync(long parentId)
        {
            return await _dbSet
                .Where(m => m.ParentId == parentId && m.Status == 1)
                .OrderBy(m => m.SortOrder)
                .ToListAsync();
        }

        public async Task<List<SysMenu>> GetMenusByPermissionCodesAsync(IEnumerable<string> codes)
        {
            return await _dbSet
                .Where(m => m.PermissionCode != null && codes.Contains(m.PermissionCode))
                .ToListAsync();
        }
    }
}
