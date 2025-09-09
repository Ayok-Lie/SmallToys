namespace Ayok.Excel.Model
{
    public class JsonExportHeaderStyle
    {
        public int FontColorR { get; set; }

        public int FontColorG { get; set; }

        public int FontColorB { get; set; }

        public int FontSize { get; set; } = 11;

        public int? BackgroundColorR { get; set; }

        public int? BackgroundColorG { get; set; }

        public int? BackgroundColorB { get; set; }

        public bool? IsBold { get; set; }

        public string? HorizontalAlignment { get; set; } = "Center";

        public string? VerticalAlignment { get; set; } = "Center";
    }
}
