using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Reflection;
using Ayok.Excel.Attributes;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Ayok.Excel.Services
{
    public class ExcelExportService : IExcelExportService
    {
        public MemoryStream ExportData<T>(IEnumerable<T> data, string sheetName = "Sheet1")
        {
            ExcelPackage.License.SetNonCommercialPersonal("Ayok");
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            MemoryStream memoryStream = new MemoryStream();
            using ExcelPackage excelPackage = new ExcelPackage(memoryStream);
            ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets.Add(sheetName);
            List<PropertyInfo> list = (
                from p in typeof(T).GetProperties()
                where !Attribute.IsDefined(p, typeof(ExcelIgnoreAttribute))
                select p
            ).ToList();
            for (int num = 0; num < list.Count; num++)
            {
                ColumnAttribute customAttribute = list[num].GetCustomAttribute<ColumnAttribute>();
                excelWorksheet.Cells[1, num + 1].Value = customAttribute?.Name ?? list[num].Name;
            }
            excelWorksheet.Cells["A2"].LoadFromCollection(data, PrintHeaders: false);
            ExcelRange excelRange = excelWorksheet.Cells[1, 1, 1, list.Count];
            excelRange.Style.Font.Bold = true;
            excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
            excelRange.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
            excelWorksheet.Cells.AutoFitColumns();
            excelPackage.Save();
            memoryStream.Position = 0L;
            return memoryStream;
        }

        public async Task<MemoryStream> ExportDataAsync<T>(IEnumerable<T> data)
        {
            return await Task.Run(() => ExportData(data));
        }
    }
}
