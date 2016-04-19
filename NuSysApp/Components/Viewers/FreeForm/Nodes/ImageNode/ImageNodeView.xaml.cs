using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NAudio.Wave;
using NuSysApp.Util;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : AnimatableUserControl, IThumbnailable
    {

        private Boolean _drawingRegion;
        private Rectangle TempRegion;
        public ImageNodeView(ImageElementViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _drawingRegion = false;
            TempRegion = new Rectangle();
            TempRegion.Fill = new SolidColorBrush(Colors.Transparent);
            TempRegion.StrokeThickness = 2;
            TempRegion.Stroke = new SolidColorBrush(Colors.Red);

            vm.Controller.SizeChanged += Controller_SizeChanged;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                //nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);

                //vm.Init();
                //lets see if this 2 way binding works
                //nodeTpl.inkCanvas.ViewModel.CanvasSize = new Size(vm.Width, vm.Height);
                //nodeTpl.inkCanvas.Width = vm.Width;
                //nodeTpl.inkCanvas.Height = vm.Height;

            };
            //XamlRenderingBackgroundTask x = new RenderTask(this.xImage);

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            Debug.WriteLine("sized changing!!!");
            ObservableCollection<Rectangle> list = (DataContext as ImageElementViewModel).RegionsList;

            foreach (var rectangle in list)
            {
                RectanglePoints rectPoint; 
                (DataContext as ImageElementViewModel).rectToPoints.TryGetValue(rectangle, out rectPoint);

                var leftRatio = rectPoint.getLeftRatio();
                var topRatio = rectPoint.getTopRatio();
                var widthRatio = rectPoint.getWidthRatio();
                var heightRatio = rectPoint.getHeightRatio();
                
                Canvas.SetLeft(rectangle, width * leftRatio);
                Canvas.SetTop(rectangle, height * topRatio);
                rectangle.Width = width * widthRatio;
                rectangle.Height = height * heightRatio;
            }
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (ImageElementViewModel) DataContext;
            vm.Controller.Disposed -= ControllerOnDisposed;
            nodeTpl.Dispose();
            DataContext = null;
        }

        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            //nodeTpl.ToggleInkMode();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDelete();
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)DataContext;
            vm.Controller.RequestDuplicate(vm.Model.X, vm.Model.Y);
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xImage, width, height);
            return r;
        }


        private void Region_OnClick(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("fdsafd");
            _drawingRegion = true;
        }

        private void XImage_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_drawingRegion)
            {
                Debug.WriteLine("here");
                Canvas.Children.Add(TempRegion);
                Canvas.SetLeft(TempRegion, e.GetCurrentPoint((UIElement) sender).Position.X);
                Canvas.SetTop(TempRegion, e.GetCurrentPoint((UIElement)sender).Position.Y);
                TempRegion.Opacity = 1;
            }
        }

        private void XImage_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_drawingRegion)
            {

                //add rectangle to model list
                //remove temp rectangle
                //have another method that reads all things from model and adds it.

                ImageElementViewModel vm = (ImageElementViewModel) DataContext;

                var width = vm.Model.Width;
                var height = vm.Model.Height;

                var leftRatio = Canvas.GetLeft(TempRegion)/width;
                var topRatio = Canvas.GetTop(TempRegion)/height;

                var widthRatio = TempRegion.Width/width;
                var heightRatio = TempRegion.Height/Height;

                RectanglePoints rectangle = new RectanglePoints(leftRatio, topRatio, widthRatio, heightRatio);

                // add to controller
                (DataContext as ImageElementViewModel).Controller.SetRegion(rectangle);
                Rectangle rect = rectangle.getRectangle();

                rect.Width = width*rectangle.getWidthRatio();
                rect.Height = height*rectangle.getHeightRatio();
                Canvas.Children.Add(rect);
                Canvas.SetLeft(rect, rectangle.getLeftRatio() * width);
                Canvas.SetTop(rect, rectangle.getTopRatio() * height);

                // works?
                Canvas.Children.Remove(TempRegion);

                //(DataContext as ImageElementViewModel).RegionsList.Add(rect);
                //(DataContext as ImageElementViewModel).Model.Regions.Add(rectangle);

                _drawingRegion = false;
                //this.AddRegionsToCanvas();
            }
        }

        private void XImage_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed && _drawingRegion)
            {
                TempRegion.Height = e.GetCurrentPoint((UIElement)sender).Position.Y - Canvas.GetTop(TempRegion);
                TempRegion.Width = e.GetCurrentPoint((UIElement)sender).Position.X - Canvas.GetLeft(TempRegion);
            }
        }
    }
}
