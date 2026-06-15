using HIS.Common.Attributes;

namespace HIS.Models.DTOs
{
    public class DrugExportDto
    {
        [ExcelColumn("药品编码", 0)] public string DrugCode { get; set; } = "";
        [ExcelColumn("药品名称", 1)] public string DrugName { get; set; } = "";
        [ExcelColumn("通用名", 2)] public string? GenericName { get; set; }
        [ExcelColumn("分类", 3)] public string? CategoryName { get; set; }
        [ExcelColumn("规格", 4)] public string? Specification { get; set; }
        [ExcelColumn("单位", 5)] public string? Unit { get; set; }
        [ExcelColumn("生产厂家", 6)] public string? Manufacturer { get; set; }
        [ExcelColumn("零售价", 7)] public decimal RetailPrice { get; set; }
        [ExcelColumn("库存", 8)] public int StockQuantity { get; set; }
        [ExcelColumn("最低库存", 9)] public int MinStock { get; set; }
    }

    public class PatientExportDto
    {
        [ExcelColumn("患者编号", 0)] public string PatientNo { get; set; } = "";
        [ExcelColumn("姓名", 1)] public string Name { get; set; } = "";
        [ExcelColumn("性别", 2)] public string Gender { get; set; } = "";
        [ExcelColumn("年龄", 3)] public int? Age { get; set; }
        [ExcelColumn("身份证号", 4)] public string? IdCard { get; set; }
        [ExcelColumn("手机号", 5)] public string? Phone { get; set; }
        [ExcelColumn("地址", 6)] public string? Address { get; set; }
        [ExcelColumn("血型", 7)] public string? BloodType { get; set; }
        [ExcelColumn("过敏史", 8)] public string? AllergyHistory { get; set; }
    }

    public class ChargeExportDto
    {
        [ExcelColumn("收费单号", 0)] public string ChargeNo { get; set; } = "";
        [ExcelColumn("患者", 1)] public string? PatientName { get; set; }
        [ExcelColumn("类型", 2)] public string ChargeType { get; set; } = "";
        [ExcelColumn("应收金额", 3)] public decimal TotalAmount { get; set; }
        [ExcelColumn("实收金额", 4)] public decimal PaidAmount { get; set; }
        [ExcelColumn("支付方式", 5)] public string PaymentMethod { get; set; } = "";
        [ExcelColumn("状态", 6)] public string Status { get; set; } = "";
        [ExcelColumn("收费时间", 7)] public string? CreateTime { get; set; }
    }
}
