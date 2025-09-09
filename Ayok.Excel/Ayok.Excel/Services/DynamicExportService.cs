using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Text.Json;
using Ayok.Excel.Helper;
using Ayok.Excel.Model;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace Ayok.Excel.Services
{
    public class DynamicExportService : IDynamicExportService, IDisposable
    {
        private readonly string _configFilePath;

        private static readonly ConcurrentDictionary<string, JsonExportConfig> _configCache;

        private static long _lastModifiedTicks;

        private static long _configVersion;

        private static readonly SemaphoreSlim _reloadSemaphore;

        private static readonly SemaphoreSlim _excelSemaphore;

        private static readonly ConcurrentDictionary<
            string,
            (DateTime timestamp, int count)
        > _requestMetrics;

        private static readonly Timer _metricsCleanupTimer;

        private const int MAX_JSON_SIZE = 10485760;

        private const int MAX_CONFIG_COUNT = 1000;

        private const int MAX_FIELDS_COUNT = 100;

        private const int MAX_REQUESTS_PER_MINUTE = 500;

        private static readonly JsonSerializerOptions _jsonOptions;

        private bool _disposed;

        private static DateTime LastModified
        {
            get { return new DateTime(Interlocked.Read(in _lastModifiedTicks)); }
            set { Interlocked.Exchange(ref _lastModifiedTicks, value.Ticks); }
        }

        static DynamicExportService()
        {
            _configCache = new ConcurrentDictionary<string, JsonExportConfig>();
            _lastModifiedTicks = DateTime.MinValue.Ticks;
            _reloadSemaphore = new SemaphoreSlim(1, 1);
            _excelSemaphore = new SemaphoreSlim(
                Environment.ProcessorCount * 2,
                Environment.ProcessorCount * 2
            );
            _requestMetrics = new ConcurrentDictionary<string, (DateTime, int)>();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
            };
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            _metricsCleanupTimer = new Timer(
                CleanupMetrics,
                null,
                TimeSpan.FromMinutes(5.0),
                TimeSpan.FromMinutes(5.0)
            );
        }

        public DynamicExportService(string? configFilePath = null)
        {
            _configFilePath = configFilePath ?? GetDefaultConfigPath();
        }

        private static string GetDefaultConfigPath()
        {
            string[] array = new string[3]
            {
                Path.Combine(AppContext.BaseDirectory, "ExportConfigs.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "ExportConfigs.json"),
                Path.Combine(Directory.GetCurrentDirectory(), "..", "ExportConfigs.json"),
            };
            return array.FirstOrDefault(File.Exists) ?? array[0];
        }

        public async Task<MemoryStream> ExportAsync<T>(List<T> data, string exportType)
            where T : class
        {
            _ = $"{Environment.CurrentManagedThreadId}_{DateTime.Now.Ticks}";
            RecordRequest(exportType);
            using Activity activity = new Activity("Excel.Export." + exportType);
            activity.Start();
            Stopwatch stopwatch = Stopwatch.StartNew();
            try
            {
                ArgumentNullException.ThrowIfNull(data, "data");
                ArgumentException.ThrowIfNullOrWhiteSpace(exportType, "exportType");
                CheckConcurrencyLimits(exportType);
                if (data.Count > 10000)
                {
                    Console.WriteLine(
                        $"警告: 大数据量检测到 {data.Count} 条记录，建议使用批量处理"
                    );
                }
                JsonExportConfig config =
                    (await GetExportConfigFromJsonAsync(exportType))
                    ?? throw new ArgumentException("未找到导出类型 '" + exportType + "' 的配置");
                MemoryStream memoryStream = await ExportDataWithOneToManySupport(data, config);
                activity?.SetTag("status", "success");
                activity?.SetTag("records_count", data.Count.ToString());
                activity?.SetTag("file_size_kb", (memoryStream.Length / 1024).ToString());
                activity?.SetTag("duration_ms", stopwatch.ElapsedMilliseconds.ToString());
                return memoryStream;
            }
            catch (Exception ex)
            {
                activity?.SetTag("status", "error");
                activity?.SetTag("error", ex.Message);
                throw;
            }
            finally
            {
                stopwatch.Stop();
                activity?.Stop();
            }
        }

        public async Task<Dictionary<string, MemoryStream>> ExportBatchAsync<T>(
            Dictionary<string, List<T>> dataGroups,
            string exportType,
            CancellationToken cancellationToken = default(CancellationToken)
        )
            where T : class
        {
            JsonExportConfig config =
                (await GetExportConfigFromJsonAsync(exportType))
                ?? throw new ArgumentException("未找到导出类型 '" + exportType + "' 的配置");
            SemaphoreSlim semaphore = new SemaphoreSlim(Environment.ProcessorCount * 2);
            ConcurrentDictionary<string, MemoryStream> results =
                new ConcurrentDictionary<string, MemoryStream>();
            await Task.WhenAll(
                dataGroups.Select<KeyValuePair<string, List<T>>, Task>(
                    async delegate(KeyValuePair<string, List<T>> kvp)
                    {
                        await semaphore.WaitAsync(cancellationToken);
                        try
                        {
                            MemoryStream value = await ExportDataWithOneToManySupport(
                                kvp.Value,
                                config
                            );
                            results[kvp.Key] = value;
                        }
                        finally
                        {
                            semaphore.Release();
                        }
                    }
                )
            );
            semaphore.Dispose();
            return new Dictionary<string, MemoryStream>(results);
        }

        public async Task<List<MemoryStream>> ExportLargeDatasetAsync<T>(
            List<T> data,
            string exportType,
            int pageSize = 1000,
            CancellationToken cancellationToken = default(CancellationToken)
        )
            where T : class
        {
            if (data.Count <= pageSize)
            {
                MemoryStream item = await ExportAsync(data, exportType);
                return new List<MemoryStream> { item };
            }
            List<MemoryStream> results = new List<MemoryStream>();
            for (int i = 0; i < data.Count; i += pageSize)
            {
                cancellationToken.ThrowIfCancellationRequested();
                List<T> data2 = data.Skip(i).Take(pageSize).ToList();
                results.Add(await ExportAsync(data2, exportType));
            }
            return results;
        }

        private async Task<JsonExportConfig?> GetExportConfigFromJsonAsync(string exportType)
        {
            long num = Interlocked.Read(in _configVersion);
            if (_configCache.TryGetValue(exportType, out JsonExportConfig value))
            {
                FileInfo fileInfo = new FileInfo(_configFilePath);
                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException("配置文件 " + _configFilePath + " 不存在");
                }
                if (
                    fileInfo.LastWriteTime <= LastModified
                    && num == Interlocked.Read(in _configVersion)
                )
                {
                    return value;
                }
            }
            return await ReloadConfigIfNeeded(exportType);
        }

        private async Task<JsonExportConfig?> ReloadConfigIfNeeded(string exportType)
        {
            await _reloadSemaphore.WaitAsync();
            try
            {
                if (_configCache.TryGetValue(exportType, out JsonExportConfig value))
                {
                    FileInfo fileInfo = new FileInfo(_configFilePath);
                    if (fileInfo.Exists && fileInfo.LastWriteTime <= LastModified)
                    {
                        return value;
                    }
                }
                FileInfo fileInfo2 = new FileInfo(_configFilePath);
                if (!fileInfo2.Exists)
                {
                    throw new FileNotFoundException("配置文件 " + _configFilePath + " 不存在");
                }
                return await LoadAndCacheConfigsAsync(exportType, fileInfo2.LastWriteTime);
            }
            finally
            {
                _reloadSemaphore.Release();
            }
        }

        private async Task<JsonExportConfig?> LoadAndCacheConfigsAsync(
            string exportType,
            DateTime fileLastWriteTime
        )
        {
            string obj = await File.ReadAllTextAsync(_configFilePath);
            ValidateFileContent(obj);
            List<JsonExportConfig>? list = JsonSerializer.Deserialize<List<JsonExportConfig>>(
                obj,
                _jsonOptions
            );
            ValidateConfigs(list);
            ConcurrentDictionary<string, JsonExportConfig> concurrentDictionary =
                new ConcurrentDictionary<string, JsonExportConfig>();
            foreach (JsonExportConfig item in list)
            {
                ValidateConfig(item);
                concurrentDictionary[item.ExportType] = item;
            }
            ReplaceCache(concurrentDictionary, fileLastWriteTime);
            return concurrentDictionary.GetValueOrDefault(exportType);
        }

        private static void ReplaceCache(
            ConcurrentDictionary<string, JsonExportConfig> newCache,
            DateTime fileTime
        )
        {
            List<string> list = _configCache.Keys.ToList();
            foreach (KeyValuePair<string, JsonExportConfig> kvp in newCache)
            {
                _configCache.AddOrUpdate(
                    kvp.Key,
                    kvp.Value,
                    (string key, JsonExportConfig oldValue) => kvp.Value
                );
            }
            HashSet<string> hashSet = newCache.Keys.ToHashSet();
            foreach (string item in list)
            {
                if (!hashSet.Contains(item))
                {
                    _configCache.TryRemove(item, out JsonExportConfig _);
                }
            }
            LastModified = fileTime;
            Interlocked.Increment(ref _configVersion);
        }

        private static void ValidateFileContent(string fileContent)
        {
            if (fileContent.Length > 10485760)
            {
                throw new InvalidOperationException($"配置文件过大，超过限制 {10}MB");
            }
        }

        private static void ValidateConfigs(List<JsonExportConfig>? configs)
        {
            if (configs == null || configs.Count == 0)
            {
                throw new InvalidOperationException("配置文件为空或格式错误");
            }
            if (configs.Count > 1000)
            {
                throw new InvalidOperationException($"配置数量过多，超过限制 {1000}");
            }
        }

        private static void ValidateConfig(JsonExportConfig config)
        {
            if (config.Fields.Count > 100)
            {
                throw new InvalidOperationException(
                    $"配置 {config.ExportType} 的字段数量过多，超过限制 {100}"
                );
            }
        }

        private static void RecordRequest(string exportType)
        {
            DateTime now = DateTime.UtcNow;
            _requestMetrics.AddOrUpdate(
                exportType,
                (now, 1),
                (string key, (DateTime timestamp, int count) value) =>
                    (timestamp: now, count: value.count + 1)
            );
        }

        private static void CheckConcurrencyLimits(string exportType)
        {
            if (
                _requestMetrics.TryGetValue(exportType, out (DateTime, int) _)
                && _requestMetrics
                    .Values.Where(
                        ((DateTime timestamp, int count) m) =>
                            DateTime.UtcNow - m.timestamp < TimeSpan.FromMinutes(1.0)
                    )
                    .Sum(((DateTime timestamp, int count) m) => m.count) > 500
            )
            {
                throw new InvalidOperationException(
                    "导出类型 " + exportType + " 请求频率过高，请稍后重试"
                );
            }
        }

        private static void CleanupMetrics(object? state)
        {
            DateTime cutoff = DateTime.UtcNow.AddMinutes(-10.0);
            foreach (
                string item in (
                    from kvp in _requestMetrics
                    where kvp.Value.timestamp < cutoff
                    select kvp.Key
                ).ToList()
            )
            {
                _requestMetrics.TryRemove(item, out (DateTime, int) _);
            }
        }

        private static async Task<MemoryStream> ExportDataWithOneToManySupport<T>(
            List<T> data,
            JsonExportConfig config
        )
            where T : class
        {
            await _excelSemaphore.WaitAsync();
            try
            {
                using ExcelPackage package = new ExcelPackage();
                JsonExportSheetSettings sheetSettings =
                    ExcelStyleHelper.EnsureSheetSettingsWithAutoColumnWidth(config);
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(sheetSettings.Name);
                bool flag = HasCollectionProperties<T>(config);
                int num = 1;
                if (!string.IsNullOrEmpty(config.Description))
                {
                    WriteDescriptionRow(
                        worksheet,
                        config.Description,
                        DataExportHelper.GetTotalColumnCount<T>(config)
                    );
                    num++;
                }
                WriteComplexHeaders<T>(worksheet, config, num, flag);
                num += ((!flag) ? 1 : 2);
                num = await WriteComplexData(worksheet, data, config, num);
                ExcelStyleHelper.SetComplexColumnWidths<T>(worksheet, config, sheetSettings);
                ExcelStyleHelper.SetComplexStyles<T>(worksheet, config, num);
                MemoryStream stream = new MemoryStream();
                await package.SaveAsAsync(stream);
                stream.Position = 0L;
                return stream;
            }
            finally
            {
                _excelSemaphore.Release();
            }
        }

        private static bool HasCollectionProperties<T>(JsonExportConfig config)
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
            return (
                from property in config
                    .Fields.Select(
                        (JsonExportField field) =>
                            allProperties.FirstOrDefault(
                                (PropertyInfo p) => p.Name == field.SourceFieldName
                            )
                    )
                    .OfType<PropertyInfo>()
                select property.PropertyType
            ).Any(
                (Type propertyType) =>
                    propertyType.IsGenericType
                    && propertyType.GetGenericTypeDefinition() == typeof(List<>)
            );
        }

        private static int WriteNestedCollectionHeaders(
            ExcelWorksheet worksheet,
            JsonExportField field,
            PropertyInfo property,
            int startRow,
            int endRow,
            int currentColumn
        )
        {
            int num = currentColumn;
            Type type = property.PropertyType.GetGenericArguments()[0];
            List<JsonExportField>? subFields = field.SubFields;
            if (subFields != null && subFields.Count > 0)
            {
                foreach (JsonExportField subField in field.SubFields)
                {
                    PropertyInfo property2 = type.GetProperty(subField.SourceFieldName);
                    if (property2 != null)
                    {
                        if (
                            property2.PropertyType.IsGenericType
                            && property2.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                        )
                        {
                            List<JsonExportField>? subFields2 = subField.SubFields;
                            if (subFields2 != null && subFields2.Count > 0)
                            {
                                currentColumn = WriteNestedCollectionHeaders(
                                    worksheet,
                                    subField,
                                    property2,
                                    startRow,
                                    endRow,
                                    currentColumn
                                );
                                continue;
                            }
                        }
                        worksheet.Cells[endRow, currentColumn].Value = subField.ColumnName;
                        if (subField.HeaderStyle != null)
                        {
                            ExcelStyleHelper.SetHeaderCellStyle(
                                worksheet.Cells[endRow, currentColumn],
                                subField.HeaderStyle
                            );
                        }
                        currentColumn++;
                    }
                    else
                    {
                        worksheet.Cells[endRow, currentColumn].Value = subField.ColumnName;
                        currentColumn++;
                    }
                }
            }
            if (currentColumn - num > 1)
            {
                worksheet.Cells[startRow, num].Value = field.ColumnName;
                worksheet.Cells[startRow, num, startRow, currentColumn - 1].Merge = true;
                if (field.HeaderStyle != null)
                {
                    ExcelStyleHelper.SetHeaderCellStyle(
                        worksheet.Cells[startRow, num],
                        field.HeaderStyle
                    );
                }
            }
            return currentColumn;
        }

        private static void WriteDescriptionRow(
            ExcelWorksheet worksheet,
            string description,
            int totalColumns
        )
        {
            worksheet.Cells[1, 1].Value = "文档说明";
            worksheet.Cells[1, 2, 1, totalColumns].Merge = true;
            worksheet.Cells[1, 2].Value = description;
            worksheet.Row(1).Height = 50.0;
            ExcelRange excelRange = worksheet.Cells[1, 1];
            excelRange.Style.Font.Size = 14f;
            excelRange.Style.Font.Bold = true;
            excelRange.Style.Font.Color.SetColor(Color.Black);
            excelRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
            excelRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
            ExcelRange excelRange2 = worksheet.Cells[1, 2, 1, totalColumns];
            excelRange2.Style.Font.Size = 12f;
            excelRange2.Style.Font.Color.SetColor(Color.Black);
            excelRange2.Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
            excelRange2.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
        }

        private static void WriteComplexHeaders<T>(
            ExcelWorksheet worksheet,
            JsonExportConfig config,
            int startRow,
            bool hasCollectionProperty
        )
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
            int num = 1;
            int endRow = (hasCollectionProperty ? (startRow + 1) : startRow);
            foreach (JsonExportField field in config.Fields)
            {
                PropertyInfo propertyInfo = source.FirstOrDefault(
                    (PropertyInfo p) => p.Name == field.SourceFieldName
                );
                if (propertyInfo != null)
                {
                    Type propertyType = propertyInfo.PropertyType;
                    num = (
                        (
                            propertyType.IsGenericType
                            && propertyType.GetGenericTypeDefinition() == typeof(List<>)
                        )
                            ? WriteNestedCollectionHeaders(
                                worksheet,
                                field,
                                propertyInfo,
                                startRow,
                                endRow,
                                num
                            )
                            : (
                                (
                                    !propertyType.IsClass
                                    || propertyType.IsPrimitive
                                    || !(propertyType != typeof(string))
                                )
                                    ? DataExportHelper.WriteBasicHeader(
                                        worksheet,
                                        field,
                                        startRow,
                                        endRow,
                                        num
                                    )
                                    : WriteObjectHeaders(
                                        worksheet,
                                        field,
                                        propertyInfo,
                                        startRow,
                                        endRow,
                                        num
                                    )
                            )
                    );
                }
            }
            ExcelStyleHelper.SetHeaderStyles(worksheet, startRow, endRow, num - 1);
        }

        private static int WriteObjectHeaders(
            ExcelWorksheet worksheet,
            JsonExportField field,
            PropertyInfo property,
            int startRow,
            int endRow,
            int currentColumn
        )
        {
            PropertyInfo[] properties = property.PropertyType.GetProperties();
            for (int i = 0; i < properties.Length; i++)
            {
                string propertyDisplayName = PropertiesHelper.GetPropertyDisplayName(properties[i]);
                worksheet.Cells[startRow, currentColumn].Value = propertyDisplayName;
                if (startRow != endRow)
                {
                    worksheet.Cells[startRow, currentColumn, endRow, currentColumn].Merge = true;
                }
                currentColumn++;
            }
            return currentColumn;
        }

        private static async Task<int> WriteComplexData<T>(
            ExcelWorksheet worksheet,
            List<T> data,
            JsonExportConfig config,
            int startRow
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
            int currentRow = startRow;
            foreach (T item in data)
            {
                int maxCollectionRowCount =
                    DataExportHelper.CalculateMaxCollectionRowCountWithNesting(
                        item,
                        config,
                        allProperties
                    );
                int currentColumn = 1;
                foreach (JsonExportField field in config.Fields)
                {
                    PropertyInfo propertyInfo = allProperties.FirstOrDefault(
                        (PropertyInfo p) => p.Name == field.SourceFieldName
                    );
                    if (!(propertyInfo == null))
                    {
                        Type propertyType = propertyInfo.PropertyType;
                        currentColumn = (
                            (
                                propertyType.IsGenericType
                                && propertyType.GetGenericTypeDefinition() == typeof(List<>)
                            )
                                ? (
                                    await WriteCollectionData(
                                        worksheet,
                                        item,
                                        field,
                                        propertyInfo,
                                        currentRow,
                                        currentColumn,
                                        maxCollectionRowCount
                                    )
                                )
                                : (
                                    (
                                        !propertyType.IsClass
                                        || propertyType.IsPrimitive
                                        || !(propertyType != typeof(string))
                                    )
                                        ? (
                                            await WriteBasicData(
                                                worksheet,
                                                item,
                                                field,
                                                propertyInfo,
                                                currentRow,
                                                currentColumn,
                                                maxCollectionRowCount
                                            )
                                        )
                                        : WriteObjectData(
                                            worksheet,
                                            item,
                                            field,
                                            propertyInfo,
                                            currentRow,
                                            currentColumn,
                                            maxCollectionRowCount
                                        )
                                )
                        );
                    }
                }
                currentRow += maxCollectionRowCount;
            }
            return currentRow;
        }

        private static async Task<int> WriteCollectionData<T>(
            ExcelWorksheet worksheet,
            T item,
            JsonExportField field,
            PropertyInfo property,
            int currentRow,
            int currentColumn,
            int maxRowCount
        )
            where T : class
        {
            IList list = (IList)property.GetValue(item);
            Type elementType = property.PropertyType.GetGenericArguments()[0];
            int startColumn = currentColumn;
            if (list == null || list.Count == 0)
            {
                return HandleEmptyCollection(field, elementType, currentColumn);
            }
            int rowOffset = 0;
            foreach (object item2 in list)
            {
                int currentItemRowCount = CalculateItemRowCount(item2, field, elementType);
                currentColumn = await WriteCollectionSubItem(
                    worksheet,
                    item2,
                    field,
                    elementType,
                    currentRow + rowOffset,
                    startColumn,
                    currentItemRowCount
                );
                rowOffset += currentItemRowCount;
            }
            return currentColumn;
        }

        private static int HandleEmptyCollection(
            JsonExportField field,
            Type elementType,
            int currentColumn
        )
        {
            List<JsonExportField>? subFields = field.SubFields;
            if (subFields != null && subFields.Count > 0)
            {
                return currentColumn
                    + DataExportHelper.CalculateSubFieldsColumnCount(field.SubFields, elementType);
            }
            if (elementType.IsClass && !elementType.IsPrimitive && elementType != typeof(string))
            {
                return currentColumn + elementType.GetProperties().Length;
            }
            return currentColumn + 1;
        }

        private static int CalculateItemRowCount(
            object subItem,
            JsonExportField field,
            Type elementType
        )
        {
            List<JsonExportField>? subFields = field.SubFields;
            if (subFields != null && subFields.Count > 0)
            {
                return DataExportHelper.CalculateItemRowCount(
                    subItem,
                    field.SubFields,
                    elementType
                );
            }
            List<PropertyInfo> list = (
                from p in subItem.GetType().GetProperties()
                where
                    p.PropertyType.IsGenericType
                    && p.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                select p
            ).ToList();
            if (!list.Any())
            {
                return 1;
            }
            int num = 0;
            foreach (PropertyInfo item in list)
            {
                IList list2 = (IList)item.GetValue(subItem);
                if (list2 != null)
                {
                    int num2 = DataExportHelper.CalculateNestedCollectionRows(list2);
                    if (num2 > num)
                    {
                        num = num2;
                    }
                }
            }
            return Math.Max(1, num);
        }

        private static async Task<int> WriteCollectionSubItem(
            ExcelWorksheet worksheet,
            object subItem,
            JsonExportField field,
            Type elementType,
            int currentRow,
            int startColumn,
            int currentItemRowCount
        )
        {
            int num = startColumn;
            List<JsonExportField>? subFields = field.SubFields;
            if (subFields != null && subFields.Count > 0)
            {
                return await WriteConfiguredSubFields(
                    worksheet,
                    subItem,
                    field.SubFields,
                    elementType,
                    currentRow,
                    num,
                    currentItemRowCount
                );
            }
            if (elementType.IsClass && !elementType.IsPrimitive && elementType != typeof(string))
            {
                PropertyInfo[] properties = elementType.GetProperties();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object value = propertyInfo.GetValue(subItem);
                    if (
                        propertyInfo.PropertyType.IsGenericType
                        && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                    )
                    {
                        WriteNestedCollectionData(
                            worksheet,
                            value as IList,
                            propertyInfo,
                            currentRow,
                            num,
                            currentItemRowCount
                        );
                    }
                    else
                    {
                        ExcelRange excelRange = worksheet.Cells[
                            currentRow,
                            num,
                            currentRow + currentItemRowCount - 1,
                            num
                        ];
                        if (currentItemRowCount > 1)
                        {
                            excelRange.Merge = true;
                        }
                        object value2 = DataExportHelper.DataConvert(
                            value,
                            propertyInfo.PropertyType
                        );
                        excelRange.Value = value2;
                    }
                    num++;
                }
                return num;
            }
            ExcelRange excelRange2 = worksheet.Cells[
                currentRow,
                startColumn,
                currentRow + currentItemRowCount - 1,
                startColumn
            ];
            if (currentItemRowCount > 1)
            {
                excelRange2.Merge = true;
            }
            object value3 = DataExportHelper.DataConvert(subItem, elementType);
            excelRange2.Value = value3;
            return startColumn + 1;
        }

        private static async Task<int> WriteConfiguredSubFields(
            ExcelWorksheet worksheet,
            object subItem,
            List<JsonExportField> subFields,
            Type elementType,
            int currentRow,
            int startColumn,
            int currentItemRowCount
        )
        {
            int currentColumn = startColumn;
            foreach (JsonExportField subField in subFields)
            {
                PropertyInfo property = elementType.GetProperty(subField.SourceFieldName);
                if (property != null)
                {
                    object value = property.GetValue(subItem);
                    if (
                        property.PropertyType.IsGenericType
                        && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                    )
                    {
                        List<JsonExportField>? subFields2 = subField.SubFields;
                        if (subFields2 != null && subFields2.Count > 0)
                        {
                            currentColumn = await WriteNestedCollectionWithConfig(
                                worksheet,
                                value as IList,
                                subField,
                                currentRow,
                                currentColumn,
                                currentItemRowCount
                            );
                            continue;
                        }
                        WriteNestedCollectionData(
                            worksheet,
                            value as IList,
                            property,
                            currentRow,
                            currentColumn,
                            currentItemRowCount
                        );
                        currentColumn++;
                        continue;
                    }
                    ExcelRange excelRange = worksheet.Cells[
                        currentRow,
                        currentColumn,
                        currentRow + currentItemRowCount - 1,
                        currentColumn
                    ];
                    if (currentItemRowCount > 1)
                    {
                        excelRange.Merge = true;
                    }
                    await DataExportHelper.ProcessCellValueForSubField(
                        excelRange,
                        value,
                        subField,
                        property,
                        worksheet,
                        currentRow,
                        currentColumn
                    );
                    currentColumn++;
                }
                else
                {
                    currentColumn++;
                }
            }
            return currentColumn;
        }

        private static async Task<int> WriteNestedCollectionWithConfig(
            ExcelWorksheet worksheet,
            IList? nestedCollection,
            JsonExportField parentField,
            int currentRow,
            int startColumn,
            int availableRows
        )
        {
            if (nestedCollection == null || nestedCollection.Count == 0)
            {
                return startColumn + (parentField.SubFields?.Count ?? 1);
            }
            int currentColumn = startColumn;
            int rowOffset = 0;
            foreach (object nestedItem in nestedCollection)
            {
                if (rowOffset >= availableRows)
                {
                    break;
                }
                currentColumn = startColumn;
                List<JsonExportField>? subFields = parentField.SubFields;
                if (subFields != null && subFields.Count > 0)
                {
                    Type nestedItemType = nestedItem.GetType();
                    foreach (JsonExportField subField in parentField.SubFields)
                    {
                        PropertyInfo property = nestedItemType.GetProperty(
                            subField.SourceFieldName
                        );
                        if (property != null)
                        {
                            object value = property.GetValue(nestedItem);
                            if (
                                property.PropertyType.IsGenericType
                                && property.PropertyType.GetGenericTypeDefinition()
                                    == typeof(List<>)
                            )
                            {
                                List<JsonExportField>? subFields2 = subField.SubFields;
                                if (subFields2 != null && subFields2.Count > 0)
                                {
                                    currentColumn = await WriteNestedCollectionWithConfig(
                                        worksheet,
                                        value as IList,
                                        subField,
                                        currentRow + rowOffset,
                                        currentColumn,
                                        1
                                    );
                                    continue;
                                }
                            }
                            string text = subField.ExcelColumnType?.ToLowerInvariant() ?? "常规";
                            ExcelRange excelRange = worksheet.Cells[
                                currentRow + rowOffset,
                                currentColumn
                            ];
                            bool flag;
                            switch (text)
                            {
                                case "网络图片":
                                case "本地图片":
                                case "超链接":
                                    flag = true;
                                    break;
                                default:
                                    flag = false;
                                    break;
                            }
                            if (flag)
                            {
                                await DataExportHelper.ProcessCellValueForSubField(
                                    excelRange,
                                    value,
                                    subField,
                                    property,
                                    worksheet,
                                    currentRow + rowOffset,
                                    currentColumn
                                );
                            }
                            else
                            {
                                string value2 = DataExportHelper.FormatValueFromSubField(
                                    value,
                                    subField
                                );
                                excelRange.Value = value2;
                            }
                            currentColumn++;
                        }
                        else
                        {
                            currentColumn++;
                        }
                    }
                }
                rowOffset++;
            }
            return currentColumn;
        }

        private static void WriteNestedCollectionData(
            ExcelWorksheet worksheet,
            IList? nestedCollection,
            PropertyInfo parentProperty,
            int startRow,
            int startColumn,
            int availableRows
        )
        {
            if (nestedCollection == null || nestedCollection.Count == 0)
            {
                return;
            }
            int num = startRow;
            Type type = parentProperty.PropertyType.GetGenericArguments()[0];
            foreach (object item in nestedCollection)
            {
                if (num - startRow >= availableRows)
                {
                    break;
                }
                object value = DataExportHelper.DataConvert(item, type);
                worksheet.Cells[num, startColumn].Value = value;
                num++;
            }
        }

        private static async Task WriteNestedCollectionData(
            ExcelWorksheet worksheet,
            IList? nestedCollection,
            JsonExportField parentField,
            int startRow,
            int startColumn,
            int availableRows
        )
        {
            if (nestedCollection == null || nestedCollection.Count == 0)
            {
                return;
            }
            int currentRow = startRow;
            int currentColumn = startColumn;
            foreach (object nestedItem in nestedCollection)
            {
                if (currentRow - startRow >= availableRows)
                {
                    break;
                }
                List<JsonExportField>? subFields = parentField.SubFields;
                if (subFields != null && subFields.Count > 0)
                {
                    Type nestedItemType = nestedItem.GetType();
                    foreach (JsonExportField subField in parentField.SubFields)
                    {
                        PropertyInfo property = nestedItemType.GetProperty(
                            subField.SourceFieldName
                        );
                        if (!(property != null))
                        {
                            continue;
                        }
                        object value = property.GetValue(nestedItem);
                        if (
                            property.PropertyType.IsGenericType
                            && property.PropertyType.GetGenericTypeDefinition() == typeof(List<>)
                        )
                        {
                            List<JsonExportField>? subFields2 = subField.SubFields;
                            if (subFields2 != null && subFields2.Count > 0)
                            {
                                await WriteNestedCollectionData(
                                    worksheet,
                                    value as IList,
                                    subField,
                                    currentRow,
                                    currentColumn,
                                    1
                                );
                                goto IL_01f3;
                            }
                        }
                        string value2 = DataExportHelper.FormatValueFromSubField(value, subField);
                        worksheet.Cells[currentRow, currentColumn].Value = value2;
                        goto IL_01f3;
                        IL_01f3:
                        currentColumn++;
                    }
                }
                else
                {
                    object value3 = DataExportHelper.DataConvert(nestedItem, nestedItem.GetType());
                    worksheet.Cells[currentRow, startColumn].Value = value3;
                }
                currentRow++;
                currentColumn = startColumn;
            }
        }

        private static int WriteObjectData<T>(
            ExcelWorksheet worksheet,
            T item,
            JsonExportField field,
            PropertyInfo property,
            int currentRow,
            int currentColumn,
            int maxRowCount
        )
            where T : class
        {
            object value = property.GetValue(item);
            if (value != null)
            {
                PropertyInfo[] properties = property.PropertyType.GetProperties();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    object value2 = propertyInfo.GetValue(value);
                    ExcelRange excelRange = worksheet.Cells[
                        currentRow,
                        currentColumn,
                        currentRow + maxRowCount - 1,
                        currentColumn
                    ];
                    excelRange.Merge = true;
                    if (value2 != null)
                    {
                        object value3 = DataExportHelper.DataConvert(
                            value2,
                            propertyInfo.PropertyType
                        );
                        excelRange.Value = value3;
                    }
                    currentColumn++;
                }
            }
            else
            {
                PropertyInfo[] properties2 = property.PropertyType.GetProperties();
                currentColumn += properties2.Length;
            }
            return currentColumn;
        }

        private static async Task<int> WriteBasicData<T>(
            ExcelWorksheet worksheet,
            T item,
            JsonExportField field,
            PropertyInfo property,
            int currentRow,
            int currentColumn,
            int maxRowCount
        )
            where T : class
        {
            object value = property.GetValue(item);
            ExcelRange mergeRange = worksheet.Cells[
                currentRow,
                currentColumn,
                currentRow + maxRowCount - 1,
                currentColumn
            ];
            if (maxRowCount > 1)
            {
                mergeRange.Merge = true;
            }
            if (value != null)
            {
                bool flag;
                switch (field.ExcelColumnType?.ToLowerInvariant() ?? "常规")
                {
                    case "网络图片":
                    case "本地图片":
                    case "超链接":
                        flag = true;
                        break;
                    default:
                        flag = false;
                        break;
                }
                if (flag)
                {
                    await DataExportHelper.ProcessCellValueForSubField(
                        mergeRange,
                        value,
                        field,
                        property,
                        worksheet,
                        currentRow,
                        currentColumn
                    );
                }
                else
                {
                    string value2 = DataExportHelper.FormatValueFromSubField(value, field);
                    mergeRange.Value = value2;
                }
            }
            if (field.ColumnStyle != null)
            {
                ExcelStyleHelper.SetColumnCellStyle(mergeRange, field.ColumnStyle);
            }
            return currentColumn + 1;
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                _disposed = true;
            }
        }
    }
}
