using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ayok.Excel.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExcelSheetAttribute : Attribute
    {
        public string Name { get; set; } = "sheet1";

        public int ColumnWidth { get; set; } = 15;

        public bool AutoColumnWidth { get; set; }
    }

}
