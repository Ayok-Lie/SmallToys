namespace Ayok.Excel.Services
{
    public interface IExcelExportService
    {
        MemoryStream ExportData<T>(IEnumerable<T> data, string sheetName = "Sheet1");
    }
}
