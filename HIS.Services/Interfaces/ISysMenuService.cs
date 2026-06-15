using HIS.Models.DTOs;
using HIS.Models.ViewModels;

namespace HIS.Services.Interfaces
{
    public interface ISysMenuService
    {
        Task<List<MenuTreeViewModel>> GetMenuTreeAsync();
        Task<List<MenuTreeViewModel>> GetMenuTreeByRoleIdAsync(long roleId);
        Task<SysMenuDto?> GetMenuByIdAsync(long id);
        Task<(bool Success, string Message)> CreateMenuAsync(SysMenuDto dto);
        Task<(bool Success, string Message)> UpdateMenuAsync(SysMenuDto dto);
        Task<(bool Success, string Message)> DeleteMenuAsync(long id);
    }
}
