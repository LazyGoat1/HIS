using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 药品分类
    /// </summary>
    [Table("DrugCategory")]
    public class DrugCategory : BaseEntity
    {
        /// <summary>分类名称</summary>
        [Required, MaxLength(50)]
        public string CategoryName { get; set; } = string.Empty;

        /// <summary>上级分类ID</summary>
        public long? ParentId { get; set; }

        /// <summary>排序</summary>
        public int SortOrder { get; set; }

        // 导航属性
        [ForeignKey(nameof(ParentId))]
        public virtual DrugCategory? Parent { get; set; }

        public virtual ICollection<DrugCategory> Children { get; set; } = new List<DrugCategory>();
        public virtual ICollection<DrugInfo> Drugs { get; set; } = new List<DrugInfo>();
    }
}
