using HIS.Models.DTOs;
using HIS.Models.Entities;
using HIS.Models.ViewModels;
using HIS.Repository.Interfaces;
using HIS.Repository.UnitOfWork;
using HIS.Services.Interfaces;

namespace HIS.Services.Implementations
{
    public class SysMenuService : ISysMenuService
    {
        private readonly ISysMenuRepository _menuRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SysMenuService(ISysMenuRepository menuRepository, IUnitOfWork unitOfWork)
        {
            _menuRepository = menuRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<List<MenuTreeViewModel>> GetMenuTreeAsync()
        {
            var menus = await _menuRepository.GetAllMenusAsync();
            return BuildMenuTree(menus, null);
        }

        public async Task<List<MenuTreeViewModel>> GetMenuTreeByRoleIdAsync(long roleId)
        {
            var allMenus = await _menuRepository.GetAllMenusAsync();
            return BuildMenuTree(allMenus, null);
        }

        public async Task<SysMenuDto?> GetMenuByIdAsync(long id)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            return menu == null ? null : MapToDto(menu);
        }

        public async Task<(bool Success, string Message)> CreateMenuAsync(SysMenuDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var menu = new SysMenu
            {
                ParentId = dto.ParentId,
                MenuName = dto.MenuName,
                MenuType = dto.MenuType,
                MenuUrl = dto.MenuUrl,
                MenuIcon = dto.MenuIcon,
                SortOrder = dto.SortOrder,
                PermissionCode = dto.PermissionCode,
                Status = dto.Status
            };

            await _menuRepository.AddAsync(menu);
            await _unitOfWork.SaveChangesAsync();
            return (true, "新增菜单成功");
        }

        public async Task<(bool Success, string Message)> UpdateMenuAsync(SysMenuDto dto)
        {
            if (dto == null) return (false, "请求数据为空");
            var menu = await _menuRepository.GetByIdAsync(dto.Id);
            if (menu == null) return (false, "菜单不存在");

            menu.ParentId = dto.ParentId;
            menu.MenuName = dto.MenuName;
            menu.MenuType = dto.MenuType;
            menu.MenuUrl = dto.MenuUrl;
            menu.MenuIcon = dto.MenuIcon;
            menu.SortOrder = dto.SortOrder;
            menu.PermissionCode = dto.PermissionCode;
            menu.Status = dto.Status;

            _menuRepository.Update(menu);
            await _unitOfWork.SaveChangesAsync();
            return (true, "更新菜单成功");
        }

        public async Task<(bool Success, string Message)> DeleteMenuAsync(long id)
        {
            var menu = await _menuRepository.GetByIdAsync(id);
            if (menu == null) return (false, "菜单不存在");

            // 检查是否有子菜单
            if (await _menuRepository.AnyAsync(m => m.ParentId == id))
                return (false, "请先删除子菜单");

            _menuRepository.Delete(menu);
            await _unitOfWork.SaveChangesAsync();
            return (true, "删除菜单成功");
        }

        private static SysMenuDto MapToDto(SysMenu menu) => new()
        {
            Id = menu.Id,
            ParentId = menu.ParentId,
            MenuName = menu.MenuName,
            MenuType = menu.MenuType,
            MenuUrl = menu.MenuUrl,
            MenuIcon = menu.MenuIcon,
            SortOrder = menu.SortOrder,
            PermissionCode = menu.PermissionCode,
            Status = menu.Status,
            CreateTime = menu.CreateTime
        };

        private List<MenuTreeViewModel> BuildMenuTree(List<SysMenu> menus, long? parentId)
        {
            return menus
                .Where(m => m.ParentId == parentId)
                .Select(m => new MenuTreeViewModel
                {
                    Id = m.Id,
                    Title = m.MenuName,
                    Href = m.MenuUrl,
                    Icon = m.MenuIcon,
                    Spread = true,
                    Children = BuildMenuTree(menus, m.Id)
                })
                .ToList();
        }
    }
}
