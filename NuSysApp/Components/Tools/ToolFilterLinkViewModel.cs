namespace NuSysApp {
    public class ToolFilterLinkViewModel : BaseINPC
    {
        public ToolController InToolController;

        public ElementController InToolElementController;

        public ToolViewModel InTool;

        public ToolFilterView OutTool;


        //public LinkModel LinkModel { get; }
        //private SolidColorBrush _defaultColor;
        public ToolFilterLinkViewModel(ToolViewModel inTool, ToolFilterView outTool)
        {
            //InToolController
            InTool = inTool;
            OutTool = outTool;
            

            InToolController = inTool.Controller;

            InToolController.LocationChanged += InToolController_LocationChanged; 
            InToolController.SizeChanged += OutElementControllerOnSizeChanged;

            OutTool.LocationChanged += OutToolController_LocationChanged;
            OutTool.SizeChanged += OutElementControllerOnSizeChanged;

        }

        public void Dispose()
        {
            InToolController.LocationChanged -= InToolController_LocationChanged; ;
            OutTool.LocationChanged -= OutToolController_LocationChanged; ;
            InToolController.SizeChanged -= OutElementControllerOnSizeChanged;
            OutTool.SizeChanged -= OutElementControllerOnSizeChanged;
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