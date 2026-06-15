using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>护理记录</summary>
    [Table("NursingRecord")]
    public class NursingRecord : BaseEntity
    {
        public long InpatientId { get; set; }
        public long PatientId { get; set; }
        public long? MedicalOrderId { get; set; }
        [Required, MaxLength(500)]
        public string Content { get; set; } = string.Empty;
        public long NurseId { get; set; }
        public DateTime RecordTime { get; set; } = DateTime.Now;

        [ForeignKey(nameof(InpatientId))] public virtual InpatientRecord? InpatientRecord { get; set; }
        [ForeignKey(nameof(PatientId))] public virtual PatientInfo? Patient { get; set; }
        [ForeignKey(nameof(MedicalOrderId))] public virtual MedicalOrder? MedicalOrder { get; set; }
        [ForeignKey(nameof(NurseId))] public virtual SysUser? Nurse { get; set; }
    }
}
