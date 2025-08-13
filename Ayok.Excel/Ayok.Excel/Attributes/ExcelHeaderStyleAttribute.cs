using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayok.Excel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExcelHeaderStyleAttribute : Attribute
    {
        public Color FontColor { get; set; }

        public int FontSize { get; set; }

        public ExcelHeaderStyleAttribute(int r = 0, int g = 0, int b = 0, int fontSize = 11)
        {
            FontColor = Color.FromArgb(r, g, b);
            FontSize = fontSize;
        }
    }

}
