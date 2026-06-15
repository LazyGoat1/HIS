using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 系统用户
    /// </summary>
    [Table("SysUser")]
    public class SysUser : SoftDeleteEntity
    {
        /// <summary>用户名(登录账号)</summary>
        [Required, MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        /// <summary>密码哈希</summary>
        [Required, MaxLength(256)]
        public string Password { get; set; } = string.Empty;

        /// <summary>密码盐值</summary>
        [MaxLength(64)]
        public string Salt { get; set; } = string.Empty;

        /// <summary>真实姓名</summary>
        [Required, MaxLength(20)]
        public string RealName { get; set; } = string.Empty;

        /// <summary>性别 0:女 1:男</summary>
        public int Gender { get; set; }

        /// <summary>手机号</summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>邮箱</summary>
        [MaxLength(100)]
        public string? Email { get; set; }

        /// <summary>头像</summary>
        [MaxLength(500)]
        public string? Avatar { get; set; }

        /// <summary>所属科室ID</summary>
        public long? DepartmentId { get; set; }

        /// <summary>角色ID</summary>
        public long RoleId { get; set; }

        /// <summary>状态 0:禁用 1:启用</summary>
        public int Status { get; set; } = 1;

        /// <summary>最后登录时间</summary>
        public DateTime? LastLoginTime { get; set; }

        /// <summary>登录失败次数</summary>
        public int LoginFailedCount { get; set; } = 0;

        /// <summary>锁定截止时间</summary>
        public DateTime? LockoutEnd { get; set; }

        // 导航属性
        [ForeignKey(nameof(RoleId))]
        public virtual SysRole? Role { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual SysDepartment? Department { get; set; }
    }
}
