using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>医生排班表</summary>
    [Table("Schedule")]
    public class Schedule : BaseEntity
    {
        public long DoctorId { get; set; }
        public long DepartmentId { get; set; }
        /// <summary>星期 0=周日 1-6</summary>
        public int DayOfWeek { get; set; }
        /// <summary>时段：上午/下午/全天</summary>
        [Required, MaxLength(20)]
        public string TimeSlot { get; set; } = "全天";
        /// <summary>该时段最大接诊数</summary>
        public int MaxPatients { get; set; } = 30;
        /// <summary>状态 1启用 0停用</summary>
        public int Status { get; set; } = 1;

        [ForeignKey(nameof(DoctorId))] public virtual DoctorInfo? Doctor { get; set; }
        [ForeignKey(nameof(DepartmentId))] public virtual SysDepartment? Department { get; set; }
    }
}
