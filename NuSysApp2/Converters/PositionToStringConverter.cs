using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace NuSysApp2
{
    public class PositionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            String hours = "";
            String minutes = "";
            String seconds = "";
            TimeSpan position = (TimeSpan) value;
            if (position.Hours.ToString().Length == 1)
            {
                minutes = "0" + position.Hours.ToString();
            }
            else
            {
                minutes = position.Hours.ToString();
            }
            if (position.Minutes.ToString().Length == 1)
            {
                minutes = "0" + position.Minutes.ToString();
            }
            else
            {
                minutes = position.Minutes.ToString();
            }
            if (position.Seconds.ToString().Length == 1)
            {
                seconds = "0" + position.Seconds.ToString();
            }
            else
            {
                seconds = position.Seconds.ToString();
            }
            return minutes + ":" + seconds;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}