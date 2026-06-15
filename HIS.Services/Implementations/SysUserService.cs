using HIS.Common.Constants;
using HIS.Common.Helpers;
using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.ViewModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HIS.Services.Implementations
{
    /// <summary>
    /// 系统用户服务实现
    /// </summary>
    public class SysUserService : ISysUserService
    {
        private readonly ISysUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SysUserService(ISysUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<(bool Success, string Message, SysUserDto? User)> LoginAsync(
            string userName, string password, string ipAddress)
        {
            // 1. 查找用户(含角色)
            var user = await _userRepository.GetUserWithRoleAsync(userName);
            if (user == null)
                return (false, "用户名或密码错误", null);

            // 2. 检查是否锁定
            if (user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTime.Now)
            {
                var remainMinutes = (user.LockoutEnd.Value - DateTime.Now).Minutes;
                return (false, $"账号已被锁定，请{remainMinutes}分钟后再试", null);
            }

            // 3. 验证密码
            var hashedPwd = EncryptionHelper.HashPassword(password, user.Salt);
            if (hashedPwd != user.Password)
            {
                // 记录登录失败
                await _userRepository.IncreaseLoginFailedAsync(user.Id);
                await _unitOfWork.SaveChangesAsync();

                // 失败超过阈值则锁定
                if (user.LoginFailedCount + 1 >= SystemConstants.MaxLoginRetryCount)
                {
                    var lockoutEnd = DateTime.Now.AddMinutes(SystemConstants.LockoutMinutes);
                    await _userRepository.LockAccountAsync(user.Id, lockoutEnd);
                    await _unitOfWork.SaveChangesAsync();
                    return (false, $"密码连续错误{SystemConstants.MaxLoginRetryCount}次，账号已锁定{SystemConstants.LockoutMinutes}分钟", null);
                }

                return (false, $"用户名或密码错误，还有{SystemConstants.MaxLoginRetryCount - user.LoginFailedCount - 1}次机会", null);
            }

            // 4. 检查账号状态
            if (user.Status != 1)
                return (false, "账号已被禁用，请联系管理员", null);

            if (user.Role == null || user.Role.Status != 1)
                return (false, "角色已被禁用，请联系管理员", null);

            // 5. 更新登录信息
            await _userRepository.UpdateLoginInfoAsync(user.Id, DateTime.Now);
            await _unitOfWork.SaveChangesAsync();

            // 6. 返回用户信息
            var userDto = new SysUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                Gender = user.Gender,
                Phone = user.Phone,
                Email = user.Email,
                DepartmentId = user.DepartmentId,
                RoleId = user.RoleId,
                Avatar = user.Avatar, Status = user.Status
            };

            return (true, "登录成功", userDto);
        }

        public async Task<(List<SysUserDto> Users, int Total)> GetUserListAsync(
            int pageIndex, int pageSize, string? keyword = null)
        {
            var (users, total) = await _userRepository.GetUserListAsync(pageIndex, pageSize, keyword);

            var dtos = users.Select(u => new SysUserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                RealName = u.RealName,
                Gender = u.Gender,
                Phone = u.Phone,
                Email = u.Email,
                DepartmentId = u.DepartmentId,
                RoleId = u.RoleId,
                Avatar = u.Avatar, Status = u.Status
            }).ToList();

            return (dtos, total);
        }

        public async Task<SysUserDto?> GetUserByIdAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted) return null;

            return new SysUserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                Gender = user.Gender,
                Phone = user.Phone,
                Email = user.Email,
                Avatar = user.Avatar,
                DepartmentId = user.DepartmentId,
                RoleId = user.RoleId,
                Status = user.Status
            };
        }

        public async Task<(bool Success, string Message)> CreateUserAsync(SysUserCreateDto dto, long createUserId)
        {
            if (dto == null) return (false, "请求数据为空");
            // 检查用户名是否已存在
            if (await _userRepository.AnyAsync(u => u.UserName == dto.UserName && !u.IsDeleted))
                return (false, "用户名已存在");

            var salt = EncryptionHelper.GenerateSalt();
            var password = dto.Password ?? SystemConstants.DefaultPassword;

            var user = new SysUser
            {
                UserName = dto.UserName,
                Password = EncryptionHelper.HashPassword(password, salt),
                Salt = salt,
                RealName = dto.RealName,
                Gender = dto.Gender,
                Phone = dto.Phone,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId,
                RoleId = dto.RoleId,
                Status = dto.Status
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, "新增用户成功");
        }

        public async Task<(bool Success, string Message)> UpdateUserAsync(SysUserCreateDto dto, long updateUserId)
        {
            if (dto == null) return (false, "请求数据为空");
            var user = await _userRepository.GetByIdAsync(dto.Id.GetValueOrDefault());
            if (user == null || user.IsDeleted)
                return (false, "用户不存在");

            // 检查用户名冲突
            if (await _userRepository.AnyAsync(u => u.UserName == dto.UserName && u.Id != dto.Id && !u.IsDeleted))
                return (false, "用户名已存在");

            user.UserName = dto.UserName;
            user.RealName = dto.RealName;
            user.Gender = dto.Gender;
            user.Phone = dto.Phone;
            user.Email = dto.Email;
            user.DepartmentId = dto.DepartmentId;
            user.RoleId = dto.RoleId;
            user.Status = dto.Status;

            // 如果传入了新密码，则更新密码
            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Salt = EncryptionHelper.GenerateSalt();
                user.Password = EncryptionHelper.HashPassword(dto.Password, user.Salt);
            }

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, "更新用户成功");
        }

        public async Task<(bool Success, string Message)> DeleteUserAsync(long id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null || user.IsDeleted)
                return (false, "用户不存在");

            _userRepository.SoftDelete(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, "删除用户成功");
        }

        public async Task<(bool Success, string Message)> ChangePasswordAsync(
            long userId, string oldPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return (false, "用户不存在");

            var oldHash = EncryptionHelper.HashPassword(oldPassword, user.Salt);
            if (oldHash != user.Password)
                return (false, "原密码错误");

            user.Salt = EncryptionHelper.GenerateSalt();
            user.Password = EncryptionHelper.HashPassword(newPassword, user.Salt);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, "密码修改成功");
        }

        public async Task<(bool Success, string Message)> UpdateProfileAsync(
            long userId, string? realName, string? phone, string? email, string? avatar)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted) return (false, "用户不存在");

            if (!string.IsNullOrWhiteSpace(realName)) user.RealName = realName;
            user.Phone = phone;
            user.Email = email;
            if (!string.IsNullOrWhiteSpace(avatar)) user.Avatar = avatar;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
            return (true, "个人信息已更新");
        }

        public async Task<(bool Success, string Message, string? NewPassword)> ResetPasswordAsync(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return (false, "用户不存在", null);

            var newPassword = SystemConstants.DefaultPassword;
            user.Salt = EncryptionHelper.GenerateSalt();
            user.Password = EncryptionHelper.HashPassword(newPassword, user.Salt);

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"密码已重置为{newPassword}", newPassword);
        }

        public async Task<(bool Success, string Message)> SetUserStatusAsync(long userId, int status)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted)
                return (false, "用户不存在");

            user.Status = status;
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return (true, status == 1 ? "用户已启用" : "用户已禁用");
        }

        public async Task<List<SidebarMenuViewModel>> GetUserMenusAsync(long userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.IsDeleted) return new List<SidebarMenuViewModel>();

            var userWithRole = await _userRepository.GetUserWithRoleAsync(user.UserName);
            if (userWithRole?.Role?.RoleMenus == null) return new List<SidebarMenuViewModel>();

            // 获取目录和菜单类型(排除按钮)
            var menus = userWithRole.Role.RoleMenus
                .Select(rm => rm.Menu)
                .Where(m => m != null && m.Status == 1 && (m.MenuType == 1 || m.MenuType == 2))
                .OrderBy(m => m!.SortOrder)
                .ToList();

            return BuildMenuTree(menus!, 0);
        }

        public Task<List<string>> GetUserPermissionsAsync(long userId)
        {
            // TODO: 从用户角色菜单中提取按钮权限码
            return Task.FromResult(new List<string>());
        }

        #region Private Methods

        private List<SidebarMenuViewModel> BuildMenuTree(List<SysMenu> menus, long parentId)
        {
            var result = new List<SidebarMenuViewModel>();

            var parentMenus = menus.Where(m => (m.ParentId ?? 0) == parentId).ToList();

            foreach (var menu in parentMenus)
            {
                var vm = new SidebarMenuViewModel
                {
                    Id = menu.Id,
                    Title = menu.MenuName,
                    Href = menu.MenuUrl,
                    Icon = menu.MenuIcon,
                    Child = BuildMenuTree(menus, menu.Id)
                };

                result.Add(vm);
            }

            return result;
        }

        #endregion
    }
}
