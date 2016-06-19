using System;
using System.Diagnostics;
using Windows.UI.Xaml.Data;


namespace NuSysApp
{
    class RegionModelToViewConverter : IValueConverter
    {
        /// <summary>
        /// Consumes a RegionViewModel and produces a RegionView of the corresponding type.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            var region = value as Region;

            switch (region.Type)
            {
                case Region.RegionType.Rectangle:
                    var rectregion = (RectangleRegion)value;
                    return new ImageRegionView(new ImageRegionViewModel(rectregion, parameter as LibraryElementController));
                    break;
                default:
                    return null;
                    break;

                
            }
            //if value as ImageRegionViewModel 
            //  

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
