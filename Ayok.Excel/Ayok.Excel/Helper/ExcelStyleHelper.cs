using System.Drawing;
using System.Reflection;
using Ayok.Excel.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Ayok.Excel.Helper
{
    public static class ExcelStyleHelper
    {
        public static int CalculateFieldColumnCount(JsonExportField field, PropertyInfo property)
        {
            Type propertyType = property.PropertyType;
            if (
                propertyType.IsGenericType
                && propertyType.GetGenericTypeDefinition() == typeof(List<>)
            )
            {
                Type type = propertyType.GetGenericArguments()[0];
                List<JsonExportField>? subFields = field.SubFields;
                if (subFields != null && subFields.Count > 0)
                {
                    int num = 0;
                    {
                        foreach (JsonExportField subField in field.SubFields)
                        {
                            PropertyInfo property2 = type.GetProperty(subField.SourceFieldName);
                            num = (
                                (!(property2 != null))
                                    ? (num + 1)
                                    : (num + CalculateFieldColumnCount(subField, property2))
                            );
                        }
                        return num;
                    }
                }
                if (type.IsClass && !type.IsPrimitive && type != typeof(string))
                {
                    return type.GetProperties().Length;
                }
                return 1;
            }
            if (propertyType.IsClass && !propertyType.IsPrimitive && propertyType != typeof(string))
            {
                return propertyType.GetProperties().Length;
            }
            return 1;
        }

        public static void SetHeaderCellStyle(ExcelRange cell, JsonExportHeaderStyle style)
        {
            if (style != null)
            {
                cell.Style.Font.Color.SetColor(
                    Color.FromArgb(style.FontColorR, style.FontColorG, style.FontColorB)
                );
                cell.Style.Font.Size = style.FontSize;
                if (
                    style.BackgroundColorR.HasValue
                    && style.BackgroundColorG.HasValue
                    && style.BackgroundColorB.HasValue
                )
                {
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(
                        Color.FromArgb(
                            style.BackgroundColorR.Value,
                            style.BackgroundColorG.Value,
                            style.BackgroundColorB.Value
                        )
                    );
                }
                if (style.IsBold.HasValue && style.IsBold.Value)
                {
                    cell.Style.Font.Bold = true;
                }
                SetHorizontalAlignment(cell, style.HorizontalAlignment);
                SetVerticalAlignment(cell, style.VerticalAlignment);
            }
        }

        private static void SetHorizontalAlignment(ExcelRange cell, string? alignment)
        {
            if (!string.IsNullOrEmpty(alignment))
            {
                ExcelStyle style = cell.Style;
                style.HorizontalAlignment = alignment.ToLowerInvariant() switch
                {
                    "left" => ExcelHorizontalAlignment.Left,
                    "center" => ExcelHorizontalAlignment.Center,
                    "right" => ExcelHorizontalAlignment.Right,
                    _ => ExcelHorizontalAlignment.Center,
                };
            }
        }

        private static void SetVerticalAlignment(ExcelRange cell, string? alignment)
        {
            if (!string.IsNullOrEmpty(alignment))
            {
                ExcelStyle style = cell.Style;
                style.VerticalAlignment = alignment.ToLowerInvariant() switch
                {
                    "top" => ExcelVerticalAlignment.Top,
                    "center" => ExcelVerticalAlignment.Center,
                    "bottom" => ExcelVerticalAlignment.Bottom,
                    _ => ExcelVerticalAlignment.Center,
                };
            }
        }

        public static void SetColumnCellStyle(ExcelRange cell, JsonExportColumnStyle style)
        {
            if (style != null)
            {
                cell.Style.Font.Color.SetColor(
                    Color.FromArgb(style.FontColorR, style.FontColorG, style.FontColorB)
                );
                cell.Style.Font.Size = style.FontSize;
                if (
                    style.BackgroundColorR.HasValue
                    && style.BackgroundColorG.HasValue
                    && style.BackgroundColorB.HasValue
                )
                {
                    cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    cell.Style.Fill.BackgroundColor.SetColor(
                        Color.FromArgb(
                            style.BackgroundColorR.Value,
                            style.BackgroundColorG.Value,
                            style.BackgroundColorB.Value
                        )
                    );
                }
            }
        }

        public static void SetDefaultHeaderStyles(
            ExcelWorksheet worksheet,
            int startRow,
            int endRow,
            int endColumn
        )
        {
            for (int i = startRow; i <= endRow; i++)
            {
                for (int j = 1; j <= endColumn; j++)
                {
                    ExcelRange excelRange = worksheet.Cells[i, j];
                    if ((double)Math.Abs(excelRange.Style.Font.Size - 11f) < 0.1)
                    {
                        excelRange.Style.Font.Size = 14f;
                        excelRange.Style.Font.Bold = true;
                        excelRange.Style.Font.Color.SetColor(Color.Black);
                    }
                }
            }
        }

        public static void SetFieldSpecificStyles<T>(
            ExcelWorksheet worksheet,
            JsonExportConfig config,
            List<PropertyInfo> allProperties,
            int dataStartRow,
            int lastRow
        )
            where T : class
        {
            int num = 1;
            foreach (JsonExportField field in config.Fields)
            {
                PropertyInfo propertyInfo = allProperties.FirstOrDefault(
                    (PropertyInfo p) => p.Name == field.SourceFieldName
                );
                if (propertyInfo != null)
                {
                    Type propertyType = propertyInfo.PropertyType;
                    int num2 = 1;
                    if (
                        propertyType.IsGenericType
                        && propertyType.GetGenericTypeDefinition() == typeof(List<>)
                    )
                    {
                        num2 = CalculateFieldColumnCount(field, propertyInfo);
                    }
                    else if (
                        propertyType.IsClass
                        && !propertyType.IsPrimitive
                        && propertyType != typeof(string)
                    )
                    {
                        num2 = propertyType.GetProperties().Length;
                    }
                    if (field.ColumnStyle == null)
                    {
                        ExcelRange excelRange = worksheet.Cells[
                            dataStartRow,
                            num,
                            lastRow,
                            num + num2 - 1
                        ];
                        excelRange.Style.Font.Size = 12f;
                        excelRange.Style.Font.Color.SetColor(Color.Black);
                        excelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        excelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    }
                    num += num2;
                }
            }
        }

        public static void SetComplexColumnWidths<T>(
            ExcelWorksheet worksheet,
            JsonExportConfig config,
            JsonExportSheetSettings sheetSettings
        )
            where T : class
        {
            Type typeFromHandle = typeof(T);
            List<PropertyInfo> allProperties = (
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
            bool flag = HasAnyColumnWidthSettings(config);
            if (sheetSettings.AutoColumnWidth || !flag)
            {
                SetMinimumColumnWidthsForComplex<T>(worksheet, config, allProperties);
                if (worksheet.Dimension == null)
                {
                    return;
                }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
                for (int i = 1; i <= worksheet.Dimension.End.Column; i++)
                {
                    if (worksheet.Column(i).Width > 100.0)
                    {
                        worksheet.Column(i).Width = 100.0;
                    }
                    else if (worksheet.Column(i).Width < 8.0)
                    {
                        worksheet.Column(i).Width = 8.0;
                    }
                }
            }
            else
            {
                SetConfiguredColumnWidthsForComplex<T>(
                    worksheet,
                    config,
                    sheetSettings,
                    allProperties
                );
            }
        }

        public static void SetMinimumColumnWidthsForComplex<T>(
            ExcelWorksheet worksheet,
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
                    propertyType.IsGenericType
                    && propertyType.GetGenericTypeDefinition() == typeof(List<>)
                )
                {
                    List<JsonExportField>? subFields = field.SubFields;
                    if (subFields != null && subFields.Count > 0)
                    {
                        foreach (JsonExportField subField in field.SubFields)
                        {
                            _ = subField;
                            worksheet.Column(num).Width = 8.0;
                            num++;
                        }
                        continue;
                    }
                    Type type = propertyType.GetGenericArguments()[0];
                    int num2 = (
                        (!type.IsClass || type.IsPrimitive || !(type != typeof(string)))
                            ? 1
                            : type.GetProperties().Length
                    );
                    for (int num3 = 0; num3 < num2; num3++)
                    {
                        worksheet.Column(num).Width = 8.0;
                        num++;
                    }
                }
                else if (
                    propertyType.IsClass
                    && !propertyType.IsPrimitive
                    && propertyType != typeof(string)
                )
                {
                    PropertyInfo[] properties = propertyType.GetProperties();
                    for (int num4 = 0; num4 < properties.Length; num4++)
                    {
                        _ = properties[num4];
                        worksheet.Column(num).Width = 8.0;
                        num++;
                    }
                }
                else
                {
                    worksheet.Column(num).Width = 8.0;
                    num++;
                }
            }
        }

        public static bool HasAnyColumnWidthSettings(JsonExportConfig config)
        {
            if (config.Fields.Any((JsonExportField f) => f.ColumnWidth.HasValue))
            {
                return true;
            }
            foreach (JsonExportField field in config.Fields)
            {
                List<JsonExportField>? subFields = field.SubFields;
                if (
                    subFields != null
                    && subFields.Any((JsonExportField sf) => sf.ColumnWidth.HasValue)
                )
                {
                    return true;
                }
            }
            return false;
        }

        public static void SetConfiguredColumnWidthsForComplex<T>(
            ExcelWorksheet worksheet,
            JsonExportConfig config,
            JsonExportSheetSettings sheetSettings,
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
                    propertyType.IsGenericType
                    && propertyType.GetGenericTypeDefinition() == typeof(List<>)
                )
                {
                    List<JsonExportField>? subFields = field.SubFields;
                    if (subFields != null && subFields.Count > 0)
                    {
                        foreach (JsonExportField subField in field.SubFields)
                        {
                            int num2 =
                                subField.ColumnWidth
                                ?? field.ColumnWidth
                                ?? sheetSettings.ColumnWidth;
                            worksheet.Column(num).Width = num2;
                            num++;
                        }
                        continue;
                    }
                    Type type = propertyType.GetGenericArguments()[0];
                    int num3 = (
                        (!type.IsClass || type.IsPrimitive || !(type != typeof(string)))
                            ? 1
                            : type.GetProperties().Length
                    );
                    for (int num4 = 0; num4 < num3; num4++)
                    {
                        int num5 = field.ColumnWidth ?? sheetSettings.ColumnWidth;
                        worksheet.Column(num).Width = num5;
                        num++;
                    }
                }
                else if (
                    propertyType.IsClass
                    && !propertyType.IsPrimitive
                    && propertyType != typeof(string)
                )
                {
                    PropertyInfo[] properties = propertyType.GetProperties();
                    for (int num6 = 0; num6 < properties.Length; num6++)
                    {
                        _ = properties[num6];
                        int num7 = field.ColumnWidth ?? sheetSettings.ColumnWidth;
                        worksheet.Column(num).Width = num7;
                        num++;
                    }
                }
                else
                {
                    int num8 = field.ColumnWidth ?? sheetSettings.ColumnWidth;
                    worksheet.Column(num).Width = num8;
                    num++;
                }
            }
        }

        public static void SetComplexStyles<T>(
            ExcelWorksheet worksheet,
            JsonExportConfig config,
            int lastRow
        )
            where T : class
        {
            worksheet.Cells.Style.ShrinkToFit = true;
            worksheet.Cells.Style.WrapText = true;
            Type typeFromHandle = typeof(T);
            List<PropertyInfo> allProperties = (
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
            int num = 1;
            if (!string.IsNullOrEmpty(config.Description))
            {
                num++;
            }
            bool flag = config.Fields.Any(
                delegate(JsonExportField field)
                {
                    PropertyInfo propertyInfo = allProperties.FirstOrDefault(
                        (PropertyInfo p) => p.Name == field.SourceFieldName
                    );
                    return (object)propertyInfo != null
                        && propertyInfo.PropertyType.IsGenericType
                        && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>);
                }
            );
            num += ((!flag) ? 1 : 2);
            SetFieldSpecificStyles<T>(worksheet, config, allProperties, num, lastRow);
        }

        public static void SetHeaderStyles(
            ExcelWorksheet worksheet,
            int startRow,
            int endRow,
            int endColumn
        )
        {
            ExcelRange excelRange = worksheet.Cells[startRow, 1, endRow, endColumn];
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(Color.WhiteSmoke);
            excelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            excelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            excelRange.Style.Border.Top.Style = ExcelBorderStyle.Thin;
            excelRange.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
            excelRange.Style.Border.Left.Style = ExcelBorderStyle.Thin;
            excelRange.Style.Border.Right.Style = ExcelBorderStyle.Thin;
            excelRange.Style.Border.Top.Color.SetColor(Color.LightGray);
            excelRange.Style.Border.Bottom.Color.SetColor(Color.LightGray);
            excelRange.Style.Border.Left.Color.SetColor(Color.LightGray);
            excelRange.Style.Border.Right.Color.SetColor(Color.LightGray);
            SetDefaultHeaderStyles(worksheet, startRow, endRow, endColumn);
        }

        public static JsonExportSheetSettings EnsureSheetSettingsWithAutoColumnWidth(
            JsonExportConfig config
        )
        {
            if (config.SheetSettings == null)
            {
                return new JsonExportSheetSettings
                {
                    Name = ((!string.IsNullOrEmpty(config.FileName)) ? config.FileName : "Sheet1"),
                    ColumnWidth = 15,
                    AutoColumnWidth = true,
                };
            }
            bool num = config.Fields.Any((JsonExportField f) => f.ColumnWidth.HasValue);
            JsonExportSheetSettings jsonExportSheetSettings = new JsonExportSheetSettings
            {
                Name = config.SheetSettings.Name,
                ColumnWidth = config.SheetSettings.ColumnWidth,
                AutoColumnWidth = config.SheetSettings.AutoColumnWidth,
            };
            if (!num && jsonExportSheetSettings.ColumnWidth == 15)
            {
                jsonExportSheetSettings.AutoColumnWidth = true;
            }
            return jsonExportSheetSettings;
        }
    }
}
