using Windows.UI.Xaml.Controls;
using NuSysApp.Components.Viewers.FreeForm;

namespace NuSysApp
{
    public class ToolViewModel
    {
        private ToolController _controller;
        public ToolViewModel(ToolController toolController)
        {
            _controller = toolController;
        }
        
        public void SetSelection(string selection)
        {
            _controller.SetSelection(selection);
        }

        public ToolModel.FilterTitle Filter { get { return _controller.Model.Filter;}  set { _controller.SetFilter(value);} }

        public void CreateNewToolWindow(Canvas canvas, double x, double y)
        {
            ToolModel model = new ToolModel();
            ToolController controller = new ToolController(model);
            controller.AddParent(controller);
            ToolViewModel viewmodel = new ToolViewModel(controller);
            ToolView view = new ToolView(viewmodel, x, y);
            canvas.Children.Add(view);
            

        }
    }
}