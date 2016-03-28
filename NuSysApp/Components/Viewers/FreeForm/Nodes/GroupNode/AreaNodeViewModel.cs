using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Util;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public class AreaNodeViewModel : FreeFormViewerViewModel
    {

        public PointCollection Points { get; set; }

        public PathGeometry PathGeometry { get; set; }

        public Rect ClipRect { get; set; }
        public AreaNodeViewModel(ElementCollectionController controller):base(controller)
        {

            ClipRect = new Rect(0, 0, controller.Model.Width, controller.Model.Height);
            
            Controller.SizeChanged += ControllerOnSizeChanged;
            Controller.ScaleChanged += ControllerOnScaleChanged;

        }

        private void ControllerOnScaleChanged(object source, double sx, double sy)
        {
            double width = Controller.Model.Width;
            double height = Controller.Model.Height;
            ClipRect = new Rect(0, 0, width * sx, height * sy);
            RaisePropertyChanged("ClipRect");
        }

        private void ControllerOnSizeChanged(object source, double width, double height)
        {
            ClipRect = new Rect(0, 0, width, height);
            RaisePropertyChanged("ClipRect");
        }

        public override void Dispose()
        {
            Controller.SizeChanged -= ControllerOnSizeChanged;
            Controller.ScaleChanged -= ControllerOnScaleChanged;
            base.Dispose();
        }
    }
}
