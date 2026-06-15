using HIS.Common.Attributes;

namespace HIS.Models.DTOs
{
    /// <summary>药品 Excel 导入模型</summary>
    public class DrugImportDto
    {
        [ExcelColumn("药品名称", 0, Required = true)]
        public string DrugName { get; set; } = string.Empty;

        [ExcelColumn("通用名", 1)]
        public string? GenericName { get; set; }

        [ExcelColumn("规格", 2)]
        public string? Specification { get; set; }

        [ExcelColumn("单位", 3)]
        public string? Unit { get; set; }

        [ExcelColumn("生产厂家", 4)]
        public string? Manufacturer { get; set; }

        [ExcelColumn("进货价", 5, Required = true)]
        public decimal UnitPrice { get; set; }

        [ExcelColumn("零售价", 6, Required = true)]
        public decimal RetailPrice { get; set; }

        [ExcelColumn("初始库存", 7)]
        public int StockQuantity { get; set; }

        [ExcelColumn("最低库存", 8)]
        public int MinStock { get; set; } = 10;

        [ExcelColumn("处方药(true/false)", 9)]
        public string IsPrescriptionStr { get; set; } = "false";

        [ExcelColumn("分类名称", 10)]
        public string? CategoryName { get; set; }
    }
}
