using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 医嘱
    /// </summary>
    [Table("MedicalOrder")]
    public class MedicalOrder : BaseEntity
    {
        /// <summary>住院ID</summary>
        public long InpatientId { get; set; }

        /// <summary>患者ID</summary>
        public long PatientId { get; set; }

        /// <summary>医生ID</summary>
        public long DoctorId { get; set; }

        /// <summary>医嘱类型 1:长期 2:临时</summary>
        public int OrderType { get; set; }

        /// <summary>医嘱内容</summary>
        [Required, MaxLength(500)]
        public string OrderContent { get; set; } = string.Empty;

        /// <summary>开始时间</summary>
        public DateTime StartTime { get; set; } = DateTime.Now;

        /// <summary>结束时间</summary>
        public DateTime? EndTime { get; set; }

        /// <summary>状态 1:已下达 2:执行中 3:已完成 4:已停止</summary>
        public int Status { get; set; } = 1;

        /// <summary>执行护士ID</summary>
        public long? ExecutorId { get; set; }

        /// <summary>执行时间</summary>
        public DateTime? ExecuteTime { get; set; }

        // 导航属性
        [ForeignKey(nameof(InpatientId))]
        public virtual InpatientRecord? InpatientRecord { get; set; }

        [ForeignKey(nameof(PatientId))]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public virtual DoctorInfo? Doctor { get; set; }
    }
}
