namespace Ayok.Excel.Services
{
    public interface IExcelImportService
    {
        List<T> ImportData<T>(Stream stream, bool hasHeader = true)
            where T : new();
    }
}
