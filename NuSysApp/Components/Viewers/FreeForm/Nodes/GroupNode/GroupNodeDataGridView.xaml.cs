using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

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
            DataGrid.ManipulationMode = ManipulationModes.All;

        }

        private void OnPointerPressed(object source, PointerRoutedEventArgs args)
        {
            var src = (FrameworkElement) args.OriginalSource;
            if (src.DataContext is GroupNodeDataGridInfo)
            {
                var dc = (GroupNodeDataGridInfo) src.DataContext;
                _el = src;
                int a = 0;
                var b = Canvas.GetLeft(_el);
                var p = (FrameworkElement)VisualTreeHelper.GetParent(_el);
                var p1 =(FrameworkElement)VisualTreeHelper.GetParent(p);
                var p2 = (FrameworkElement)VisualTreeHelper.GetParent(p1);
                var pw = (FrameworkElement)VisualTreeHelper.GetParent(p2);
                var pw2 = (FrameworkElement)VisualTreeHelper.GetParent(pw);
                var c = Canvas.GetLeft(p2);
                var d = 0;
                //SessionController.Instance.IdToControllers[dc.Id].RequestMoveToCollection()
                // get Id here
            }
        }

        private FrameworkElement _el;

        private async void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs aargs)
        {
            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            var img = new Image
            {
                RenderTransform = new CompositeTransform(),
                Source = bmp
            };
            var vm = (GroupNodeDataGridViewModel) DataContext;
            var t = (CompositeTransform)img.RenderTransform;
           //t.TranslateX = vm.Transform.TranslateX
            //t.TranslateY = point.Y
            //Canvas.SetLeft(img, point.X);
            //Canvas.SetTop(img, point.Y);
            //SessionController.Instance.SessionView.MainCanvas.Children.Add(img);
        }


        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {

            int a = 0;
            //if (!IsPointerInGroup(sender, args))
            //{
            //    this.StartTimer(sender, args);
            //}
            //else
            //{
            //    //this.StopTimer(sender, args);
            //}





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
