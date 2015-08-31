using System;
using Windows.UI;
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
                Color selColor = (Color)parameter;
                selColor.A = 255;    
                return selColor;
            }
            Color unselColor = (Color)parameter;
            unselColor.A = 150;
            return unselColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
