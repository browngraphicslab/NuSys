using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public sealed partial class GroupNodeDataGridView : AnimatableUserControl
    {
        private DispatcherTimer _timer;

        public GroupNodeDataGridView(GroupNodeDataGridViewModel viewModel)
        {
           DataContext = viewModel;
           this.InitializeComponent();

       
            DataGrid.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true );
            DataGrid.AddHandler(UIElement.ManipulationDeltaEvent, new ManipulationDeltaEventHandler(OnManipulationDelta), true);
            DataGrid.AddHandler(UIElement.ManipulationStartedEvent, new ManipulationStartedEventHandler(OnManipulationStarted), true);
            DataGrid.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), true);
            DataGrid.ManipulationMode = ManipulationModes.All;
            SessionController.Instance.SessionView.MainCanvas.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), true);

        }

        private Rectangle _drag;
        private String _id;
        private void OnPointerPressed(object source, PointerRoutedEventArgs args)
        {
            var src = (FrameworkElement) args.OriginalSource;
            if (src.DataContext is GroupNodeDataGridInfo)
            {
                //BitmapImage bmp = new BitmapImage(new Uri("ms-appx:///Assets//icon_additional.png"));

                //var img = new Image
                //{
                //    RenderTransform = new CompositeTransform(),
                //    Source = bmp
                //};

                _drag = new Rectangle
                {
                    Width = 100,
                    Height = 20,
                    Fill = new SolidColorBrush(Colors.Crimson)
                };
                var point = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
                Canvas.SetLeft(_drag, point.X);
                Canvas.SetTop(_drag, point.Y);
                SessionController.Instance.SessionView.MainCanvas.Children.Add(_drag);
                //Canvas.SetLeft(img, 0);
                //Canvas.SetTop(img, 0);
                var info = (GroupNodeDataGridInfo) src.DataContext;
                _id = info.Id;
            }
        }

        private async void OnPointerReleased(object source, PointerRoutedEventArgs args)
        {
            if (_drag != null)
            {
                var point = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
                if (!this.IsPointerInGroup(point))
                {
                    var newPos = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(point);
                    var controller = SessionController.Instance.IdToControllers[_id];
                    await controller.RequestMoveToCollection(WaitingRoomView.InitialWorkspaceId, newPos.X, newPos.Y);
                }
                SessionController.Instance.SessionView.MainCanvas.Children.Remove(_drag);
                _drag = null;
                _id = null;
            }
        }

        private bool IsPointerInGroup(Point point)
        {
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(point, SessionController.Instance.SessionView);
            var result = hits.Where((uiElem) => uiElem is GroupNodeDataGridView);
            return result.Any();
        }
        private FrameworkElement _el;

        private async void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs args)
        {
            //BitmapImage bmp = new BitmapImage(new Uri("ms-appx:///Assets//icon_additional.png"));

            //var img = new Image
            //{
            //    RenderTransform = new CompositeTransform(),
            //    Source = bmp
            //};

            //Canvas.SetLeft(img, 0);
            //Canvas.SetTop(img, 0);

            //var vm = (GroupNodeDataGridViewModel) DataContext;
            //var t = (CompositeTransform)img.RenderTransform;
            //t.TranslateX = vm.Transform.TranslateX
            //t.TranslateY = point.Y
            //Canvas.SetLeft(img, args.Position.X);
            //Canvas.SetTop(img, args.Position.Y);
            //SessionController.Instance.SessionView.MainCanvas.Children.Add(img);
        }


        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {

            if (_drag != null)
            {
                var x = Canvas.GetLeft(_drag);
                var y = Canvas.GetTop(_drag);
                x += args.Delta.Translation.X;
                y += args.Delta.Translation.Y;

                Canvas.SetLeft(_drag, x);
                Canvas.SetTop(_drag, y);
            }
         }
        //private bool IsPointerInGroup(object sender, ManipulationDeltaRoutedEventArgs args)
        //{

        //    var point = ((UIElement)sender).TransformToVisual(SessionController.Instance.SessionView.MainCanvas);
        //    var hits = VisualTreeHelper.FindElementsInHostCoordinates(this.GetRealCoordinatesOnScreen(sender), SessionController.Instance.SessionView);
        //    var result = hits.Where((uiElem) => uiElem is AreaNodeView);
        //    return result.Any();
        //}

        //private void StartTimer()
        //{
        //    if (_timer == null)
        //    {
        //        _timer = new DispatcherTimer();
        //        _timer.Tick += async delegate (object o, object o1)
        //        {


        //        };
        //        _timer.Interval = TimeSpan.FromMilliseconds(50);
        //        _timer.Start();

        //    }
        //}

        //private Point GetRealCoordinatesOnScreen(object sender)
        //{
        //    var cview = (AreaNodeView)this.Parent;
        //    var vm = (AreaNodeViewModel)cview.DataContext;
        //    var send = (FrameworkElement)sender;
        //    var sendVm = (ElementViewModel)send.DataContext;
        //    var model = sendVm.Model;
        //    var groupModel = vm.Model;

        //    var point = vm.CompositeTransform.TransformPoint(new Point(model.X, model.Y));
        //    var x = point.X + groupModel.X;
        //    var y = point.Y + groupModel.Y;
        //    var point2 = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.TransformPoint(new Point(x, y));
        //    return point2;

        //}


    }
}
