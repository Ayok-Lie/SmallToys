using System.Reflection;
using Ayok.Excel.Attributes;
using Ayok.Excel.Enums;
using OfficeOpenXml;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;

namespace Ayok.Excel
{
    public class ExcelUtil
    {
        static ExcelUtil()
        {
            //ExcelPackage.License.SetNonCommercialPersonal("Ayok");
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        //public static byte[] ExportData<T>(List<T> data) where T : class, new()
        //{
        //    ExcelSheetAttribute excelSheetAttribute = typeof(T).GetCustomAttribute<ExcelSheetAttribute>();
        //    if (excelSheetAttribute == null)
        //    {
        //        excelSheetAttribute = new ExcelSheetAttribute();
        //    }
        //    Dictionary<string, IEnumerable<Attribute>> attributes = GetAttributes<T>();
        //    List<PropertyInfo> properties = GetProperties<T>();
        //    using ExcelPackage excelPackage = new ExcelPackage();
        //    ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add(excelSheetAttribute.Name);
        //    for (int i = 0; i < properties.Count; i++)
        //    {
        //        int row = 1;
        //        int col = i + 1;
        //        ExcelRange excelRange = excelWorksheet.Cells[row, col];
        //        PropertyInfo propertyInfo = properties[i];
        //        if (attributes.TryGetValue(propertyInfo.Name, out var value))
        //        {
        //            foreach (Attribute item in value)
        //            {
        //                if (item is ExcelColumnAttribute excelColumnAttribute)
        //                {
        //                    excelRange.Value = excelColumnAttribute.Name ?? propertyInfo.Name;
        //                }
        //                else if (item is ExcelHeaderStyleAttribute excelHeaderStyleAttribute)
        //                {
        //                    excelRange.Style.Font.Color.SetColor(excelHeaderStyleAttribute.FontColor);
        //                    excelRange.Style.Font.Size = excelHeaderStyleAttribute.FontSize;
        //                }
        //            }
        //        }
        //        if (!excelSheetAttribute.AutoColumnWidth)
        //        {
        //            excelWorksheet.Column(col).AutoFit(excelSheetAttribute.ColumnWidth);
        //        }
        //    }
        //    for (int j = 0; j < data.Count; j++)
        //    {
        //        for (int k = 0; k < properties.Count; k++)
        //        {
        //            int num = j + 2;
        //            int num2 = k + 1;
        //            ExcelRange excelRange2 = excelWorksheet.Cells[num, num2];
        //            PropertyInfo propertyInfo2 = properties[k];
        //            object value2 = propertyInfo2.GetValue(data[j]);
        //            if (value2 == null || string.IsNullOrEmpty(value2.ToString()) || !attributes.TryGetValue(propertyInfo2.Name, out var value3))
        //            {
        //                continue;
        //            }
        //            foreach (Attribute item2 in value3)
        //            {
        //                if (item2 is ExcelColumnAttribute excelColumnAttribute2)
        //                {
        //                    switch (excelColumnAttribute2.Type)
        //                    {
        //                        case ExcelColumnEnum.常规:
        //                            excelRange2.Value = value2;
        //                            break;
        //                        case ExcelColumnEnum.超链接:
        //                            excelRange2.Hyperlink = new Uri(value2.ToString());
        //                            break;
        //                        case ExcelColumnEnum.本地图片:
        //                            {
        //                                string path = value2.ToString();
        //                                if (File.Exists(path))
        //                                {
        //                                    using Image image = Image.Load(path);
        //                                    using MemoryStream memoryStream = new MemoryStream();
        //                                    image.Save(memoryStream, new PngEncoder());
        //                                    SetPicture(excelWorksheet, num, num2, memoryStream);
        //                                }
        //                                else
        //                                {
        //                                    excelRange2.Value = $"图片路径无效：{value2}";
        //                                }
        //                                break;
        //                            }
        //                        case ExcelColumnEnum.网络图片:
        //                            {
        //                                if (Uri.TryCreate(value2.ToString(), UriKind.Absolute, out Uri result))
        //                                {
        //                                    using HttpClient httpClient = new HttpClient();
        //                                    using MemoryStream ms = new MemoryStream(httpClient.GetByteArrayAsync(result).Result);
        //                                    SetPicture(excelWorksheet, num, num2, ms);
        //                                }
        //                                else
        //                                {
        //                                    excelRange2.Value = $"图片地址无效：{value2}";
        //                                }
        //                                break;
        //                            }
        //                        case ExcelColumnEnum.自动类型:
        //                            if (propertyInfo2.PropertyType == typeof(string))
        //                            {
        //                                excelRange2.Value = value2.ToString();
        //                            }
        //                            else if (propertyInfo2.PropertyType == typeof(int) || propertyInfo2.PropertyType == typeof(int?))
        //                            {
        //                                excelRange2.Value = int.Parse(value2.ToString());
        //                            }
        //                            else if (propertyInfo2.PropertyType == typeof(long) || propertyInfo2.PropertyType == typeof(long?))
        //                            {
        //                                excelRange2.Value = long.Parse(value2.ToString());
        //                            }
        //                            else if (propertyInfo2.PropertyType == typeof(short) || propertyInfo2.PropertyType == typeof(short?))
        //                            {
        //                                excelRange2.Value = short.Parse(value2.ToString());
        //                            }
        //                            else if (propertyInfo2.PropertyType == typeof(decimal) || propertyInfo2.PropertyType == typeof(decimal?))
        //                            {
        //                                excelRange2.Value = decimal.Parse(value2.ToString());
        //                            }
        //                            else if (propertyInfo2.PropertyType == typeof(double) || propertyInfo2.PropertyType == typeof(double?))
        //                            {
        //                                excelRange2.Value = double.Parse(value2.ToString());
        //                            }
        //                            else if (propertyInfo2.PropertyType == typeof(float) || propertyInfo2.PropertyType == typeof(float?))
        //                            {
        //                                excelRange2.Value = float.Parse(value2.ToString());
        //                            }
        //                            else if (propertyInfo2.PropertyType == typeof(DateTime) || propertyInfo2.PropertyType == typeof(DateTime?))
        //                            {
        //                                excelRange2.Value = value2.ToString();
        //                            }
        //                            else
        //                            {
        //                                excelRange2.Value = value2;
        //                            }
        //                            break;
        //                    }
        //                }
        //                else if (item2 is ExcelColumnStyleAttribute excelColumnStyleAttribute)
        //                {
        //                    excelRange2.Style.Font.Color.SetColor(excelColumnStyleAttribute.FontColor);
        //                    excelRange2.Style.Font.Size = excelColumnStyleAttribute.FontSize;
        //                }
        //            }
        //        }
        //    }
        //    if (excelSheetAttribute.AutoColumnWidth)
        //    {
        //        excelWorksheet.Cells[excelWorksheet.Dimension.Address].AutoFitColumns();
        //    }
        //    return excelPackage.GetAsByteArray();
        //}

        public static byte[] ExportData<T>(List<T> data) where T : class, new()
        {
            using var excelPackage = new ExcelPackage();
            GenerateExcel(data, excelPackage);
            return excelPackage.GetAsByteArray();
        }

        public static void ExportData<T>(List<T> data, string filePath) where T : class, new()
        {
            using var excelPackage = new ExcelPackage();
            GenerateExcel(data, excelPackage);
            excelPackage.SaveAs(new FileInfo(filePath));
        }

        private static void GenerateExcel<T>(List<T> data, ExcelPackage excelPackage) where T : class, new()
        {
            var excelSheetAttribute = typeof(T).GetCustomAttribute<ExcelSheetAttribute>() ?? new ExcelSheetAttribute();
            var properties = GetProperties<T>();
            var attributes = GetAttributes<T>();

            var worksheet = excelPackage.Workbook.Worksheets.Add(excelSheetAttribute.Name);

            ProcessHeaderRow(worksheet, properties, attributes, excelSheetAttribute);
            ProcessDataRows(worksheet, data, properties, attributes);
            AdjustColumnWidths(worksheet, properties.Count, excelSheetAttribute);
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static List<T> ImportData<T>(string filePath) where T : class, new()
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("文件路径无效");
            }
            using ExcelPackage excel = new ExcelPackage(filePath);
            return ImportData<T>(excel);
        }

