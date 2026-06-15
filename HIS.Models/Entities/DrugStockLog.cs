using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 药品库存日志
    /// </summary>
    [Table("DrugStockLog")]
    public class DrugStockLog : BaseEntity
    {
        /// <summary>药品ID</summary>
        public long DrugId { get; set; }

        /// <summary>变更类型 1:入库 2:出库 3:发药 4:退药 5:盘点</summary>
        public int ChangeType { get; set; }

        /// <summary>变更数量(+/-)</summary>
        public int ChangeQuantity { get; set; }

        /// <summary>变更前数量</summary>
        public int BeforeQuantity { get; set; }

        /// <summary>变更后数量</summary>
        public int AfterQuantity { get; set; }

        /// <summary>关联单号</summary>
        [MaxLength(50)]
        public string? RelatedNo { get; set; }

        /// <summary>备注</summary>
        [MaxLength(200)]
        public string? Remark { get; set; }

        /// <summary>操作人ID</summary>
        public long CreateUserId { get; set; }

        // 导航属性
        [ForeignKey(nameof(DrugId))]
        public virtual DrugInfo? Drug { get; set; }
    }
}
