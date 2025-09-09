using System.ComponentModel.DataAnnotations;
using Ayok.Excel.Attributes;
using Ayok.Excel.Enums;
using OfficeOpenXml.Style;

namespace ConsoleApp1
{
    public class TestDto
    {
        [Required(ErrorMessage = "参考日均产能必填！")]
        [ExcelColumn("*参考日均产能（万）", Type = ExcelColumnEnum.常规, Order = 0)]
        public string? average_daily_capacity { get; set; }

        /// <summary>
        /// 商品供应商
        ///</summary>
        [Required(ErrorMessage = "商品供应商必填！")]
        [ExcelColumn("*商品供应商", Type = ExcelColumnEnum.常规, Order = 1)]
        public string goods_supplier { get; set; }

        /// <summary>
        /// 产品功能性状
        ///</summary>
        [Required(ErrorMessage = "产品功能性状必填！")]
        [ExcelColumn("*产品功能性状", Type = ExcelColumnEnum.常规, Order = 2)]
        public string product_functional_attribute { get; set; }

        /// <summary>
        /// 生产许可证书
        ///</summary>
        [ExcelColumn("生产许可证书", Type = ExcelColumnEnum.网络图片, Order = 5)]
        public string production_license_str { get; set; }

        /// <summary>
        /// 生产许可证书
        ///</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public List<string> production_license =>
            !string.IsNullOrEmpty(production_license_str)
                ? production_license_str.Split(new[] { ',' }).ToList()
                : new List<string>();

        /// <summary>
        /// 备注
        ///</summary>
        [StringLength(300, ErrorMessage = "备注必须小于300个字符！")]
        [ExcelColumn("备注", Type = ExcelColumnEnum.本地图片, Order = 999)]
        public string? remark { get; set; }
    }
}