        /// <summary>
        /// 导入数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static List<T> ImportData<T>(Stream stream) where T : class, new()
        {
            using ExcelPackage excel = new ExcelPackage(stream);
            return ImportData<T>(excel);
        }

        private static List<T> ImportData<T>(ExcelPackage excel) where T : class, new()
        {
            var source = (from a in typeof(T).GetProperties()
                          where a.GetCustomAttribute<ExcelColumnAttribute>() != null
                          select new
                          {
                              Property = a,
                              ColumnName = a.GetCustomAttribute<ExcelColumnAttribute>().Name
                          }).ToList();
            ExcelWorksheet excelWorksheet = excel.Workbook.Worksheets[0];
            Dictionary<int, PropertyInfo> dictionary = new Dictionary<int, PropertyInfo>();
            for (int num = 1; num <= excelWorksheet.Dimension.Columns; num++)
            {
                ExcelRange cell = excelWorksheet.Cells[1, num];
                var anon = source.FirstOrDefault(a => a.ColumnName == cell.Value.ToString());
                if (anon != null)
                {
                    dictionary.Add(num, anon.Property);
                }
            }
            List<T> list = new List<T>();
            for (int num2 = 2; num2 <= excelWorksheet.Dimension.Rows; num2++)
            {
                T val = new T();
                for (int num3 = 1; num3 <= excelWorksheet.Dimension.Columns; num3++)
                {
                    try
                    {
                        object value = excelWorksheet.Cells[num2, num3].Value;
                        if (value != null && dictionary.TryGetValue(num3, out var value2))
                        {
                            Type conversionType = Nullable.GetUnderlyingType(value2.PropertyType) ?? value2.PropertyType;
                            object value3 = Convert.ChangeType(value, conversionType);
                            value2.SetValue(val, value3);
                        }
                    }
                    catch (Exception innerException)
                    {
                        throw new Exception($"导入失败：第{num2}行，第{num3}列数据类型转换失败！", innerException);
                    }
                }
                list.Add(val);
            }
            return list;
        }

