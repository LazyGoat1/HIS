namespace HIS.Common.Enums
{
    /// <summary>
    /// 挂号类型
    /// </summary>
    public enum RegistrationTypeEnum
    {
        /// <summary>普通号</summary>
        Normal = 1,
        /// <summary>专家号</summary>
        Expert = 2,
        /// <summary>急诊</summary>
        Emergency = 3
    }

    /// <summary>
    /// 挂号状态
    /// </summary>
    public enum RegistrationStatusEnum
    {
        /// <summary>已挂号</summary>
        Registered = 1,
        /// <summary>已接诊</summary>
        Consulted = 2,
        /// <summary>已退号</summary>
        Refunded = 3
    }
}
