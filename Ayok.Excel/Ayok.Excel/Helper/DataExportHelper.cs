using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Reflection;
using Ayok.Excel.Model;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;

namespace Ayok.Excel.Helper
{
    public static class DataExportHelper
    {
        public static string FormatValueFromSubField(object? value, JsonExportField subField)
        {
            if (value == null)
            {
                return "";
            }
            switch (subField.FormatType?.ToLowerInvariant())
            {
                case "price":
                    return FormatPrice(value);
                case "datetime":
                    return FormatDateTime(value, "yyyy-MM-dd HH:mm:ss");
                case "date":
                    return FormatDateTime(value, "yyyy-MM-dd");
                case "custom":
                    if (!string.IsNullOrEmpty(subField.FormatString))
                    {
                        return FormatCustom(value, subField.FormatString);
                    }
                    break;
                case "raw":
                case null:
                    return value.ToString() ?? "";
            }
            return value.ToString() ?? "";
        }

        public static object? DataConvert(object? value, Type type)
        {
            if (value == null)
            {
                return value;
            }
            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return ((DateTime)value).ToString("yyyy-MM-dd HH:mm:ss");
            }
            if (type == typeof(bool) || type == typeof(bool?))
            {
                if (!(bool)value)
                {
                    return "否";
                }
                return "是";
            }
            if (type.IsEnum)
            {
                Enum obj = (Enum)value;
                MemberInfo[] member = type.GetMember(obj.ToString());
                if (member.Length != 0)
                {
                    DisplayAttribute customAttribute = member[0]
                        .GetCustomAttribute<DisplayAttribute>();
                    if (customAttribute != null)
                    {
                        return customAttribute.Name ?? obj.ToString();
                    }
                }
                return obj.ToString();
            }
            return value;
        }

        private static string FormatPrice(object value)
        {
            if (decimal.TryParse(value.ToString(), out var result))
            {
                return $"￥{result:N2}";
            }
            return value.ToString() ?? "";
        }

        private static string FormatDateTime(object value, string format)
        {
            if (DateTime.TryParse(value.ToString(), out var result))
            {
                return result.ToString(format);
            }
            return value.ToString() ?? "";
        }

        private static string FormatCustom(object value, string formatString)
        {
            try
            {
                if (value is IFormattable formattable)
                {
                    return formattable.ToString(formatString, null);
                }
                return string.Format("{0:" + formatString + "}", value);
            }
            catch
            {
                return value.ToString() ?? "";
            }
        }

        public static void SetPicture(
            ExcelWorksheet worksheet,
            int rowNum,
            int columnNum,
            Stream ms
        )
        {
            ExcelPicture excelPicture = worksheet.Drawings.AddPicture(
                $"img_{DateTime.Now.Ticks}",
                ms
            );
            excelPicture.SetSize(80, 80);
            excelPicture.SetPosition(rowNum - 1, 2, columnNum - 1, 2);
            worksheet.Row(rowNum).Height = 60.0;
        }

        public static async Task ProcessUrlImageCell(
            ExcelRange cell,
            object value,
            ExcelWorksheet worksheet,
            int row,
            int column
        )
        {
            string text = value.ToString();
            if (string.IsNullOrEmpty(text))
            {
                cell.Value = "图片链接为空";
                return;
            }
            try
            {
                if (
                    !Uri.TryCreate(text, UriKind.Absolute, out Uri result)
                    || (result.Scheme != Uri.UriSchemeHttp && result.Scheme != Uri.UriSchemeHttps)
                )
                {
                    cell.Value = "图片地址无效：" + text;
                    return;
                }
                using HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(30.0);
                byte[] array = await httpClient.GetByteArrayAsync(result);
                if (array.Length > 10485760)
                {
                    cell.Value = "图片文件过大,不能超过10MB";
                    return;
                }
                using MemoryStream ms = new MemoryStream(array);
                SetPicture(worksheet, row, column, ms);
            }
            catch (HttpRequestException)
            {
                cell.Value = "图片下载失败";
            }
            catch (TaskCanceledException)
            {
                cell.Value = "图片下载超时";
            }
            catch (Exception ex3)
            {
                cell.Value = "图片处理错误: " + ex3.Message;
            }
        }

