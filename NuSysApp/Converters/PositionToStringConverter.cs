using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;

namespace NuSysApp
{
    public class PositionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            String hours = "";
            String minutes = "";
            String seconds = "";
            TimeSpan position = (TimeSpan) value;
            if (position.Hours.ToString().Length == 1 && position.Hours != 0)
            {
                hours = "0" + position.Hours.ToString();
            }
            else if (position.Hours != 0)
            {
                hours = position.Hours.ToString();
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

            // if we've set hours to a value
            if (!string.IsNullOrEmpty(hours))
            {
                return hours + ":" + minutes + ":" + seconds;
            }
            else
            {
                return minutes + ":" + seconds;
            }

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}