namespace Ayok.Excel.Model
{
    public class JsonExportField
    {
        public string SourceFieldName { get; set; } = "";

        public string ColumnName { get; set; } = "";

        public string? FormatType { get; set; }

        public string? FormatString { get; set; }

        public string? ExcelColumnType { get; set; } = "常规";

        public string? DateTimeFormat { get; set; }

        public int? ColumnWidth { get; set; }

        public JsonExportHeaderStyle? HeaderStyle { get; set; }

        public JsonExportColumnStyle? ColumnStyle { get; set; }

        public List<JsonExportField>? SubFields { get; set; }
    }
}
