using System.Windows.Media;

namespace PinPrompt.Controls.ColorSelector
{
    public static class ColorSelectorHelper
    {
        /// <summary>
        /// 十六进制颜色字符串转Brush
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        public static Brush HexToBrush(string hexColor, double opacity=1.0)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(hexColor))
                    return Brushes.Transparent;

                if (!hexColor.StartsWith("#"))
                    hexColor = "#" + hexColor;

                byte alpha = (byte)(opacity * 255 + 0.5);
                var color = (Color)ColorConverter.ConvertFromString(hexColor);
                color.A = alpha;
                return new SolidColorBrush(color);
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        /// <summary>
        /// 将Brush转换为十六进制颜色字符串
        /// </summary>
        /// <param name="brush"></param>
        /// <returns></returns>
        public static string BrushToHex(Brush brush)
        {
            if (brush is SolidColorBrush solidBrush)
            {
                Color color = solidBrush.Color;
                return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
            }

            // 对于其他类型的Brush，返回默认颜色
            return "#000000";
        }

        /// <summary>
        /// 根据WCAG 2.0标准计算对比色
        /// </summary>
        /// <param name="hexColor"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetContrastColorWCAG(string hexColor)
        {
            hexColor = hexColor.Trim().Replace("#", "");

            if (hexColor.Length != 6)
                throw new ArgumentException("颜色格式不正确，应为6位十六进制数");

            int r = Convert.ToInt32(hexColor.Substring(0, 2), 16);
            int g = Convert.ToInt32(hexColor.Substring(2, 2), 16);
            int b = Convert.ToInt32(hexColor.Substring(4, 2), 16);

            // WCAG 2.0亮度计算公式
            double luminance = GetLuminance(r, g, b);

            // 使用4.5:1的对比度阈值（WCAG AA标准）
            double contrastWithBlack = GetContrastRatio(luminance, 0);
            double contrastWithWhite = GetContrastRatio(luminance, 1);

            return contrastWithBlack > contrastWithWhite ? "#000000" : "#FFFFFF";
        }

        private static double GetLuminance(int r, int g, int b)
        {
            // 将sRGB分量转换为线性值
            double rs = r / 255.0;
            double gs = g / 255.0;
            double bs = b / 255.0;

            rs = rs <= 0.03928 ? rs / 12.92 : Math.Pow((rs + 0.055) / 1.055, 2.4);
            gs = gs <= 0.03928 ? gs / 12.92 : Math.Pow((gs + 0.055) / 1.055, 2.4);
            bs = bs <= 0.03928 ? bs / 12.92 : Math.Pow((bs + 0.055) / 1.055, 2.4);

            return 0.2126 * rs + 0.7152 * gs + 0.0722 * bs;
        }

        private static double GetContrastRatio(double luminance1, double luminance2)
        {
            double lighter = Math.Max(luminance1, luminance2);
            double darker = Math.Min(luminance1, luminance2);
            return (lighter + 0.05) / (darker + 0.05);
        }
    }
}