        private static Dictionary<string, IEnumerable<Attribute>> GetAttributes<T>() where T : class, new()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();
            Dictionary<string, IEnumerable<Attribute>> dictionary = new Dictionary<string, IEnumerable<Attribute>>();
            PropertyInfo[] array = properties;
            foreach (PropertyInfo propertyInfo in array)
            {
                IEnumerable<Attribute> customAttributes = propertyInfo.GetCustomAttributes();
                dictionary.Add(propertyInfo.Name, customAttributes);
            }
            return dictionary;
        }

        private static List<PropertyInfo> GetProperties<T>() where T : class, new()
        {
            return (from a in typeof(T).GetProperties()
                    where a.GetCustomAttribute<ExcelColumnAttribute>() != null
                    orderby a.GetCustomAttribute<ExcelColumnAttribute>().Order
                    select a).ToList();
        }

        private static void SetPicture(ExcelWorksheet worksheet, int rowNum, int columnNum, Stream ms)
        {
            OfficeOpenXml.Drawing.ExcelPicture excelPicture = worksheet.Drawings.AddPicture($"img_{DateTime.Now.Ticks}", ms);
            excelPicture.SetSize(20, 20);
            excelPicture.SetPosition(rowNum - 1, 0, columnNum - 1, 5);
        }


        private static void ProcessHeaderRow(ExcelWorksheet worksheet,
    List<PropertyInfo> properties,
    Dictionary<string, IEnumerable<Attribute>> attributes,
    ExcelSheetAttribute sheetAttribute)
        {
            for (int col = 0; col < properties.Count; col++)
            {
                var cell = worksheet.Cells[1, col + 1];
                var property = properties[col];
                if (attributes.TryGetValue(property.Name, out var attrs))
                {
                    foreach (var attr in attrs)
                    {
                        switch (attr)
                        {
                            case ExcelColumnAttribute columnAttr:
                                cell.Value = columnAttr.Name ?? property.Name;
                                break;
                            case ExcelHeaderStyleAttribute styleAttr:
                                ApplyCellStyle(cell.Style, styleAttr.FontColor, styleAttr.FontSize);
                                break;
                        }
                    }
                }
                if (!sheetAttribute.AutoColumnWidth)
                {
                    worksheet.Column(col + 1).AutoFit(sheetAttribute.ColumnWidth);
                }
            }
        }

        private static void ProcessDataRows<T>(ExcelWorksheet worksheet,
            List<T> data,
            List<PropertyInfo> properties,
            Dictionary<string, IEnumerable<Attribute>> attributes)
        {
            for (int rowIdx = 0; rowIdx < data.Count; rowIdx++)
            {
                var item = data[rowIdx];
                for (int colIdx = 0; colIdx < properties.Count; colIdx++)
                {
                    var cell = worksheet.Cells[rowIdx + 2, colIdx + 1];
                    var property = properties[colIdx];
                    var value = property.GetValue(item);

                    if (value == null || string.IsNullOrEmpty(value.ToString())) continue;
                    ProcessCellValue(cell, value, attributes.TryGetValue(property.Name, out var attrs) ? attrs : Enumerable.Empty<Attribute>());
                }
            }
        }

