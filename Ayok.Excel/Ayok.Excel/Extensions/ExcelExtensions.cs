using Microsoft.Extensions.DependencyInjection;
using OfficeOpenXml;

namespace Ayok.Excel.Extensions
{
    public static class ExcelExtensions
    {
        public static IServiceCollection AddExcelService(this IServiceCollection services)
        {
            ExcelPackage.License.SetNonCommercialPersonal("Ayok");
            //ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            services.AddSingleton<IExcelImportService, ExcelImportService>();
            services.AddSingleton<IExcelExportService, ExcelExportService>();
            return services;
        }
    }

}
