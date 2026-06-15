using System.Reflection;
using HIS.Common.Attributes;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace HIS.Common.Helpers
{
    /// <summary>泛型 Excel 导入导出工具</summary>
    public static class ExcelHelper
    {
        /// <summary>导出数据为 Excel</summary>
        public static byte[] Export<T>(IEnumerable<T> data)
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.LightBlue.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            var headerFont = workbook.CreateFont(); headerFont.IsBold = true; headerStyle.SetFont(headerFont);

            var props = GetExcelProperties<T>().OrderBy(p => p.Attr.Order).ToList();
            var headerRow = sheet.CreateRow(0);
            for (int i = 0; i < props.Count; i++)
            {
                var cell = headerRow.CreateCell(i);
                cell.SetCellValue(props[i].Attr.Title);
                cell.CellStyle = headerStyle;
                sheet.SetColumnWidth(i, Math.Max(props[i].Attr.Title.Length * 512, 3000));
            }

            int rowIdx = 1;
            foreach (var item in data)
            {
                var row = sheet.CreateRow(rowIdx++);
                for (int i = 0; i < props.Count; i++)
                {
                    var val = props[i].Prop.GetValue(item);
                    row.CreateCell(i).SetCellValue(val?.ToString() ?? "");
                }
            }
            using var ms = new MemoryStream();
            workbook.Write(ms);
            return ms.ToArray();
        }

        /// <summary>生成导入模板（第一行为表头）</summary>
        public static byte[] GenerateTemplate<T>()
        {
            var workbook = new XSSFWorkbook();
            var sheet = workbook.CreateSheet("Sheet1");
            var headerRow = sheet.CreateRow(0);
            var headerStyle = workbook.CreateCellStyle();
            headerStyle.FillForegroundColor = IndexedColors.LightBlue.Index;
            headerStyle.FillPattern = FillPattern.SolidForeground;
            var headerFont = workbook.CreateFont();
            headerFont.IsBold = true;
            headerStyle.SetFont(headerFont);

            var props = GetExcelProperties<T>();
            foreach (var (prop, attr) in props.OrderBy(p => p.Attr.Order))
            {
                var cell = headerRow.CreateCell(attr.Order);
                cell.SetCellValue(attr.Title);
                cell.CellStyle = headerStyle;
                sheet.SetColumnWidth(attr.Order, Math.Max(attr.Title.Length * 512, 3000));
            }
            // 添加示例行
            var exampleRow = sheet.CreateRow(1);
            foreach (var (_, attr) in props.OrderBy(p => p.Attr.Order))
                exampleRow.CreateCell(attr.Order).SetCellValue(attr.Required ? "(必填)" : "(可选)");

            using var ms = new MemoryStream();
            workbook.Write(ms);
            return ms.ToArray();
        }

        /// <summary>解析 Excel 为对象列表</summary>
        public static (List<T> Data, List<string> Errors) Parse<T>(Stream stream) where T : new()
        {
            var result = new List<T>();
            var errors = new List<string>();
            var workbook = new XSSFWorkbook(stream);
            var sheet = workbook.GetSheetAt(0);
            if (sheet.LastRowNum < 1) { errors.Add("Excel 文件无数据行"); return (result, errors); }

            // 读表头建立列名→序号映射
            var headerRow = sheet.GetRow(0);
            var headerMap = new Dictionary<string, int>();
            for (int i = 0; i < headerRow.LastCellNum; i++)
            {
                var val = headerRow.GetCell(i)?.ToString()?.Trim();
                if (!string.IsNullOrEmpty(val)) headerMap[val] = i;
            }

            var props = GetExcelProperties<T>();
            for (int r = 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                if (row == null) continue;
                bool isEmpty = true;
                for (int c = 0; c < row.LastCellNum; c++)
                { if (row.GetCell(c)?.ToString()?.Trim() is string v && v != "") { isEmpty = false; break; } }
                if (isEmpty) continue;

                var obj = new T();
                bool rowOk = true;
                foreach (var (prop, attr) in props)
                {
                    if (!headerMap.TryGetValue(attr.Title, out var col)) continue;
                    var cell = row.GetCell(col);
                    var cellValue = GetCellString(cell);
                    if (attr.Required && string.IsNullOrWhiteSpace(cellValue))
                    { errors.Add($"第{r + 1}行「{attr.Title}」为必填项"); rowOk = false; continue; }
                    try { SetProperty(obj, prop, cellValue); }
                    catch (Exception ex) { errors.Add($"第{r + 1}行「{attr.Title}」格式错误: {ex.Message}"); rowOk = false; }
                }
                if (rowOk) result.Add(obj);
            }
            return (result, errors);
        }

        private static List<(PropertyInfo Prop, ExcelColumnAttribute Attr)> GetExcelProperties<T>()
        {
            var withAttr = typeof(T).GetProperties()
                .Select(p => (Prop: p, Attr: p.GetCustomAttribute<ExcelColumnAttribute>()))
                .Where(x => x.Attr != null).ToList();

            if (withAttr.Any()) return withAttr;

            // 无 ExcelColumn 属性时，自动用所有公共属性（按名称排序）
            return typeof(T).GetProperties()
                .Where(p => p.CanRead)
                .Select((p, i) => (Prop: p, Attr: new ExcelColumnAttribute(p.Name, i)))
                .ToList();
        }

        private static string GetCellString(ICell? cell)
        {
            if (cell == null) return "";
            return cell.CellType switch
            {
                CellType.Numeric => cell.NumericCellValue.ToString(),
                CellType.String => cell.StringCellValue?.Trim() ?? "",
                _ => cell.ToString()?.Trim() ?? ""
            };
        }

        private static void SetProperty(object obj, PropertyInfo prop, string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
            if (targetType == typeof(string)) prop.SetValue(obj, value);
            else if (targetType == typeof(int)) prop.SetValue(obj, int.Parse(value));
            else if (targetType == typeof(long)) prop.SetValue(obj, long.Parse(value));
            else if (targetType == typeof(decimal)) prop.SetValue(obj, decimal.Parse(value));
            else if (targetType == typeof(double)) prop.SetValue(obj, double.Parse(value));
            else if (targetType == typeof(bool)) prop.SetValue(obj, bool.Parse(value));
            else if (targetType == typeof(DateTime)) prop.SetValue(obj, DateTime.Parse(value));
            else if (targetType.IsEnum) prop.SetValue(obj, Enum.Parse(targetType, value));
        }
    }
}
