using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 系统部门/科室
    /// </summary>
    [Table("SysDepartment")]
    public class SysDepartment : BaseEntity
    {
        /// <summary>科室名称</summary>
        [Required, MaxLength(50)]
        public string DeptName { get; set; } = string.Empty;

        /// <summary>科室编码</summary>
        [Required, MaxLength(20)]
        public string DeptCode { get; set; } = string.Empty;

        /// <summary>上级科室ID</summary>
        public long? ParentId { get; set; }

        /// <summary>科室类型 1:临床 2:医技 3:行政</summary>
        public int DeptType { get; set; }

        /// <summary>联系电话</summary>
        [MaxLength(20)]
        public string? Phone { get; set; }

        /// <summary>位置</summary>
        [MaxLength(100)]
        public string? Location { get; set; }

        /// <summary>描述</summary>
        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>排序</summary>
        public int SortOrder { get; set; }

        /// <summary>状态</summary>
        public int Status { get; set; } = 1;

        // 导航属性
        [ForeignKey(nameof(ParentId))]
        public virtual SysDepartment? Parent { get; set; }

        public virtual ICollection<SysDepartment> Children { get; set; } = new List<SysDepartment>();
        public virtual ICollection<SysUser> Users { get; set; } = new List<SysUser>();
        public virtual ICollection<DoctorInfo> Doctors { get; set; } = new List<DoctorInfo>();
        public virtual ICollection<BedInfo> Beds { get; set; } = new List<BedInfo>();
    }
}
