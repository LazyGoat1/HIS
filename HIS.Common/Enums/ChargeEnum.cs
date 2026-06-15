namespace HIS.Common.Enums
{
    /// <summary>
    /// 收费类型
    /// </summary>
    public enum ChargeTypeEnum
    {
        /// <summary>挂号费</summary>
        Registration = 1,
        /// <summary>门诊收费</summary>
        Outpatient = 2,
        /// <summary>住院预交金</summary>
        Deposit = 3,
        /// <summary>住院结算</summary>
        Settlement = 4
    }

    /// <summary>
    /// 支付方式
    /// </summary>
    public enum PaymentMethodEnum
    {
        /// <summary>现金</summary>
        Cash = 1,
        /// <summary>微信</summary>
        WeChat = 2,
        /// <summary>支付宝</summary>
        Alipay = 3,
        /// <summary>银行卡</summary>
        BankCard = 4,
        /// <summary>医保</summary>
        MedicalInsurance = 5
    }

    /// <summary>
    /// 收费状态
    /// </summary>
    public enum ChargeStatusEnum
    {
        /// <summary>已收费</summary>
        Charged = 1,
        /// <summary>已退费</summary>
        Refunded = 2
    }
}
