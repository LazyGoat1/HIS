using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    public interface ISysRoleRepository : IBaseRepository<SysRole>
    {
        /// <summary>获取角色列表(含用户数量)</summary>
        Task<(List<SysRole> Roles, int Total)> GetRoleListAsync(int pageIndex, int pageSize, string? keyword = null);

        /// <summary>根据角色编码获取</summary>
        Task<SysRole?> GetByRoleCodeAsync(string roleCode);

        /// <summary>获取角色的菜单权限</summary>
        Task<List<long>> GetRoleMenuIdsAsync(long roleId);

        /// <summary>保存角色菜单权限</summary>
        Task SaveRoleMenusAsync(long roleId, List<long> menuIds);
    }
}
