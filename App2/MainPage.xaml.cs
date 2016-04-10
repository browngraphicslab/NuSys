using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.UI.Xaml;
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

        public MainPage()
        {
            this.InitializeComponent();

            var analyticsInfo = Windows.System.Profile.AnalyticsInfo.VersionInfo;
            // get the device manufacturer and model name
            EasClientDeviceInformation eas = new EasClientDeviceInformation();
            var DeviceManufacturer = eas.SystemManufacturer;
            var DeviceModel = eas.SystemProductName;

            xToggleInk.Click += delegate(object sender, RoutedEventArgs args)
            {
                inqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Ink;
            };


            xSelect.Click += delegate (object sender, RoutedEventArgs args)
            {
                inqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Disabled;
            };

            xErase.Click += delegate (object sender, RoutedEventArgs args)
            {
                inqCanvas.Mode = PhilInqCanvas.InqCanvasMode.Erase;
            };



            SizeChanged += delegate(object sender, SizeChangedEventArgs args)
            {
                inqCanvas.Width = args.NewSize.Width;
                inqCanvas.Height = args.NewSize.Height;
            };

            xAtomCanvas.RenderTransform = new CompositeTransform
            {
                CenterX = -50000,
                CenterY = -50000,
                ScaleX = 1,
                ScaleY = 1,
                TranslateX = -50000,
                TranslateY = -50000
            };

            inqCanvas.Transform = (CompositeTransform)xAtomCanvas.RenderTransform;
           
            ManipulationMode = ManipulationModes.All;
            ManipulationDelta += OnManipulationDelta;
            ManipulationStarting += OnManipulationStarting;
            ManipulationCompleted += OnManipulationCompleted;
            PointerWheelChanged += OnPointerWheelChanged;

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(5);
            _timer.Tick += delegate(object sender, object o)
            {
                _timer.Stop();
                inqCanvas.Transform = (CompositeTransform) xAtomCanvas.RenderTransform;
                inqCanvas.Invalidate(true);
                _timer.Start();
            };

            
        }

        private async void OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {
            _timer.Stop();
            inqCanvas.Invalidate(true);

    
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

            inqCanvas.Transform = (CompositeTransform)xAtomCanvas.RenderTransform;
            inqCanvas.Invalidate(true);
        }

        protected void OnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs e)
        {
            _timer.Start();
            e.Container = this;
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

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
