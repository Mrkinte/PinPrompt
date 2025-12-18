using System.Globalization;
using System.Windows.Data;

namespace PinPrompt.Helpers
{
    public class InvertibleBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = value is true;

            // 检查是否需要反转
            if (parameter?.ToString()?.ToLower() == "invert")
                isVisible = !isVisible;

            return isVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
