using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 处方
    /// </summary>
    [Table("Prescription")]
    public class Prescription : BaseEntity
    {
        /// <summary>处方号</summary>
        [Required, MaxLength(50)]
        public string PrescriptionNo { get; set; } = string.Empty;

        /// <summary>挂号ID</summary>
        public long RegistrationId { get; set; }

        /// <summary>门诊记录ID</summary>
        public long OutpatientRecordId { get; set; }

        /// <summary>患者ID</summary>
        public long PatientId { get; set; }

        /// <summary>医生ID</summary>
        public long DoctorId { get; set; }

        /// <summary>处方类型 1:西药 2:中药 3:检查</summary>
        public int PrescriptionType { get; set; }

        /// <summary>合计金额</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>状态 1:已开具 2:已收费 3:已发药 4:已退方</summary>
        public int Status { get; set; } = 1;

        /// <summary>备注</summary>
        [MaxLength(500)]
        public string? Remark { get; set; }

        // 导航属性
        [ForeignKey(nameof(RegistrationId))]
        public virtual Registration? Registration { get; set; }

        [ForeignKey(nameof(OutpatientRecordId))]
        public virtual OutpatientRecord? OutpatientRecord { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual DoctorInfo? Doctor { get; set; }

        public virtual ICollection<PrescriptionDetail> Details { get; set; } = new List<PrescriptionDetail>();
    }
}
