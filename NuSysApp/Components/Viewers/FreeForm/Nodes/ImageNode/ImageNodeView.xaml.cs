using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ImageNodeView : AnimatableUserControl, IThumbnailable, Sizeable
    {

        private Boolean _drawingRegion;
        private Rectangle TempRegion;
        private ImageElementViewModel _vm;


        public ImageNodeView(ImageElementViewModel vm)
        {
            _vm = vm;
            _vm.View = this;
           
            InitializeComponent();
            DataContext = vm;
            _drawingRegion = false;
            TempRegion = new Rectangle();
            TempRegion.Fill = new SolidColorBrush(Colors.Transparent);
            TempRegion.StrokeThickness = 2;
            TempRegion.Stroke = new SolidColorBrush(Colors.Red);
            

            // vm.Controller.SizeChanged += Controller_SizeChanged;
           // vm.Controller.ContainerSizeChanged += Controller_SizeChanged;
    
            vm.PropertyChanged +=VmOnPropertyChanged;

            Loaded += ViewLoaded;
            //XamlRenderingBackgroundTask x = new RenderTask(this.xImage);

            vm.Controller.Disposed += ControllerOnDisposed;
            SizeChanged += ImageNodeView_SizeChanged;
        }

        private void ViewLoaded(object sender, RoutedEventArgs e)
        {
            _vm.CreateRegionViews();
        }

        private void ImageNodeView_SizeChanged(object sender, SizeChangedEventArgs e)
        {

            var vm = DataContext as ImageElementViewModel;

            if (vm == null)
                return;
            vm.SizeChanged(this, xImage.ActualWidth, xImage.ActualHeight);
        }

        public async Task onGoTo(Region region)
        {
            foreach (var reg in _vm.Regions)
            {
                if ((reg.DataContext as ImageRegionViewModel).Model.Id == region.Id)
                {
                    reg.Select();
                }
            }
        }


        private void VmOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Width" || e.PropertyName == "Height")
            {
                ObservableCollection<RectangleView> list1 = _vm.RegionsListTest;

                foreach (var rectangle in list1)
                {
                    rectangle.setRectangleSize(_vm.Width, _vm.Height);
                }
            }
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            ObservableCollection<RectangleView> list1 = _vm.RegionsListTest;

            foreach (var rectangle in list1)
            {
                rectangle.setRectangleSize(_vm.Width, _vm.Height);
            }
        }

        private void ControllerOnDisposed(object source, object args)
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
                var width = _vm.Width;
                var height = _vm.Height;

                var leftRatio = Canvas.GetLeft(TempRegion)/width;
                var topRatio = Canvas.GetTop(TempRegion)/height;
                var widthRatio = TempRegion.Width/width;
                var heightRatio = TempRegion.Height/height;

                //create dictionary
                Dictionary<string, double> attributes = new Dictionary<string, double>();
                attributes.Add("nodeWidth", width);
                attributes.Add("nodeHeight", height);
                attributes.Add("widthRatio", widthRatio);
                attributes.Add("heightRatio", heightRatio);
                attributes.Add("leftRatio", leftRatio);
                attributes.Add("topRatio", topRatio);

                RectangleViewModel rvm = new RectangleViewModel(new RectangleModel(), attributes);
                RectangleView rv = new RectangleView(rvm); 

                // add to controller
                _vm.Controller.SetRegionModel(rvm);
                _vm.RegionsListTest.Add(rv);

                // works?
                Canvas.Children.Remove(TempRegion);
                TempRegion.Height = 0;
                TempRegion.Width = 0;

                _drawingRegion = false;
            }
        }

        private void XImage_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        {
            var uiSender = sender as UIElement;
            if (e.GetCurrentPoint(uiSender).Properties.IsLeftButtonPressed && _drawingRegion)
            {
                var point = e.GetCurrentPoint(uiSender);
                var top = Canvas.GetTop(TempRegion);
                var left = Canvas.GetLeft(TempRegion);
                TempRegion.Height = Math.Max(point.Position.Y - top, 0);
                TempRegion.Width = Math.Max(point.Position.X - left, 0);
            }
        }

        private void XImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_drawingRegion)
            {
                XImage_OnPointerReleased(sender, e);
            }
        }

        public double GetWidth()
        {
            return xImage.ActualWidth;
        }

        public double GetHeight()
        {
            return xImage.ActualHeight;
        }

        public double GetViewWidth()
        {
            throw new NotImplementedException();
        }

        public double GetViewHeight()
        {
            throw new NotImplementedException();
        }
    }
}
