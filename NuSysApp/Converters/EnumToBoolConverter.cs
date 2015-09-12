using System;
using Windows.UI.Xaml.Data;

namespace NuSysApp
{
    public class EnumToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var status = (AtomModel.EditStatus) value;
            if (status == AtomModel.EditStatus.No)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
