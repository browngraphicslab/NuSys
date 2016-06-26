using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class ToolLinkViewModel : BaseINPC
    {

        public ToolController InToolController;
        public ToolController OutToolController;

        public ElementController InToolElementController;
        public ElementController OutToolElementController;

        public TemporaryToolView InTool;
        public TemporaryToolView OutTool;



        //public LinkModel LinkModel { get; }
        //private SolidColorBrush _defaultColor;
        public ToolLinkViewModel(TemporaryToolView inTool, TemporaryToolView outTool)
        {
            //InToolController
            InTool = inTool;
            OutTool = outTool;


            InToolElementController = ((ToolViewModel)inTool.DataContext).Controller;
            OutToolElementController = ((ToolViewModel)outTool.DataContext).Controller;

            InToolElementController.PositionChanged += InElementControllerOnPositionChanged;
            OutToolElementController.PositionChanged += InElementControllerOnPositionChanged;
            InToolElementController.SizeChanged += OutElementControllerOnSizeChanged;
            OutToolElementController.SizeChanged += OutElementControllerOnSizeChanged;

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
