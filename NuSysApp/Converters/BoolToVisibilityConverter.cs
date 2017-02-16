using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NuSysApp
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            bool invertOuput = false;

            if (parameter != null)
            {
                bool.TryParse((string)parameter, out invertOuput);
            }
            if (invertOuput)
            {
                if ((bool)value)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }
            if ((bool)value)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
