using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 药品信息
    /// </summary>
    [Table("DrugInfo")]
    public class DrugInfo : BaseEntity
    {
        /// <summary>药品编码</summary>
        [Required, MaxLength(50)]
        public string DrugCode { get; set; } = string.Empty;

        /// <summary>药品名称</summary>
        [Required, MaxLength(100)]
        public string DrugName { get; set; } = string.Empty;

        /// <summary>通用名</summary>
        [MaxLength(100)]
        public string? GenericName { get; set; }

        /// <summary>药品分类ID</summary>
        public long? CategoryId { get; set; }

        /// <summary>规格</summary>
        [MaxLength(100)]
        public string? Specification { get; set; }

        /// <summary>单位</summary>
        [MaxLength(20)]
        public string? Unit { get; set; }

        /// <summary>生产厂家</summary>
        [MaxLength(200)]
        public string? Manufacturer { get; set; }

        /// <summary>进货价</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal UnitPrice { get; set; }

        /// <summary>零售价</summary>
        [Column(TypeName = "decimal(18,4)")]
        public decimal RetailPrice { get; set; }

        /// <summary>库存数量</summary>
        public int StockQuantity { get; set; }

        /// <summary>最低库存预警</summary>
        public int MinStock { get; set; } = 10;

        /// <summary>是否处方药</summary>
        public bool IsPrescription { get; set; }

        /// <summary>状态 0:停用 1:启用</summary>
        public int Status { get; set; } = 1;

        // 导航属性
        [ForeignKey(nameof(CategoryId))]
        public virtual DrugCategory? Category { get; set; }

        public virtual ICollection<DrugStockLog> StockLogs { get; set; } = new List<DrugStockLog>();
    }
}
