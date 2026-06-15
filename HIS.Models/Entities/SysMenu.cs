using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 系统菜单/权限
    /// </summary>
    [Table("SysMenu")]
    public class SysMenu : BaseEntity
    {
        /// <summary>父菜单ID</summary>
        public long? ParentId { get; set; }

        /// <summary>菜单名称</summary>
        [Required, MaxLength(50)]
        public string MenuName { get; set; } = string.Empty;

        /// <summary>菜单类型 1:目录 2:菜单 3:按钮</summary>
        public int MenuType { get; set; }

        /// <summary>菜单URL/路由</summary>
        [MaxLength(200)]
        public string? MenuUrl { get; set; }

        /// <summary>菜单图标(Layui图标)</summary>
        [MaxLength(50)]
        public string? MenuIcon { get; set; }

        /// <summary>排序</summary>
        public int SortOrder { get; set; }

        /// <summary>权限标识码</summary>
        [MaxLength(100)]
        public string? PermissionCode { get; set; }

        /// <summary>状态</summary>
        public int Status { get; set; } = 1;

        // 导航属性
        [ForeignKey(nameof(ParentId))]
        public virtual SysMenu? Parent { get; set; }

        public virtual ICollection<SysMenu> Children { get; set; } = new List<SysMenu>();
        public virtual ICollection<SysRoleMenu> RoleMenus { get; set; } = new List<SysRoleMenu>();
    }
}
