using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HIS.Models.Entities
{
    /// <summary>
    /// 收费记录
    /// </summary>
    [Table("ChargeRecord")]
    public class ChargeRecord : BaseEntity
    {
        /// <summary>收费单号</summary>
        [Required, MaxLength(50)]
        public string ChargeNo { get; set; } = string.Empty;

        /// <summary>患者ID</summary>
        public long PatientId { get; set; }

        /// <summary>收费类型 1:挂号 2:门诊 3:住院预交 4:住院结算</summary>
        public int ChargeType { get; set; }

        /// <summary>关联业务ID</summary>
        public long? RelatedId { get; set; }

        /// <summary>应收金额</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        /// <summary>实收金额</summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal PaidAmount { get; set; }

        /// <summary>支付方式 1:现金 2:微信 3:支付宝 4:银行卡 5:医保</summary>
        public int PaymentMethod { get; set; }

        /// <summary>状态 1:已收费 2:已退费</summary>
        public int Status { get; set; } = 1;

        /// <summary>收费员ID</summary>
        public long CreateUserId { get; set; }

        /// <summary>备注</summary>
        [MaxLength(500)]
        public string? Remark { get; set; }

        // 导航属性
        [ForeignKey(nameof(PatientId))]
        public virtual PatientInfo? Patient { get; set; }

        [ForeignKey(nameof(CreateUserId))]
        public virtual SysUser? CreateUser { get; set; }
    }
}
