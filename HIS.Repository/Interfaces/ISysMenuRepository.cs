using HIS.Models.Entities;

namespace HIS.Repository.Interfaces
{
    public interface ISysMenuRepository : IBaseRepository<SysMenu>
    {
        /// <summary>获取所有菜单(树形结构)</summary>
        Task<List<SysMenu>> GetAllMenusAsync();

        /// <summary>获取指定角色的菜单</summary>
        Task<List<SysMenu>> GetMenusByRoleIdAsync(long roleId);

        /// <summary>获取子菜单</summary>
        Task<List<SysMenu>> GetChildMenusAsync(long parentId);

        /// <summary>根据权限码获取菜单</summary>
        Task<List<SysMenu>> GetMenusByPermissionCodesAsync(IEnumerable<string> codes);
    }
}
