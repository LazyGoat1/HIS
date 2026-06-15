namespace HIS.Common.Attributes
{
    /// <summary>标记 DTO 属性对应的 Excel 列名和序号</summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExcelColumnAttribute : Attribute
    {
        /// <summary>Excel 列标题</summary>
        public string Title { get; set; }

        /// <summary>列序号（从 0 开始）</summary>
        public int Order { get; set; }

        /// <summary>是否必填</summary>
        public bool Required { get; set; }

        public ExcelColumnAttribute(string title, int order, bool required = false)
        {
            Title = title;
            Order = order;
            Required = required;
        }
    }
}
