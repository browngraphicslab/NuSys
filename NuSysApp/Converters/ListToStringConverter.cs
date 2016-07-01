using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace NuSysApp.Converters
{
    /// <summary>
    /// Used to parse a list into a string
    /// </summary>
    public class ListToStringConverter:IValueConverter
    {
        /// <summary>
        /// Converts the list into a string by looping thru all elements and adding to a string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var words = (List<string>) value ?? new List<string>();
            if (words.Count() == 0)
            {
                return "";
            }
            var text = "";
            foreach (var word in words)
            {
                text = text + word + ", ";
            }
            return text.Substring(0, text.Length - 2);

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
