using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>系统配置</summary>
    [Table("SysConfig")]
    public class SysConfig : BaseEntity
    {
        [Required, MaxLength(50)]
        public string ConfigKey { get; set; } = string.Empty;
        [Required, MaxLength(500)]
        public string ConfigValue { get; set; } = string.Empty;
        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
