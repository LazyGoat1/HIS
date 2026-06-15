using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    public class SysRoleRepository : BaseRepository<SysRole>, ISysRoleRepository
    {
        public SysRoleRepository(HisDbContext context) : base(context) { }

        public async Task<(List<SysRole> Roles, int Total)> GetRoleListAsync(int pageIndex, int pageSize, string? keyword = null)
        {
            var query = _dbSet.AsQueryable();
            if (!string.IsNullOrEmpty(keyword))
                query = query.Where(r => r.RoleName.Contains(keyword) || r.RoleCode.Contains(keyword));

            var total = await query.CountAsync();
            var roles = await query.OrderBy(r => r.SortOrder).ThenByDescending(r => r.CreateTime)
                .Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return (roles, total);
        }

        public async Task<SysRole?> GetByRoleCodeAsync(string roleCode)
        {
            return await _dbSet.FirstOrDefaultAsync(r => r.RoleCode == roleCode);
        }

        public async Task<List<long>> GetRoleMenuIdsAsync(long roleId)
        {
            return await _context.SysRoleMenus
                .Where(rm => rm.RoleId == roleId)
                .Select(rm => rm.MenuId)
                .ToListAsync();
        }

        public async Task SaveRoleMenusAsync(long roleId, List<long> menuIds)
        {
            // 删除原有菜单
            var existing = await _context.SysRoleMenus.Where(rm => rm.RoleId == roleId).ToListAsync();
            _context.SysRoleMenus.RemoveRange(existing);

            // 添加新菜单
            var newEntries = menuIds.Select(menuId => new SysRoleMenu { RoleId = roleId, MenuId = menuId });
            await _context.SysRoleMenus.AddRangeAsync(newEntries);
        }
    }
}
