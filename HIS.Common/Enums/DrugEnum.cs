namespace HIS.Common.Enums
{
    /// <summary>
    /// 药品库存变更类型
    /// </summary>
    public enum DrugStockChangeTypeEnum
    {
        /// <summary>入库</summary>
        StockIn = 1,
        /// <summary>出库</summary>
        StockOut = 2,
        /// <summary>发药</summary>
        Dispense = 3,
        /// <summary>退药</summary>
        Return = 4,
        /// <summary>盘点</summary>
        Check = 5
    }
}
