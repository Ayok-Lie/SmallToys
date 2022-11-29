using QRCoder;
using static QRCoder.QRCodeGenerator;

namespace SmallToys_QRCoder
{
    public static class QRCoderHelper
    {
        ///生成二维码
        /// </summary>
        /// <param name="TextContent">文本信息</param>
        /// <param name="level">容错等级</param>
        /// <param name="version">版本</param>
        /// <param name="pixel">像素点大小</param>
        /// <param name="darkColor">数点颜色</param>
        /// <param name="lightColor">背景颜色</param>
        /// <param name="iconPath">图标路径</param>
        /// <param name="iconSize">图标尺寸</param>
        /// <param name="iconBorder">图标边框厚度</param>
        /// <param name="whiteBorder">二维码白边</param>
        /// <returns></returns>
        public static Bitmap generateQrCode(string TextContent, string level, int version, int pixel, Color darkColor, Color lightColor, string iconPath, int iconSize, int iconBorder, bool whiteBorder)
        {
            ECCLevel eccLevel = (ECCLevel)(level == "L" ? 0 : level == "M" ? 1 : level == "Q" ? 2 : 3);
            QRCodeGenerator codeGenerator = new QRCodeGenerator();

            QRCodeData codeData = codeGenerator.CreateQrCode(TextContent, eccLevel, false, false, EciMode.Utf8, version);

            QRCode code = new QRCode(codeData);

            if (iconPath == "")
            {
                Bitmap bmp = code.GetGraphic(pixel, darkColor, lightColor, whiteBorder);

                return bmp;
            }
            Bitmap icon = new Bitmap(iconPath);

            Bitmap iocnbmp = code.GetGraphic(pixel, darkColor, lightColor, icon, iconSize, iconBorder, whiteBorder);

            return iocnbmp;
        }
    }
}