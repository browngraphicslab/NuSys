using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace NuStarterProject
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
                else
                {
                    return Visibility.Visible;
                }
            }
            else
            {
                if ((bool)value)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed; 
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
