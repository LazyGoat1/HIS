using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 系统角色
    /// </summary>
    [Table("SysRole")]
    public class SysRole : BaseEntity
    {
        /// <summary>角色名称</summary>
        [Required, MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;

        /// <summary>角色编码</summary>
        [Required, MaxLength(50)]
        public string RoleCode { get; set; } = string.Empty;

        /// <summary>描述</summary>
        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>排序</summary>
        public int SortOrder { get; set; }

        /// <summary>状态</summary>
        public int Status { get; set; } = 1;

        // 导航属性
        public virtual ICollection<SysRoleMenu> RoleMenus { get; set; } = new List<SysRoleMenu>();
        public virtual ICollection<SysUser> Users { get; set; } = new List<SysUser>();
    }
}
