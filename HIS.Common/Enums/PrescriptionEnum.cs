namespace HIS.Common.Enums
{
    /// <summary>
    /// 处方类型
    /// </summary>
    public enum PrescriptionTypeEnum
    {
        /// <summary>西药</summary>
        Western = 1,
        /// <summary>中药</summary>
        Chinese = 2,
        /// <summary>检查</summary>
        Examination = 3
    }

    /// <summary>
    /// 处方状态
    /// </summary>
    public enum PrescriptionStatusEnum
    {
        /// <summary>已开具</summary>
        Issued = 1,
        /// <summary>已收费</summary>
        Charged = 2,
        /// <summary>已发药</summary>
        Dispensed = 3,
        /// <summary>已退方</summary>
        Refunded = 4
    }

    /// <summary>
    /// 处方明细项目类型
    /// </summary>
    public enum PrescriptionItemTypeEnum
    {
        /// <summary>药品</summary>
        Drug = 1,
        /// <summary>检查项目</summary>
        Examination = 2
    }
}
