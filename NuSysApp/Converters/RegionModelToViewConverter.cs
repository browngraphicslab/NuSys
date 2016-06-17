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
            switch (parameter as string)
            {
                // case enum.Image:
                //return new ImageRegionView(value as ImageRegionViewModel);
                // case enum.Audio:
                //return new AudioRegionView(value as AudioRegionViewModel);
                // case enum.PDF:
                //return new PDFRegionView(value as PDFRegionViewModel);
                //default:
                //return null;
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