        public static async Task ProcessLocalImageCell(
            ExcelRange cell,
            object value,
            ExcelWorksheet worksheet,
            int row,
            int column
        )
        {
            string text = value.ToString();
            if (string.IsNullOrEmpty(text) || !File.Exists(text))
            {
                cell.Value = "图片不存在";
                return;
            }
            try
            {
                if (new FileInfo(text).Length > 5242880)
                {
                    cell.Value = "图片文件过大";
                    return;
                }
                ExcelPicture obj = await worksheet.Drawings.AddPictureAsync(
                    $"pic_{row}_{column}",
                    new FileInfo(text)
                );
                obj.SetPosition(row - 1, 2, column - 1, 2);
                obj.SetSize(80, 80);
                worksheet.Row(row).Height = 60.0;
                cell.Value = "";
            }
            catch (Exception)
            {
                cell.Value = "图片加载失败";
            }
        }

        public static void ProcessHyperlinkCell(
            ExcelRange cell,
            object value,
            JsonExportField field
        )
        {
            string text = (string)(cell.Value = value.ToString() ?? "");
            if (Uri.TryCreate(text, UriKind.Absolute, out Uri result))
            {
                cell.Hyperlink = result;
                cell.Style.Font.Color.SetColor(Color.Blue);
                cell.Style.Font.UnderLine = true;
            }
            else if (text.Contains('@'))
            {
                cell.Hyperlink = new Uri("mailto:" + text);
                cell.Style.Font.Color.SetColor(Color.Blue);
                cell.Style.Font.UnderLine = true;
            }
            else
            {
                cell.Value = "无效链接";
            }
        }

        public static void ProcessAutoTypeCellForSubField(
            ExcelRange cell,
            object value,
            PropertyInfo propertyInfo,
            JsonExportField subField
        )
        {
            Type propertyType = propertyInfo.PropertyType;
            Type type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (!string.IsNullOrEmpty(subField.FormatType))
            {
                string value2 = FormatValueFromSubField(value, subField);
                cell.Value = value2;
            }
            else if (type == typeof(string))
            {
                cell.Value = value.ToString();
            }
            else if (type == typeof(int))
            {
                cell.Value = Convert.ToInt32(value);
            }
            else if (type == typeof(long))
            {
                cell.Value = Convert.ToInt64(value);
            }
            else if (type == typeof(short))
            {
                cell.Value = Convert.ToInt16(value);
            }
            else if (type == typeof(decimal))
            {
                cell.Value = Convert.ToDecimal(value);
            }
            else if (type == typeof(double))
            {
                cell.Value = Convert.ToDouble(value);
            }
            else if (type == typeof(float))
            {
                cell.Value = Convert.ToSingle(value);
            }
            else if (type == typeof(DateTime))
            {
                string text = subField.DateTimeFormat ?? "yyyy-MM-dd HH:mm:ss";
                cell.Value = DateTime.Parse(value.ToString()).ToString(text);
            }
            else if (type == typeof(bool))
            {
                cell.Value = (((bool)value) ? "是" : "否");
            }
            else
            {
                cell.Value = value;
            }
        }

        public static async Task ProcessCellValueForSubField(
            ExcelRange cell,
            object? value,
            JsonExportField subField,
            PropertyInfo propertyInfo,
            ExcelWorksheet worksheet,
            int row,
            int column
        )
        {
            if (value != null)
            {
                switch (subField.ExcelColumnType?.ToLowerInvariant() ?? "常规")
                {
                    case "常规":
                    {
                        string value3 = FormatValueFromSubField(value, subField);
                        cell.Value = value3;
                        break;
                    }
                    case "自动类型":
                        ProcessAutoTypeCellForSubField(cell, value, propertyInfo, subField);
                        break;
                    case "超链接":
                        ProcessHyperlinkCell(cell, value, subField);
                        break;
                    case "本地图片":
                        await ProcessLocalImageCell(cell, value, worksheet, row, column);
                        break;
                    case "网络图片":
                        await ProcessUrlImageCell(cell, value, worksheet, row, column);
                        break;
                    default:
                    {
                        string value2 = FormatValueFromSubField(value, subField);
                        cell.Value = value2;
                        break;
                    }
                }
                if (subField.ColumnStyle != null)
                {
                    ExcelStyleHelper.SetColumnCellStyle(cell, subField.ColumnStyle);
                }
            }
        }

