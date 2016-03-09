using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MyToolkit.UI;
using NuSysApp;
using ReverseMarkdown.Converters;

namespace NuSysApp { 
    public class LinkMode : AbstractWorkspaceViewMode
    {
        public LinkMode(FreeFormViewer view) : base(view)
        {
        }

        public async override Task Activate()
        {
            _view.InqCanvas.ViewModel.Model.LineFinalized += OnLineFinalized;  
        }


        public async override Task Deactivate()
        {
            _view.InqCanvas.ViewModel.Model.LineFinalized -= OnLineFinalized;
        }

        private async void OnLineFinalized(InqLineModel lineModel)
        {
            var unNormalizedPoints = lineModel.Points.Select(p => new Point(p.X * Constants.MaxCanvasSize, p.Y * Constants.MaxCanvasSize));
            var points = unNormalizedPoints.ToList();

            var t = (_view.DataContext as FreeFormViewerViewModel).CompositeTransform;

            var pStart = t.TransformPoint(points.First());
            var pEnd = t.TransformPoint(points.Last());

            
            var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(pStart, _view);
            hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel && !((uiElem as FrameworkElement).DataContext is FreeFormViewerViewModel));
            var hitsEnd = VisualTreeHelper.FindElementsInHostCoordinates(pEnd, _view).Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel);
            hitsEnd = hitsEnd.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel && !((uiElem as FrameworkElement).DataContext is FreeFormViewerViewModel));

            if (hitsEnd.Count() == 0 || hitsStart.Count() == 0)
                return;

            var startVm = (hitsStart.First() as FrameworkElement).DataContext as ElementViewModel;
            var endVm = (hitsEnd.First() as FrameworkElement).DataContext as ElementViewModel;

            // Don't allow links where the start and end atom are identical
            if (startVm == endVm)
                return;

            startVm.Controller.RequestLinkTo(endVm.Id);

            lineModel.Delete();
            
        }
    }
}
