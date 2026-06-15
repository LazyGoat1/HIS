using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 处方明细
    /// </summary>
    [Table("PrescriptionDetail")]
    public class PrescriptionDetail : BaseEntity
    {
        /// <summary>处方ID</summary>
        public long PrescriptionId { get; set; }

        /// <summary>项目类型 1:药品 2:检查项目</summary>
        public int ItemType { get; set; }

        /// <summary>项目ID(药品ID或检查项目ID)</summary>
        public long ItemId { get; set; }

        /// <summary>项目名称</summary>
        [Required, MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>规格</summary>
        [MaxLength(100)]
        public string? Specification { get; set; }

        /// <summary>单位</summary>
        [MaxLength(20)]
        public string? Unit { get; set; }

        /// <summary>单价</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        /// <summary>数量</summary>
        public int Quantity { get; set; }

        /// <summary>金额</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>用法</summary>
        [MaxLength(200)]
        public string? Usage { get; set; }

        /// <summary>用量</summary>
        [MaxLength(100)]
        public string? Dosage { get; set; }

        /// <summary>频次</summary>
        [MaxLength(50)]
        public string? Frequency { get; set; }

        /// <summary>天数</summary>
        public int? Days { get; set; }

        // 导航属性
        [ForeignKey(nameof(PrescriptionId))]
        public virtual Prescription? Prescription { get; set; }
    }
}
