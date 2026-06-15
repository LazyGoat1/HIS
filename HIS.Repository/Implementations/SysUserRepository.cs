using HIS.Models.Entities;
using HIS.Repository.Data;
using HIS.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Repository.Implementations
{
    /// <summary>
    /// 系统用户仓储实现
    /// </summary>
    public class SysUserRepository : BaseRepository<SysUser>, ISysUserRepository
    {
        public SysUserRepository(HisDbContext context) : base(context)
        {
        }

        public async Task<SysUser?> GetByUserNameAsync(string userName)
        {
            return await _dbSet
                .Where(u => u.UserName == userName && !u.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<SysUser?> GetUserWithRoleAsync(string userName)
        {
            return await _dbSet
                .Include(u => u.Role)
                    .ThenInclude(r => r!.RoleMenus)
                        .ThenInclude(rm => rm.Menu)
                .Include(u => u.Department)
                .Where(u => u.UserName == userName && !u.IsDeleted && u.Status == 1)
                .FirstOrDefaultAsync();
        }

        public async Task<(List<SysUser> Users, int Total)> GetUserListAsync(
            int pageIndex, int pageSize, string? keyword = null)
        {
            var query = _dbSet
                .Include(u => u.Role)
                .Include(u => u.Department)
                .Where(u => !u.IsDeleted)
                .AsQueryable();

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(u =>
                    u.UserName.Contains(keyword) ||
                    u.RealName.Contains(keyword) ||
                    (u.Phone != null && u.Phone.Contains(keyword)));
            }

            var total = await query.CountAsync();
            var users = await query
                .OrderByDescending(u => u.CreateTime)
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, total);
        }

        public async Task UpdateLoginInfoAsync(long userId, DateTime loginTime)
        {
            var user = await _dbSet.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null)
            { user.LastLoginTime = loginTime; user.LoginFailedCount = 0; user.LockoutEnd = null; await _context.SaveChangesAsync(); }
        }

        public async Task IncreaseLoginFailedAsync(long userId)
        {
            var user = await _dbSet.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null) { user.LoginFailedCount++; await _context.SaveChangesAsync(); }
        }

        public async Task LockAccountAsync(long userId, DateTime lockoutEnd)
        {
            var user = await _dbSet.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null) { user.LockoutEnd = lockoutEnd; await _context.SaveChangesAsync(); }
        }

        public async Task ResetLoginFailedAsync(long userId)
        {
            var user = await _dbSet.Where(u => u.Id == userId).FirstOrDefaultAsync();
            if (user != null) { user.LoginFailedCount = 0; user.LockoutEnd = null; await _context.SaveChangesAsync(); }
        }
    }
}
