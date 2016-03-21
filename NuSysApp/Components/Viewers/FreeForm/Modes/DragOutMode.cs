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


        public DragOutMode(FrameworkElement view) : base(view)
        {

        }

        public override async Task Activate()
        {
            //_view.ManipulationMode = ManipulationModes.All;
            //_view.ManipulationDelta += OnManipulationDelta;
            //_view.ManipulationStarted += OnManipulationStarted;
            //_view.ManipulationStarting += OnManipulationStarting;

            FreeFormViewerViewModel wvm = (FreeFormViewerViewModel)_view.DataContext;

            foreach (var n in wvm.AtomViewList.Where(s => s.DataContext is ElementViewModel))
            {
                var userControl = (UserControl)n;
                if (userControl.DataContext is ElementViewModel)
                {
                    userControl.ManipulationMode = ManipulationModes.All;
                    userControl.ManipulationDelta += OnManipulationDelta;
                    userControl.ManipulationStarting += OnManipulationStarting;
                    userControl.ManipulationStarted += OnManipulationStarted;
                    userControl.PointerReleased += UserControlOnPointerReleased;
                }
            }

            wvm.AtomViewList.CollectionChanged += AtomViewListOnCollectionChanged;
        }

        private void UserControlOnPointerReleased(object sender, PointerRoutedEventArgs pointerRoutedEventArgs)
        {
            _dragItem = null;
            _counter = 0;
            if (_timer != null)
            {
                _timer.Stop();
            }
            _timer = null;
        }


        public override Task Deactivate()
        {
            throw new NotImplementedException();
        }

        private async void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {
            var cview = (AreaNodeView)_view;
            var vm = (AreaNodeViewModel)cview.DataContext;
            var send = (FrameworkElement)sender;
            var sendVm = (ElementViewModel)send.DataContext;
            var model = sendVm.Model;
            var groupModel = vm.Model;

            var point = vm.CompositeTransform.TransformPoint(args.Position);
            var x = point.X + groupModel.X;
            var y = point.Y + groupModel.Y; var point2 = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.TransformPoint(new Point(x, y));
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(new Point(x, y), SessionController.Instance.SessionView);
            var result = hits.Where((uiElem) =>
            {
                var fe = (FrameworkElement)uiElem;
                var r = fe is AreaNodeView;
                return r;
            });
            if (!result.Any())
            {

                if (_timer == null)
                {
                    
                    _timer = new DispatcherTimer();
                    _timer.Tick += async delegate (object o, object o1)
                    {
                        _counter++;
                        if (_counter == 1)
                        {
                            
                      
                            var bmp = new RenderTargetBitmap();
                            await bmp.RenderAsync((UIElement)sender);
                            var img = new Image
                            {
                                Opacity = 30,
                                RenderTransform = new CompositeTransform(),
                                Source = bmp
                            };

                            _dragItem = img;
                            //cview.OuterCanvas.Children.Add(_dragItem);
                            SessionController.Instance.SessionView.MainCanvas.Children.Add(_dragItem);
                            this.SetUpObject(sender);
                            var t = (CompositeTransform)_dragItem.RenderTransform;
                            t.TranslateX += args.Delta.Translation.X;
                            t.TranslateY += args.Delta.Translation.Y;

                        sendVm.IsVisible = false;
                        }
                        if (_counter == 2)
                        {
                            var controller = SessionController.Instance.IdToControllers[model.Id];
                            await controller.RequestMoveToCollection(WaitingRoomView.InitialWorkspaceId);
                            _timer.Stop();
                            _counter = 0;
                        }
                        
                        
                        
                    };
                    _timer.Interval = TimeSpan.FromMilliseconds(500);
                    _timer.Start();

                }




               
            }
            else
            {
                return;
            }


         
            //var d = (AreaNodeViewModel) (cview.DataContext);
            ////rect.Intersect(d.ClipRect);
            //if (rect.IsEmpty)
            //{
            //    //START TIMER
            //    int a = 0;
            //}
            //var a = VisualTreeHelper.  var cview = (AreaNodeView)_view;







        }

        private int _counter = 0;
        private void SetUpObject(object sender)
        {
            if (_dragItem == null)
                return;
            _dragItem.Opacity = 0.5;
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
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX = point2.X;
            t.TranslateY = point2.Y;
        }

        private void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs args)
        {
            //if (_dragItem == null)
            //    return;
            //_dragItem.Opacity = 0.5;
            //var cview = (AreaNodeView)_view;
            //var vm = (AreaNodeViewModel) cview.DataContext;
            //var send = (FrameworkElement) sender;
            //var sendVm = (ElementViewModel)send.DataContext;
            //var model = sendVm.Model;
            //var groupModel = vm.Model;
            //var point = vm.CompositeTransform.TransformPoint(new Point(model.X, model.Y));
            //var x = point.X + groupModel.X;
            //var y = point.Y + groupModel.Y;
            //var point2 = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.TransformPoint(new Point(x, y));
            //var t = (CompositeTransform) _dragItem.RenderTransform;
            //t.TranslateX = point2.X;
            //t.TranslateY = point2.Y;
            
          
        }
   
        private async void OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs args)
        {
            //_elementType = sender == btnAddNode ? ElementType.Text : ElementType.Image;
           

            //args.Handled = true;
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
                    userControl.ManipulationStarting += OnManipulationStarting;
                    userControl.ManipulationStarted += OnManipulationStarted;
                    userControl.PointerReleased += UserControlOnPointerReleased;
                }
            }
        }


    }
}
