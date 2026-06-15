using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 挂号记录
    /// </summary>
    [Table("Registration")]
    public class Registration : BaseEntity
    {
        /// <summary>挂号单号</summary>
        [Required, MaxLength(50)]
        public string RegistrationNo { get; set; } = string.Empty;

        /// <summary>患者ID</summary>
        public long PatientId { get; set; }

        /// <summary>科室ID</summary>
        public long DepartmentId { get; set; }

        /// <summary>医生ID</summary>
        public long DoctorId { get; set; }

        /// <summary>挂号类型 1:普通 2:专家 3:急诊</summary>
        public int RegistrationType { get; set; }

        /// <summary>挂号费</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal RegistrationFee { get; set; }

        /// <summary>状态 1:已挂号 2:已接诊 3:已退号</summary>
        public int Status { get; set; } = 1;

        /// <summary>就诊日期</summary>
        public DateTime VisitDate { get; set; }

        /// <summary>排队序号</summary>
        public int QueueNumber { get; set; }

        /// <summary>创建用户ID(挂号员)</summary>
        public long CreateUserId { get; set; }

        // 导航属性
        [ForeignKey(nameof(PatientId))]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual SysDepartment? Department { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual DoctorInfo? Doctor { get; set; }

        [ForeignKey(nameof(CreateUserId))]
        public virtual SysUser? CreateUser { get; set; }

        public virtual OutpatientRecord? OutpatientRecord { get; set; }
        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
