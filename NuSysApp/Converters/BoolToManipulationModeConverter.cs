using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;


namespace NuSysApp
{
    class BoolToManipulationModeConverter : IValueConverter
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
                    //Windows.UI.Xaml.UIElement. 
                    return Windows.UI.Xaml.Input.ManipulationModes.None;
                }
                else
                {
                    return Windows.UI.Xaml.Input.ManipulationModes.All;
                }
            }
            else
            {
                if ((bool)value)
                {
                    return Windows.UI.Xaml.Input.ManipulationModes.All;
                }
                else
                {
                    return Windows.UI.Xaml.Input.ManipulationModes.None;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
