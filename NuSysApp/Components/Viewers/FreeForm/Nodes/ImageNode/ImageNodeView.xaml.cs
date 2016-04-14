using System;
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
            TempRegion.Fill = new SolidColorBrush(Colors.Red);

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
                Rectangle region = new Rectangle();
                region.Height = TempRegion.Height;
                region.Width = TempRegion.Width;
                region.Fill = new SolidColorBrush(Colors.Blue);
                Canvas.SetLeft(region, Canvas.GetLeft(TempRegion));
                Canvas.SetTop(region, Canvas.GetTop(TempRegion));
                (DataContext as ImageElementViewModel).Model.Regions.Add(region);
                _drawingRegion = false;
                this.AddRegionsToCanvas();
            }
        }

        private void AddRegionsToCanvas()
        {
            Canvas.Children.Clear();
            foreach (var element in (DataContext as ImageElementViewModel).Model.Regions)
            {
                Canvas.Children.Add(element);
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
