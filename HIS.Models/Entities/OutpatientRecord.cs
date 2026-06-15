using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 门诊诊疗记录
    /// </summary>
    [Table("OutpatientRecord")]
    public class OutpatientRecord : BaseEntity
    {
        /// <summary>挂号ID</summary>
        public long RegistrationId { get; set; }

        /// <summary>患者ID</summary>
        public long PatientId { get; set; }

        /// <summary>医生ID</summary>
        public long DoctorId { get; set; }

        /// <summary>主诉</summary>
        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        /// <summary>现病史</summary>
        [MaxLength(1000)]
        public string? PresentIllness { get; set; }

        /// <summary>既往史</summary>
        [MaxLength(500)]
        public string? PastHistory { get; set; }

        /// <summary>体格检查</summary>
        [MaxLength(1000)]
        public string? PhysicalExamination { get; set; }

        /// <summary>初步诊断</summary>
        [MaxLength(500)]
        public string? PreliminaryDiagnosis { get; set; }

        /// <summary>医嘱建议</summary>
        [MaxLength(1000)]
        public string? Advice { get; set; }

        /// <summary>就诊时间</summary>
        public DateTime VisitTime { get; set; } = DateTime.Now;

        // 导航属性
        [ForeignKey(nameof(RegistrationId))]
        public virtual Registration? Registration { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual DoctorInfo? Doctor { get; set; }

        public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
    }
}
