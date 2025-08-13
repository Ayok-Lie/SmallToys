using Ayok.Excel;
using Ayok.Excel.Extensions;
using ConsoleApp1;
using Microsoft.Extensions.DependencyInjection;

IServiceCollection services = new ServiceCollection();
services.AddExcelService();
var sp = services.BuildServiceProvider();
var importService = sp.GetRequiredService<IExcelImportService>();
var exportService = sp.GetRequiredService<IExcelExportService>();
var excel = ExcelUtil.ImportData<TestDto>(@"D:\file\加工厂产能导入模版.xlsx");
var fileBytes = ExcelUtil.ExportData(excel);

// 生成动态文件名
var fileName = $"D:\\file\\排除商品范围_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
{
    await fs.WriteAsync(fileBytes, 0, fileBytes.Length);
}
