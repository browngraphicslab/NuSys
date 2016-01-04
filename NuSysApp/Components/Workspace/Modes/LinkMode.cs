using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp;
using ReverseMarkdown.Converters;

namespace NuSysApp { 
    public class LinkMode : AbstractWorkspaceViewMode
    {
        public LinkMode(WorkspaceView view) : base(view)
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

        private void OnLineFinalized(InqLineModel lineModel)
        {
            var Points = new List<Point>();

            var unNormalizedPoints = lineModel.Points.Select(p => new Point(p.X * Constants.MaxCanvasSize, p.Y * Constants.MaxCanvasSize));
            foreach (var p in unNormalizedPoints)
            {
                Points.Add(p);
            }

            var t = (_view.DataContext as WorkspaceViewModel).CompositeTransform;

            var pStart = t.TransformPoint(Points.First());
            var pEnd = t.TransformPoint(Points.Last());

            
            var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(pStart, _view);
            hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is AtomViewModel && !((uiElem as FrameworkElement).DataContext is WorkspaceViewModel));
            var hitsEnd = VisualTreeHelper.FindElementsInHostCoordinates(pEnd, _view).Where(uiElem => (uiElem as FrameworkElement).DataContext is AtomViewModel);
            hitsEnd = hitsEnd.Where(uiElem => (uiElem as FrameworkElement).DataContext is AtomViewModel && !((uiElem as FrameworkElement).DataContext is WorkspaceViewModel));

            if (hitsEnd.Count() == 0 || hitsStart.Count() == 0)
                return;

            var startVm = (hitsStart.First() as FrameworkElement).DataContext as AtomViewModel;
            var endVm = (hitsEnd.First() as FrameworkElement).DataContext as AtomViewModel;


            if (startVm == endVm)
                return;

           // var linkVm = new LinkViewModel(new LinkModel(startVm.Model, endVm.Model, "LINK_ID"), startVm, endVm);

            NetworkConnector.Instance.RequestMakeLinq(startVm.Id, endVm.Id, null, null, null);
          //  startVm.AddLink(linkVm);
          //  endVm.AddLink(linkVm);
           // var linkView = new BezierLinkView(linkVm);
           // (_view.DataContext as WorkspaceViewModel).AtomViewList.Add(linkView);

                lineModel.Delete();
            
        }
    }
}
