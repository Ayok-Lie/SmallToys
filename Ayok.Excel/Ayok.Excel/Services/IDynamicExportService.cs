namespace Ayok.Excel.Services
{
    public interface IDynamicExportService : IDisposable
    {
        Task<MemoryStream> ExportAsync<T>(List<T> data, string exportType)
            where T : class;

        Task<Dictionary<string, MemoryStream>> ExportBatchAsync<T>(
            Dictionary<string, List<T>> dataGroups,
            string exportType,
            CancellationToken cancellationToken = default(CancellationToken)
        )
            where T : class;

        Task<List<MemoryStream>> ExportLargeDatasetAsync<T>(
            List<T> data,
            string exportType,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
        )
            where T : class;
    }
}
