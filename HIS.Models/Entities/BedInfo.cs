using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 床位信息
    /// </summary>
    [Table("BedInfo")]
    public class BedInfo : BaseEntity
    {
        /// <summary>床位号</summary>
        [Required, MaxLength(20)]
        public string BedNo { get; set; } = string.Empty;

        /// <summary>房间号</summary>
        [Required, MaxLength(20)]
        public string RoomNo { get; set; } = string.Empty;

        /// <summary>所属科室ID</summary>
        public long DepartmentId { get; set; }

        /// <summary>床位类型 1:普通 2:双人 3:单人 4:VIP</summary>
        public int BedType { get; set; }

        /// <summary>日床位费</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal DailyRate { get; set; }

        /// <summary>状态 1:空闲 2:占用 3:维修</summary>
        public int Status { get; set; } = 1;

        // 导航属性
        [ForeignKey(nameof(DepartmentId))]
        public virtual SysDepartment? Department { get; set; }

        public virtual ICollection<InpatientRecord> InpatientRecords { get; set; } = new List<InpatientRecord>();
    }
}
