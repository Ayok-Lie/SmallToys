using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using OfficeOpenXml;

namespace Ayok.Excel.Services
{
    public class ExcelImportService : IExcelImportService
    {
        public List<T> ImportData<T>(Stream stream, bool hasHeader = true)
            where T : new()
        {
            using ExcelPackage excelPackage = new ExcelPackage(stream);
            ExcelWorksheet excelWorksheet =
                excelPackage.Workbook.Worksheets.FirstOrDefault()
                ?? throw new InvalidOperationException("Excel文件中未找到有效工作表");
            List<T> list = new List<T>();
            PropertyInfo[] properties = typeof(T).GetProperties();
            Dictionary<int, PropertyInfo> dictionary = new Dictionary<int, PropertyInfo>();
            int num = ((!hasHeader) ? 1 : 2);
            if (hasHeader)
            {
                ExcelRangeBase dimension = excelWorksheet.Dimension;
                if (dimension != null && dimension.Rows >= 1)
                {
                    for (int i = 1; i <= excelWorksheet.Dimension.End.Column; i++)
                    {
                        string headerName = excelWorksheet.Cells[1, i].Text.Trim();
                        PropertyInfo propertyInfo = properties.FirstOrDefault(
                            (PropertyInfo p) =>
                                p.Name.Equals(headerName, StringComparison.OrdinalIgnoreCase)
                                || (p.GetCustomAttribute<ColumnAttribute>()?.Name ?? "")
                                    == headerName
                        );
                        if (propertyInfo != null)
                        {
                            dictionary[i] = propertyInfo;
                            continue;
                        }
                        throw new InvalidOperationException(
                            "未找到与列名'" + headerName + "'对应的实体属性，请检查映射配置"
                        );
                    }
                    goto IL_0138;
                }
            }
            for (
                int num2 = 1;
                num2 <= Math.Min(properties.Length, excelWorksheet.Dimension.End.Column);
                num2++
            )
            {
                dictionary[num2] = properties[num2 - 1];
            }
            goto IL_0138;
            IL_0138:
            for (int num3 = num; num3 <= excelWorksheet.Dimension.End.Row; num3++)
            {
                T val = new T();
                bool flag = false;
                foreach (KeyValuePair<int, PropertyInfo> item in dictionary)
                {
                    int key = item.Key;
                    PropertyInfo value = item.Value;
                    ExcelRange excelRange = excelWorksheet.Cells[num3, key];
                    if (excelRange.Value != null)
                    {
                        flag = true;
                        try
                        {
                            object value2 = ConvertValue(excelRange.Value, value.PropertyType);
                            value.SetValue(val, value2);
                        }
                        catch (Exception ex)
                        {
                            throw new InvalidOperationException(
                                $"第{num3}行第{key}列数据转换失败({value.Name}): {ex.Message}"
                            );
                        }
                    }
                }
                if (flag)
                {
                    list.Add(val);
                }
            }
            return list;
        }

        private static object ConvertValue(object cellValue, Type targetType)
        {
            if (cellValue == null)
            {
                if (!targetType.IsValueType)
                {
                    return null;
                }
                return Activator.CreateInstance(targetType);
            }
            Type type = Nullable.GetUnderlyingType(targetType) ?? targetType;
            if (type == typeof(DateTime))
            {
                if (cellValue is DateTime dateTime)
                {
                    return dateTime;
                }
                if (cellValue is double d)
                {
                    return DateTime.FromOADate(d);
                }
                if (double.TryParse(cellValue.ToString(), out var result))
                {
                    return DateTime.FromOADate(result);
                }
            }
            if (type.IsEnum)
            {
                try
                {
                    return (cellValue is string value)
                        ? Enum.Parse(type, value, ignoreCase: true)
                        : Enum.ToObject(type, cellValue);
                }
                catch (ArgumentException)
                {
                    throw new InvalidCastException(
                        $"值 '{cellValue}' 无法转换为枚举类型 {type.Name}"
                    );
                }
            }
            if (type == typeof(bool))
            {
                switch (cellValue.ToString().Trim().ToLower())
                {
                    case "是":
                    case "true":
                    case "1":
                        return true;
                    case "否":
                    case "false":
                    case "0":
                        return false;
                }
            }
            try
            {
                return Convert.ChangeType(cellValue, type);
            }
            catch (InvalidCastException)
            {
                if (type == typeof(decimal) && cellValue is double value2)
                {
                    return Convert.ToDecimal(value2);
                }
                throw;
            }
        }
    }
}
