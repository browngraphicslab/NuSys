using System;
using Windows.UI.Xaml.Data;


namespace NuSysApp2
{
    public class SubStractConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            double s;
            Double.TryParse((string)parameter, out s);
            return ((double)value) - s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

    }
}