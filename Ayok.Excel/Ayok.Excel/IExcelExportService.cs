namespace Ayok.Excel
{
    public interface IExcelExportService
    {
        MemoryStream ExportData<T>(IEnumerable<T> data, string sheetName = "Sheet1");
    }

}
