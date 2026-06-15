using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 角色菜单关联
    /// </summary>
    [Table("SysRoleMenu")]
    public class SysRoleMenu : BaseEntity
    {
        /// <summary>角色ID</summary>
        public long RoleId { get; set; }

        /// <summary>菜单ID</summary>
        public long MenuId { get; set; }

        // 导航属性
        [ForeignKey(nameof(RoleId))]
        public virtual SysRole? Role { get; set; }

        [ForeignKey(nameof(MenuId))]
        public virtual SysMenu? Menu { get; set; }
    }
}
