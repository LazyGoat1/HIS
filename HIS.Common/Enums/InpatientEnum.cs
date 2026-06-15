namespace HIS.Common.Enums
{
    /// <summary>
    /// 住院状态
    /// </summary>
    public enum InpatientStatusEnum
    {
        /// <summary>在院</summary>
        InHospital = 1,
        /// <summary>出院</summary>
        Discharged = 2,
        /// <summary>转科</summary>
        Transferred = 3
    }

    /// <summary>
    /// 床位状态
    /// </summary>
    public enum BedStatusEnum
    {
        /// <summary>空闲</summary>
        Available = 1,
        /// <summary>占用</summary>
        Occupied = 2,
        /// <summary>维修</summary>
        Maintenance = 3
    }

    /// <summary>
    /// 床位类型
    /// </summary>
    public enum BedTypeEnum
    {
        /// <summary>普通病房</summary>
        Normal = 1,
        /// <summary>双人间</summary>
        Double = 2,
        /// <summary>单人间</summary>
        Single = 3,
        /// <summary>VIP</summary>
        VIP = 4
    }

    /// <summary>
    /// 医嘱类型
    /// </summary>
    public enum MedicalOrderTypeEnum
    {
        /// <summary>长期医嘱</summary>
        LongTerm = 1,
        /// <summary>临时医嘱</summary>
        Temporary = 2
    }

    /// <summary>
    /// 医嘱状态
    /// </summary>
    public enum MedicalOrderStatusEnum
    {
        /// <summary>已下达</summary>
        Issued = 1,
        /// <summary>执行中</summary>
        Executing = 2,
        /// <summary>已完成</summary>
        Completed = 3,
        /// <summary>已停止</summary>
        Stopped = 4
    }
}
