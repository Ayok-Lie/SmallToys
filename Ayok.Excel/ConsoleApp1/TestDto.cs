using System.ComponentModel.DataAnnotations;
using Ayok.Excel.Attributes;
using Ayok.Excel.Enums;

namespace ConsoleApp1
{
    public class TestDto
    {
        [Required(ErrorMessage = "参考日均产能必填！")]
        [ExcelColumn("*参考日均产能（万）",ExcelColumnEnum.常规,0)]
        public string? average_daily_capacity { get; set; }

        /// <summary>
        /// 商品供应商
        ///</summary>
        [Required(ErrorMessage = "商品供应商必填！")]
        [ExcelColumn("*商品供应商", ExcelColumnEnum.常规, 1)]
        public string goods_supplier { get; set; }


        /// <summary>
        /// 产品功能性状
        ///</summary>
        [Required(ErrorMessage = "产品功能性状必填！")]
        [ExcelColumn("*产品功能性状", ExcelColumnEnum.常规, 2)]
        public string product_functional_attribute { get; set; }




        /// <summary>
        /// 生产许可证书
        ///</summary>
        [ExcelColumn("生产许可证书", ExcelColumnEnum.网络图片, 5)]
        public string production_license_str { get; set; }


        /// <summary>
        /// 生产许可证书
        ///</summary>
        [System.Text.Json.Serialization.JsonIgnore]
        public List<string> production_license => !string.IsNullOrEmpty(production_license_str) ?
            production_license_str.Split(new[] { ',' }).ToList() : new List<string>();




        /// <summary>
        /// 备注
        ///</summary>
        [StringLength(300, ErrorMessage = "备注必须小于300个字符！")]
        [ExcelColumn("备注",ExcelColumnEnum.本地图片, 999)]
        public string? remark { get; set; }
    }
}
