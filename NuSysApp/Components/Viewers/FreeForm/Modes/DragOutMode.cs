using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;

namespace NuSysApp
{
    public class DragOutMode : AbstractWorkspaceViewMode
    {
        private FrameworkElement _dragItem;
        private NusysConstants.ElementType _elementType;
        private DispatcherTimer _timer;
        private int _counter = 0;
        private readonly int _waitTime = 500;
        private ElementViewModel _draggedVm;
        private PointerEventHandler _releasedHandler;
        private bool _released;

        public DragOutMode(FrameworkElement view) : base(view)
        {
            _releasedHandler = UserControlOnPointerReleased;
        }

        public override async Task Activate()
        {
            var wvm = (FreeFormViewerViewModel)_view.DataContext;
            SessionController.Instance.SessionView.AddHandler(UIElement.PointerReleasedEvent, _releasedHandler, true);

            foreach (var n in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                var userControl = (UserControl)n;
                if (userControl.DataContext is ElementViewModel)
                {
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationStarting += UserControlOnManipulationStarting;
                    userControl.ManipulationDelta += OnManipulationDelta;
              //      userControl.PointerReleased += UserControlOnPointerReleased;
                }
            }

            wvm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        public override async Task Deactivate()
        {
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;
            //          _view.PointerReleased -= UserControlOnPointerReleased;
            SessionController.Instance.SessionView.RemoveHandler(UIElement.PointerReleasedEvent, _releasedHandler);
            foreach (var n in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                var userControl = (UserControl)n;
                if (userControl.DataContext is ElementViewModel)
                {

                    userControl.ManipulationDelta -= OnManipulationDelta;
                    userControl.ManipulationStarting -= UserControlOnManipulationStarting;
                    userControl.PointerReleased -= UserControlOnPointerReleased;
                }
            }

            wvm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }

        private async void UserControlOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs manipulationStartingRoutedEventArgs)
        {
            _released = false;
            await Task.Delay(300);
            if (_released)
                return;
            var send = (FrameworkElement)sender;
          
            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            var img = new Image
            {
                RenderTransform = new CompositeTransform(),
                Source = bmp
            };
            _dragItem = img;
            _dragItem.IsHitTestVisible = false;
            _draggedVm = (ElementViewModel)send.DataContext;
            var point = this.GetRealCoordinatesOnScreen(sender);
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX = point.X;
            t.TranslateY = point.Y;

          //  SessionController.Instance.SessionView.MainCanvas.Children.Add(_dragItem);
        }

    


        //HANDLER METHODS START HERE
        private async void UserControlOnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
        //    if (IsPointerInGroup())
            _released = true;
            if (_dragItem == null)
                return;


            if (!IsPointerInGroup(pointerRoutedEventArgs.GetCurrentPoint(null).Position))
            {
                var t = (CompositeTransform) _dragItem.RenderTransform;
                var screenX = t.TranslateX;
                var screenY = t.TranslateY;
                var newPos =
                    SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(
                        new Point(screenX, screenY));
                var controller = SessionController.Instance.IdToControllers[_draggedVm.Id];

                _dragItem = null;
                _counter = 0;
                _timer?.Stop();
                _timer = null;

                

                await controller.RequestMoveToCollection(WaitingRoomView.InitialWorkspaceId, newPos.X, newPos.Y);
            }

            _dragItem = null;
            _counter = 0;
            _timer?.Stop();
            _timer = null;


            //  SessionController.Instance.SessionView.MainCanvas.Children.Remove(_dragItem);


        }


        private async void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;

            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX += args.Delta.Translation.X;
            t.TranslateY += args.Delta.Translation.Y;
        }


        private void AtomViewListOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            var newItems = notifyCollectionChangedEventArgs.NewItems;
            if (newItems == null)
                return;

            var newNodes = newItems;
            foreach (var n in newNodes)
            {
                var userControl = (UserControl)n;
                if (userControl.DataContext is ElementViewModel)
                {
                    userControl.ManipulationDelta += OnManipulationDelta;
                    userControl.ManipulationStarting += UserControlOnManipulationStarting;
                    userControl.PointerReleased += UserControlOnPointerReleased;
                }
            }
        }

       

        private Point GetRealCoordinatesOnScreen(object sender)
        {
            var cview = (AreaNodeView)_view;
            var vm = (AreaNodeViewModel)cview.DataContext;
            var send = (FrameworkElement)sender;
            var sendVm = (ElementViewModel)send.DataContext;
            var model = sendVm.Model;
            var groupModel = vm.Model;
            var point = vm.CompositeTransform.TransformPoint(new Point(model.X, model.Y));
            var x = point.X + groupModel.X;
            var y = point.Y + groupModel.Y;
            var point2 = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.TransformPoint(new Point(x, y));
            return point2;
        }

        private bool IsPointerInGroup(Point point)
        {
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(point, SessionController.Instance.SessionView);
            var result = hits.Where((uiElem) => uiElem is AreaNodeView);
            return result.Any();
        }



        private void StopTimer(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            if (_dragItem != null)
            {
                SessionController.Instance.SessionView.MainCanvas.Children.Remove(_dragItem);
            }
            if (sender != null)
            {
                var send = (FrameworkElement)sender;
                if (send != null)
                {
                    var sendVm = (ElementViewModel)send.DataContext;
                    if (sendVm != null)
                        sendVm.IsVisible = true;
                }

            }
            _dragItem = null;
            _counter = 0;
            _timer?.Stop();
            _timer = null;
        }
    }
}
