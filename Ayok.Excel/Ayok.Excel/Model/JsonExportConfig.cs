namespace Ayok.Excel.Model
{
    public class JsonExportConfig
    {
        public string ExportType { get; set; } = "";

        public string ExportName { get; set; } = "";

        public string FileName { get; set; } = "";

        public List<JsonExportField> Fields { get; set; } = new List<JsonExportField>();

        public JsonExportSheetSettings? SheetSettings { get; set; }

        public bool IsDescendingClass { get; set; }

        public string? Description { get; set; }
    }
}
