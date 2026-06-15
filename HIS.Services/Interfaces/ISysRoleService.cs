using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    public interface ISysRoleService
    {
        Task<(List<SysRoleDto> Roles, int Total)> GetRoleListAsync(int pageIndex, int pageSize, string? keyword = null);
        Task<List<SysRoleDto>> GetAllRolesAsync();
        Task<SysRoleDto?> GetRoleByIdAsync(long id);
        Task<(bool Success, string Message)> CreateRoleAsync(SysRoleDto dto);
        Task<(bool Success, string Message)> UpdateRoleAsync(SysRoleDto dto);
        Task<(bool Success, string Message)> DeleteRoleAsync(long id);
        Task<List<long>> GetRoleMenuIdsAsync(long roleId);
        Task<(bool Success, string Message)> SetRoleMenusAsync(long roleId, List<long> menuIds);
    }
}
