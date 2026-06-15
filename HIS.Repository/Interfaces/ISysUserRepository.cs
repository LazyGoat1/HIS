using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    /// <summary>
    /// 系统用户仓储接口
    /// </summary>
    public interface ISysUserRepository : IBaseRepository<SysUser>
    {
        /// <summary>根据用户名获取用户(含角色信息)</summary>
        Task<SysUser?> GetByUserNameAsync(string userName);

        /// <summary>根据用户名获取用户(含角色和角色菜单)</summary>
        Task<SysUser?> GetUserWithRoleAsync(string userName);

        /// <summary>获取用户列表(含角色和部门)</summary>
        Task<(List<SysUser> Users, int Total)> GetUserListAsync(int pageIndex, int pageSize, string? keyword = null);

        /// <summary>更新最后登录时间</summary>
        Task UpdateLoginInfoAsync(long userId, DateTime loginTime);

        /// <summary>增加登录失败次数</summary>
        Task IncreaseLoginFailedAsync(long userId);

        /// <summary>锁定账号</summary>
        Task LockAccountAsync(long userId, DateTime lockoutEnd);

        /// <summary>重置登录失败次数</summary>
        Task ResetLoginFailedAsync(long userId);
    }
}
