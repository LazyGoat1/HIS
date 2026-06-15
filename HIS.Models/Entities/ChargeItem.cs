using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>检查/收费项目字典（CT、MRI、X线、B超等）</summary>
    [Table("ChargeItem")]
    public class ChargeItem : BaseEntity
    {
        /// <summary>分类：CT / MRI / X线(DR) / 超声 / PET-CT / 核医学 / 内镜 / 心电</summary>
        [Required, MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        /// <summary>项目名称</summary>
        [Required, MaxLength(100)]
        public string ItemName { get; set; } = string.Empty;

        /// <summary>规格/备注（如：不含麻醉、需造影剂）</summary>
        [MaxLength(200)]
        public string? Specification { get; set; }

        /// <summary>单位</summary>
        [MaxLength(20)]
        public string? Unit { get; set; } = "次";

        /// <summary>单价</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        /// <summary>说明（检查部位/临床意义）</summary>
        [MaxLength(500)]
        public string? Description { get; set; }

        /// <summary>状态 0:停用 1:启用</summary>
        public int Status { get; set; } = 1;
    }
}
