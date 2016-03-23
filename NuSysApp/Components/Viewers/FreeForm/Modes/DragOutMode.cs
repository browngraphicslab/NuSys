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

namespace NuSysApp
{
    public class DragOutMode : AbstractWorkspaceViewMode
    {
        private FrameworkElement _dragItem;
        private ElementType _elementType;
        private DispatcherTimer _timer;
        private int _counter = 0;
        private readonly int _waitTime = 500;
       
        public DragOutMode(FrameworkElement view) : base(view){ }

        public override async Task Activate()
        {
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;

            foreach (var n in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                var userControl = (UserControl)n;
                if (userControl.DataContext is ElementViewModel)
                {
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationDelta += OnManipulationDelta;
                    userControl.PointerReleased += UserControlOnPointerReleased;
                }
            }

            wvm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        public override async Task Deactivate()
        {
            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;

            foreach (var n in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                var userControl = (UserControl)n;
                if (userControl.DataContext is ElementViewModel)
                {
                    
                    userControl.ManipulationDelta -= OnManipulationDelta;
                    userControl.PointerReleased -= UserControlOnPointerReleased;
                }
            }

            wvm.AtomViewList.CollectionChanged -= AtomViewListOnCollectionChanged;
        }


        //HANDLER METHODS START HERE
        private void UserControlOnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            _dragItem = null;
            _counter = 0;
            _timer?.Stop();
            _timer = null;
        }

       
        private async void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            if (!IsPointerInGroup(sender, args))
            {
                this.StartTimer(sender, args);
            }
            else
            {
                this.StopTimer(sender, args);
            }
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
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationDelta += OnManipulationDelta;
                    userControl.PointerReleased += UserControlOnPointerReleased;
                }
            }
        }

        //PRIVATE HELPER METHODS START HERE
        private void SetUpDragObject(object sender)
        {
            if (_dragItem == null)
                return;
            var cview = (AreaNodeView)_view;
            var vm = (AreaNodeViewModel)cview.DataContext;
            var send = (FrameworkElement)sender;
            var sendVm = (ElementViewModel)send.DataContext;
            var model = sendVm.Model;
            var groupModel = vm.Model;
            var point = this.GetRealCoordinatesOnScreen(sender);
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX = point.X;
            t.TranslateY = point.Y;
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

        private bool IsPointerInGroup(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            var point = ((UIElement) sender).TransformToVisual(SessionController.Instance.SessionView.MainCanvas);
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(this.GetRealCoordinatesOnScreen(sender), SessionController.Instance.SessionView);
            var result = hits.Where((uiElem) => uiElem is AreaNodeView);
            return result.Any();
        }

        private void StartTimer(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            var send = (FrameworkElement)sender;
            var sendVm = (ElementViewModel)send.DataContext;
            var model = sendVm.Model;
            if (_timer == null)
            {
                _timer = new DispatcherTimer();
                _timer.Tick += async delegate (object o, object o1)
                {
                    _counter++;
                    if (_counter == 1)//This happens after dragging a node out and waiting 0.5 seconds 
                    {
                        var bmp = new RenderTargetBitmap();
                        await bmp.RenderAsync((UIElement)sender);
                        var img = new Image
                        {
                            RenderTransform = new CompositeTransform(),
                            Source = bmp
                        };
                        _dragItem = img;
                        SessionController.Instance.SessionView.MainCanvas.Children.Add(_dragItem);
                        this.SetUpDragObject(sender);
                        var t = (CompositeTransform)_dragItem.RenderTransform;
                        t.TranslateX += args.Delta.Translation.X;
                        t.TranslateY += args.Delta.Translation.Y;

                        sendVm.IsVisible = false;
                    }
                    if (_counter == 2) //This happens after waiting another 0.5 seconds
                    {
                        var t = (CompositeTransform)_dragItem.RenderTransform;
                        var screenX = t.TranslateX;
                        var screenY = t.TranslateY;
                        var newPos = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(screenX, screenY));
                        var controller = SessionController.Instance.IdToControllers[model.Id];
                        await controller.RequestMoveToCollection(WaitingRoomView.InitialWorkspaceId, newPos.X, newPos.Y);
                        this.StopTimer(sender, args);
                    }
                };
                _timer.Interval = TimeSpan.FromMilliseconds(_waitTime);
                _timer.Start();

            }
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
