using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Security.ExchangeActiveSyncProvisioning;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace App2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DispatcherTimer _timer;
        private WetDryInkCanvas _wetDryCanvas;

        public MainPage()
        {
            this.InitializeComponent();

            Clip = new RectangleGeometry { Rect = new Rect { X = 0, Y = 0, Width = 1000, Height = 1000 } };
            var analyticsInfo = Windows.System.Profile.AnalyticsInfo.VersionInfo;
  
            EasClientDeviceInformation eas = new EasClientDeviceInformation();
            var DeviceManufacturer = eas.SystemManufacturer;
            var DeviceModel = eas.SystemProductName;

            Loaded += OnLoaded;            

            xAtomCanvas.RenderTransform = new CompositeTransform
            {
                CenterX = -50000,
                CenterY = -50000,
                ScaleX = 1,
                ScaleY = 1,
                TranslateX = -50000,
                TranslateY = -50000
            };
           
            ManipulationMode = ManipulationModes.All;
            ManipulationDelta += OnManipulationDelta;
            ManipulationStarted += OnManipulationStarted;
            ManipulationCompleted += OnManipulationCompleted;
            PointerWheelChanged += OnPointerWheelChanged;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(10);
            _timer.Tick += delegate(object sender, object o)
            {
                _timer.Stop();
                _wetDryCanvas.Redraw(); 
                _timer.Start();
            };            
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _wetDryCanvas = new WetDryInkCanvas(wetCanvas, dryCanvas);
            _wetDryCanvas.Transform = (CompositeTransform)xAtomCanvas.RenderTransform;
        }

        private async void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {
            if (manipulationCompletedRoutedEventArgs.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;
            _timer.Stop();

            _wetDryCanvas.Redraw();  
        }

        protected void OnPointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            //var vm = (FreeFormViewerViewModel)_view.DataContext;
            var compositeTransform = (CompositeTransform)xAtomCanvas.RenderTransform;

            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var mousePoint = e.GetCurrentPoint(null).Position;

            var cent = compositeTransform.Inverse.TransformPoint(mousePoint);

            var localPoint = tmpTranslate.Inverse.TransformPoint(cent);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - cent.X,
                worldPoint.Y - cent.Y);

            //...amd balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;
            var direction = Math.Sign((double)e.GetCurrentPoint(null).Properties.MouseWheelDelta);

            var zoomspeed = direction < 0 ? 0.95 : 1.05;//0.08 * direction;
            var translateSpeed = 10;

            var center = compositeTransform.Inverse.TransformPoint(e.GetCurrentPoint(null).Position);
            compositeTransform.ScaleX *= zoomspeed;
            compositeTransform.ScaleY *= zoomspeed;

            compositeTransform.CenterX = cent.X;
            compositeTransform.CenterY = cent.Y;

            //    inqCanvas.Transform = (CompositeTransform)xAtomCanvas.RenderTransform;
            //    inqCanvas.Invalidate(true);
            _wetDryCanvas.Redraw();
        }

        protected void OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            _timer.Start();
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            var compositeTransform = (CompositeTransform)xAtomCanvas.RenderTransform;

            var tmpTranslate = new TranslateTransform
            {
                X = compositeTransform.CenterX,
                Y = compositeTransform.CenterY
            };

            var center = compositeTransform.Inverse.TransformPoint(e.Position);

            var localPoint = tmpTranslate.Inverse.TransformPoint(center);

            //Now scale the point in local space
            localPoint.X *= compositeTransform.ScaleX;
            localPoint.Y *= compositeTransform.ScaleY;

            //Transform local space into world space again
            var worldPoint = tmpTranslate.TransformPoint(localPoint);

            //Take the actual scaling...
            var distance = new Point(
                worldPoint.X - center.X,
                worldPoint.Y - center.Y);

            //...and balance the jump of the changed scaling origin by changing the translation            

            compositeTransform.TranslateX += distance.X;
            compositeTransform.TranslateY += distance.Y;

            //Also set the scaling values themselves, especially set the new scale center...
            compositeTransform.ScaleX *= e.Delta.Scale;
            compositeTransform.ScaleY *= e.Delta.Scale;

            compositeTransform.CenterX = center.X;
            compositeTransform.CenterY = center.Y;

            //And consider a translational shift
            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;
        }
    }
}
