using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class SelectionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((bool)value)
            {
                return new SolidColorBrush(Constants.SELECTED_COLOR);
            }
            return new SolidColorBrush(Constants.DEFAULT_COLOR);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
