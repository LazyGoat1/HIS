using HIS.Models.DTOs;

namespace HIS.Services.Interfaces
{
    /// <summary>
    /// 系统用户服务接口
    /// </summary>
    public interface ISysUserService
    {
        /// <summary>用户登录</summary>
        Task<(bool Success, string Message, SysUserDto? User)> LoginAsync(string userName, string password, string ipAddress);

        /// <summary>获取用户列表</summary>
        Task<(List<SysUserDto> Users, int Total)> GetUserListAsync(int pageIndex, int pageSize, string? keyword = null);

        /// <summary>根据ID获取用户</summary>
        Task<SysUserDto?> GetUserByIdAsync(long id);

        /// <summary>更新个人信息</summary>
        Task<(bool Success, string Message)> UpdateProfileAsync(long userId, string? realName, string? phone, string? email, string? avatar);

        /// <summary>新增用户</summary>
        Task<(bool Success, string Message)> CreateUserAsync(SysUserCreateDto dto, long createUserId);

        /// <summary>更新用户</summary>
        Task<(bool Success, string Message)> UpdateUserAsync(SysUserCreateDto dto, long updateUserId);

        /// <summary>删除用户</summary>
        Task<(bool Success, string Message)> DeleteUserAsync(long id);

        /// <summary>修改密码</summary>
        Task<(bool Success, string Message)> ChangePasswordAsync(long userId, string oldPassword, string newPassword);

        /// <summary>重置密码</summary>
        Task<(bool Success, string Message, string? NewPassword)> ResetPasswordAsync(long userId);

        /// <summary>禁用/启用用户</summary>
        Task<(bool Success, string Message)> SetUserStatusAsync(long userId, int status);

        /// <summary>获取用户菜单权限</summary>
        Task<List<Models.ViewModels.SidebarMenuViewModel>> GetUserMenusAsync(long userId);

        /// <summary>获取用户按钮权限</summary>
        Task<List<string>> GetUserPermissionsAsync(long userId);
    }
}
