using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    class TransformHelper
    {
        
        /// <summary>
        /// Will make a full screen appeareance for the passed in element view model. Use this for presentation view.
        /// </summary>
        /// <param name="e"></param>

        public static void FullScreen(ElementModel e)
        {
            // Define some variables that will be used in future translation/scaling
            var sv = SessionController.Instance.SessionView;
            var x = e.X + e.Width / 2;
            var y = e.Y + e.Height / 2;
            var widthAdjustment = sv.ActualWidth / 2;
            var heightAdjustment = sv.ActualHeight / 2;

            // Reset the scaling and translate the free form viewer so that the passed in element is at the center
            var compositeTransform = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform;
            compositeTransform.ScaleX = 1;
            compositeTransform.ScaleY = 1;
            compositeTransform.TranslateX = widthAdjustment - x;
            compositeTransform.TranslateY = heightAdjustment - y;

            // Obtain correct scale value based on width/height ratio of passed in element
            double scale;
            if (e.Width > e.Height)
                scale = sv.ActualWidth / e.Width;
            else
                scale = sv.ActualHeight / e.Height;

            // Scale the active free form viewer so that the passed in element appears to be full screen.
            scale = scale * .7; // adjustment so things don't get cut off
            compositeTransform.CenterX = x;
            compositeTransform.CenterY = y;
            compositeTransform.ScaleX = scale;
            compositeTransform.ScaleY = scale;
        }


      
    }
}
