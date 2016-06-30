using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Converters
{
    /// <summary>
    /// Used to convert the Mutability property in Metadata Entries to the Brush
    /// property of the rows in the list view
    /// </summary>
    public class MutabilityToBrushConverter : IValueConverter
    {

        /// <summary>
        /// Converts the passed in Mutability into a SolidColorBrush for the MD Editor
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((MetadataMutability) value == MetadataMutability.IMMUTABLE)
            {
                return new SolidColorBrush() {Color = Colors.LightGray, Opacity = .5};
            }
           return new SolidColorBrush() { Color = Colors.DarkGray };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
