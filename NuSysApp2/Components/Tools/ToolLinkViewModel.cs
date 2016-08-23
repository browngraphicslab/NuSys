using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp2
{
    public class ToolLinkViewModel : BaseINPC
    {

        public ToolController InToolController;
        public ToolController OutToolController;

        public ToolViewModel InTool;
        public ToolViewModel OutTool;



        //public LinkModel LinkModel { get; }
        //private SolidColorBrush _defaultColor;
        public ToolLinkViewModel(ToolViewModel inTool, ToolViewModel outTool)
        {
            //InToolController
            InTool = inTool;
            OutTool = outTool;
            

            InToolController = inTool.Controller;
            OutToolController = outTool.Controller;

            InToolController.LocationChanged += InToolController_LocationChanged; ;
            OutToolController.LocationChanged += OutToolController_LocationChanged; ;
            InToolController.SizeChanged += OutElementControllerOnSizeChanged;
            OutToolController.SizeChanged += OutElementControllerOnSizeChanged;

            

        }

        public void Dispose()
        {
            InToolController.LocationChanged -= InToolController_LocationChanged; ;
            OutToolController.LocationChanged -= OutToolController_LocationChanged; ;
            InToolController.SizeChanged -= OutElementControllerOnSizeChanged;
            OutToolController.SizeChanged -= OutElementControllerOnSizeChanged;
        }

        private void OutToolController_LocationChanged(object sender, double x, double y)
        {
            RaisePropertyChanged("Anchor");
        }

        private void InToolController_LocationChanged(object sender, double x, double y)
        {
            RaisePropertyChanged("Anchor");
        }

        private void OutElementControllerOnSizeChanged(object source, double width, double height)
        {
            RaisePropertyChanged("Anchor");
        }

        private void InElementControllerOnPositionChanged(object source, double x, double y, double dx, double dy)
        {
            RaisePropertyChanged("Anchor");
        }
    }
}