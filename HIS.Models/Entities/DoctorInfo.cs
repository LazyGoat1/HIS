using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 医生信息
    /// </summary>
    [Table("DoctorInfo")]
    public class DoctorInfo : BaseEntity
    {
        /// <summary>关联系统用户ID</summary>
        public long? UserId { get; set; }

        /// <summary>医生工号</summary>
        [Required, MaxLength(50)]
        public string DoctorNo { get; set; } = string.Empty;

        /// <summary>姓名</summary>
        [Required, MaxLength(20)]
        public string Name { get; set; } = string.Empty;

        /// <summary>性别</summary>
        public int Gender { get; set; }

        /// <summary>科室ID</summary>
        public long DepartmentId { get; set; }

        /// <summary>职称 1:主任医师 2:副主任医师 3:主治医师 4:住院医师</summary>
        public int Title { get; set; }

        /// <summary>专长</summary>
        [MaxLength(200)]
        public string? Specialty { get; set; }

        /// <summary>每日最大接诊数</summary>
        public int MaxDailyPatients { get; set; } = 50;

        /// <summary>挂号费/诊疗费</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal ConsultationFee { get; set; }

        /// <summary>状态 0:休假 1:在岗</summary>
        public int Status { get; set; } = 1;

        // 导航属性
        [ForeignKey(nameof(DepartmentId))]
        public virtual SysDepartment? Department { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual SysUser? User { get; set; }

        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public virtual ICollection<OutpatientRecord> OutpatientRecords { get; set; } = new List<OutpatientRecord>();
    }
}