        private static void ProcessCellValue(ExcelRange cell, object value, IEnumerable<Attribute> attributes)
        {
            foreach (var attr in attributes)
            {
                switch (attr)
                {
                    case ExcelColumnAttribute columnAttr:
                        HandleColumnType(cell, value, columnAttr.Type);
                        break;
                    case ExcelColumnStyleAttribute styleAttr:
                        ApplyCellStyle(cell.Style, styleAttr.FontColor, styleAttr.FontSize);
                        break;
                }
            }
        }

        private static void HandleColumnType(ExcelRange cell, object value, ExcelColumnEnum columnType)
        {
            switch (columnType)
            {
                case ExcelColumnEnum.常规:
                    cell.Value = value;
                    break;
                case ExcelColumnEnum.超链接:
                    cell.Hyperlink = new Uri(value.ToString());
                    break;
                case ExcelColumnEnum.本地图片:
                    HandleLocalImage(cell, value.ToString());
                    break;
                case ExcelColumnEnum.网络图片:
                    HandleWebImage(cell, value.ToString());
                    break;
                case ExcelColumnEnum.自动类型:
                    SetAutoTypeValue(cell, value);
                    break;
            }
        }

        private static void HandleLocalImage(ExcelRange cell, string path)
        {
            if (File.Exists(path))
            {
                using var image = Image.Load(path);
                using var stream = new MemoryStream();
                image.Save(stream, new PngEncoder());
                SetPicture(cell.Worksheet, cell.Start.Row, cell.Start.Column, stream);
            }
            else
            {
                cell.Value = $"图片路径无效：{path}";
            }
        }

        private static void HandleWebImage(ExcelRange cell, string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                using HttpClient httpClient = new HttpClient();
                var imageData = httpClient.GetByteArrayAsync(uri).Result;
                using var stream = new MemoryStream(imageData);
                SetPicture(cell.Worksheet, cell.Start.Row, cell.Start.Column, stream);
            }
            else
            {
                cell.Value = $"图片地址无效：{url}";
            }
        }

        private static void SetAutoTypeValue(ExcelRange cell, object value)
        {
            var type = value.GetType();
            var stringValue = value.ToString();

            cell.Value = Type.GetTypeCode(type) switch
            {
                TypeCode.String => stringValue,
                TypeCode.Int32 => int.Parse(stringValue),
                TypeCode.Int64 => long.Parse(stringValue),
                TypeCode.Int16 => short.Parse(stringValue),
                TypeCode.Decimal => decimal.Parse(stringValue),
                TypeCode.Double => double.Parse(stringValue),
                TypeCode.Single => float.Parse(stringValue),
                TypeCode.DateTime => DateTime.Parse(stringValue),
                _ => value
            };
        }

        private static void AdjustColumnWidths(ExcelWorksheet worksheet, int columnCount, ExcelSheetAttribute sheetAttribute)
        {
            if (sheetAttribute.AutoColumnWidth)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }
            else if (sheetAttribute.ColumnWidth > 0)
            {
                for (int col = 1; col <= columnCount; col++)
                {
                    worksheet.Column(col).Width = sheetAttribute.ColumnWidth;
                    //worksheet.Column(col).Style.ShrinkToFit = true;
                    worksheet.Column(col).Style.HorizontalAlignment = ExcelHorizontalAlignment.Fill;
                }
            }
            //worksheet.Cells.Style.WrapText = true;//自动换行
            //worksheet.Cells.Style.ShrinkToFit = true;
        }

        private static void ApplyCellStyle(ExcelStyle style, System.Drawing.Color fontColor, float fontSize)
        {
            style.Font.Color.SetColor(fontColor);
            style.Font.Size = fontSize;
        }

        private static void SetPicture(ExcelWorksheet worksheet, int row, int col, MemoryStream imageStream)
        {
            //worksheet.Column(col).Width = 50;
            //worksheet.Row(row).Height = 50;
            var picture = worksheet.Drawings.AddPicture(Guid.NewGuid().ToString(), imageStream);
            //var picture = worksheet.Drawings.AddPicture(Guid.NewGuid().ToString(), "D:/file/obj.png", PictureLocation.None);
            picture.SetSize(20, 20);
            picture.SetPosition(row - 1, 0, col - 1, 5);
        }
    }
}