        public static int CalculateMaxCollectionRowCountWithNesting<T>(
            T item,
            JsonExportConfig config,
            List<PropertyInfo> allProperties
        )
            where T : class
        {
            int num = 1;
            foreach (JsonExportField field in config.Fields)
            {
                PropertyInfo propertyInfo = allProperties.FirstOrDefault(
                    (PropertyInfo p) => p.Name == field.SourceFieldName
                );
                if (!(propertyInfo != null))
                {
                    continue;
                }
                Type propertyType = propertyInfo.PropertyType;
                if (
                    !propertyType.IsGenericType
                    || !(propertyType.GetGenericTypeDefinition() == typeof(List<>))
                )
                {
                    continue;
                }
                IList list = (IList)propertyInfo.GetValue(item);
                if (list != null)
                {
                    int num2 = CalculateNestedCollectionRows(list);
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
            }
            return num;
        }

        public static int CalculateItemRowCount(
            object item,
            List<JsonExportField> subFields,
            Type elementType
        )
        {
            int num = 1;
            foreach (JsonExportField subField in subFields)
            {
                PropertyInfo property = elementType.GetProperty(subField.SourceFieldName);
                if (!(property != null))
                {
                    continue;
                }
                Type propertyType = property.PropertyType;
                if (
                    !propertyType.IsGenericType
                    || !(propertyType.GetGenericTypeDefinition() == typeof(List<>))
                )
                {
                    continue;
                }
                IList list = (IList)property.GetValue(item);
                if (list != null)
                {
                    int num2 = CalculateNestedCollectionRows(list);
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
            }
            return num;
        }

        public static int CalculateNestedCollectionRows(IList collection)
        {
            if (collection == null || collection.Count == 0)
            {
                return 0;
            }
            int num = 0;
            foreach (object item in collection)
            {
                int num2 = 1;
                List<PropertyInfo> list = (
                    from p in item.GetType().GetProperties()
                    where
                        p.PropertyType.IsGenericType
                        && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                    select p
                ).ToList();
                if (list.Any())
                {
                    int num3 = 0;
                    foreach (PropertyInfo item2 in list)
                    {
                        IList list2 = (IList)item2.GetValue(item);
                        if (list2 != null)
                        {
                            int num4 = CalculateNestedCollectionRows(list2);
                            if (num4 > num3)
                            {
                                num3 = num4;
                            }
                        }
                    }
                    num2 = Math.Max(num2, num3);
                }
                num += num2;
            }
            return num;
        }

        public static int GetTotalColumnCount<T>(JsonExportConfig config)
            where T : class
        {
            Type typeFromHandle = typeof(T);
            List<PropertyInfo> source = (
                config.IsDescendingClass
                    ? PropertiesHelper.GetBasePropertiesFirstWithoutNew(
                        typeFromHandle,
                        includeInherited: true
                    )
                    : PropertiesHelper.GetBasePropertiesFirstWithoutNew(
                        typeFromHandle,
                        includeInherited: false
                    )
            );
            int num = 0;
            foreach (JsonExportField field in config.Fields)
            {
                PropertyInfo propertyInfo = source.FirstOrDefault(
                    (PropertyInfo p) => p.Name == field.SourceFieldName
                );
                if (propertyInfo != null)
                {
                    num += ExcelStyleHelper.CalculateFieldColumnCount(field, propertyInfo);
                }
            }
            return num;
        }

        public static int CalculateSubFieldsColumnCount(
            List<JsonExportField> subFields,
            Type elementType
        )
        {
            int num = 0;
            foreach (JsonExportField subField in subFields)
            {
                PropertyInfo property = elementType.GetProperty(subField.SourceFieldName);
                if (property != null)
                {
                    Type propertyType = property.PropertyType;
                    if (
                        propertyType.IsGenericType
                        && propertyType.GetGenericTypeDefinition() == typeof(List<>)
                    )
                    {
                        List<JsonExportField>? subFields2 = subField.SubFields;
                        if (subFields2 != null && subFields2.Count > 0)
                        {
                            Type elementType2 = propertyType.GetGenericArguments()[0];
                            num += CalculateSubFieldsColumnCount(subField.SubFields, elementType2);
                        }
                        else
                        {
                            num++;
                        }
                    }
                    else
                    {
                        num++;
                    }
                }
                else
                {
                    num++;
                }
            }
            return num;
        }

        public static int WriteBasicHeader(
            ExcelWorksheet worksheet,
            JsonExportField field,
            int startRow,
            int endRow,
            int currentColumn
        )
        {
            worksheet.Cells[startRow, currentColumn].Value = field.ColumnName;
            if (startRow != endRow)
            {
                worksheet.Cells[startRow, currentColumn, endRow, currentColumn].Merge = true;
            }
            if (field.HeaderStyle != null)
            {
                ExcelStyleHelper.SetHeaderCellStyle(
                    worksheet.Cells[startRow, currentColumn],
                    field.HeaderStyle
                );
            }
            return currentColumn + 1;
        }
    }
}
