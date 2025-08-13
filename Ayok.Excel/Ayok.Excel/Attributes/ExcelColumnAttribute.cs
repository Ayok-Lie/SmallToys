using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ayok.Excel.Enums;

namespace Ayok.Excel.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ExcelColumnAttribute : Attribute
    {
        /// <summary>
        /// 列名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 类型
        /// </summary>
        public ExcelColumnEnum Type { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Order { get; set; } = 999999999;

        public ExcelColumnAttribute(string name,ExcelColumnEnum type, int order)
        {
            Name = name;
            Type = type;
            Order = order;
        }
    }

}
