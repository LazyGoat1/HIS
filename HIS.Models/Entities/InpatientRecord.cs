using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 住院记录
    /// </summary>
    [Table("InpatientRecord")]
    public class InpatientRecord : BaseEntity
    {
        /// <summary>住院号</summary>
        [Required, MaxLength(50)]
        public string InpatientNo { get; set; } = string.Empty;

        /// <summary>患者ID</summary>
        public long PatientId { get; set; }

        /// <summary>床位ID</summary>
        public long? BedId { get; set; }

        /// <summary>科室ID</summary>
        public long DepartmentId { get; set; }

        /// <summary>主治医生ID</summary>
        public long DoctorId { get; set; }

        /// <summary>入院时间</summary>
        public DateTime AdmissionTime { get; set; } = DateTime.Now;

        /// <summary>出院时间</summary>
        public DateTime? DischargeTime { get; set; }

        /// <summary>入院诊断</summary>
        [MaxLength(500)]
        public string? AdmissionDiagnosis { get; set; }

        /// <summary>出院诊断</summary>
        [MaxLength(500)]
        public string? DischargeDiagnosis { get; set; }

        /// <summary>状态 1:在院 2:出院 3:转科</summary>
        public int Status { get; set; } = 1;

        /// <summary>预交金余额</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositAmount { get; set; }

        /// <summary>总费用</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalCost { get; set; }

        // 导航属性
        [ForeignKey(nameof(PatientId))]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey(nameof(BedId))]
        public virtual BedInfo? Bed { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual SysDepartment? Department { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual DoctorInfo? Doctor { get; set; }

        public virtual ICollection<MedicalOrder> MedicalOrders { get; set; } = new List<MedicalOrder>();
    }
}
