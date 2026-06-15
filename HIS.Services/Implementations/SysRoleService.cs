using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    public class SysRoleService : ISysRoleService
    {
        private readonly ISysRoleRepository _roleRepository;
        private readonly IBaseRepository<SysUser> _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SysRoleService(ISysRoleRepository roleRepository, IBaseRepository<SysUser> userRepository, IUnitOfWork unitOfWork)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<(List<SysRoleDto> Roles, int Total)> GetRoleListAsync(int pageIndex, int pageSize, string? keyword = null)
        {
            var (roles, total) = await _roleRepository.GetRoleListAsync(pageIndex, pageSize, keyword);
            var dtos = roles.Select(MapToDto).ToList();
            return (dtos, total);
        }

        public async Task<List<SysRoleDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Where(r => r.Status == 1).Select(MapToDto).ToList();
        }

        public async Task<SysRoleDto?> GetRoleByIdAsync(long id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            return role == null ? null : MapToDto(role);
        }

        public async Task<(bool Success, string Message)> CreateRoleAsync(SysRoleDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            if (await _roleRepository.AnyAsync(r => r.RoleCode == dto.RoleCode))
                return (false, "角色编码已存在");

            var role = new SysRole
            {
                RoleName = dto.RoleName,
                RoleCode = dto.RoleCode,
                Description = dto.Description,
                SortOrder = dto.SortOrder,
                Status = dto.Status
            };

            await _roleRepository.AddAsync(role);
            await _unitOfWork.SaveChangesAsync();
            return (true, "新增角色成功");
        }

        public async Task<(bool Success, string Message)> UpdateRoleAsync(SysRoleDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var role = await _roleRepository.GetByIdAsync(dto.Id);
            if (role == null) return (false, "角色不存在");

            if (await _roleRepository.AnyAsync(r => r.RoleCode == dto.RoleCode && r.Id != dto.Id))
                return (false, "角色编码已存在");

            role.RoleName = dto.RoleName;
            role.RoleCode = dto.RoleCode;
            role.Description = dto.Description;
            role.SortOrder = dto.SortOrder;
            role.Status = dto.Status;

            _roleRepository.Update(role);
            await _unitOfWork.SaveChangesAsync();
            return (true, "更新角色成功");
        }

        public async Task<(bool Success, string Message)> DeleteRoleAsync(long id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return (false, "角色不存在");

            // 检查是否有用户正在使用该角色
            if (await _userRepository.AnyAsync(u => u.RoleId == id && !u.IsDeleted))
                return (false, "该角色下还有用户，无法删除");

            _roleRepository.Delete(role);
            await _unitOfWork.SaveChangesAsync();
            return (true, "删除角色成功");
        }

        public async Task<List<long>> GetRoleMenuIdsAsync(long roleId)
        {
            return await _roleRepository.GetRoleMenuIdsAsync(roleId);
        }

        public async Task<(bool Success, string Message)> SetRoleMenusAsync(long roleId, List<long> menuIds)
        {
            var role = await _roleRepository.GetByIdAsync(roleId);
            if (role == null) return (false, "角色不存在");

            await _roleRepository.SaveRoleMenusAsync(roleId, menuIds);
            await _unitOfWork.SaveChangesAsync();
            return (true, "权限分配成功");
        }

        private static SysRoleDto MapToDto(SysRole role) => new()
        {
            Id = role.Id,
            RoleName = role.RoleName,
            RoleCode = role.RoleCode,
            Description = role.Description,
            SortOrder = role.SortOrder,
            Status = role.Status,
            CreateTime = role.CreateTime
        };
    }
}
