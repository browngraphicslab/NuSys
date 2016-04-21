using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components.Viewers.FreeForm;
using NuSysApp.Util;
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfNodeView : AnimatableUserControl, IThumbnailable
    {
        private Boolean _drawingRegion;
        private Rectangle TempRegion;
        private PdfNodeViewModel _vm;

        public PdfNodeView(PdfNodeViewModel vm)
        {
            _vm = vm;
            InitializeComponent();
            //  IsDoubleTapEnabled = true;
            DataContext = vm;

            _drawingRegion = false;
            TempRegion = new Rectangle();
            TempRegion.Fill = new SolidColorBrush(Colors.Transparent);
            TempRegion.StrokeThickness = 2;
            TempRegion.Stroke = new SolidColorBrush(Colors.Red);

            //vm.Controller.SizeChanged += Controller_SizeChanged;
            vm.PropertyChanged += VmOnPropertyChanged;

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            ObservableCollection<RectangleView> list1 = _vm.RegionsListTest;

            Debug.WriteLine("width:" + _vm.Width + " height: " + _vm.Height);

            foreach (var rectangle in list1)
            {
                rectangle.setRectangleSize(_vm.Width, _vm.Height);
            }
        }

        private void VmOnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
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

        private void ControllerOnDisposed(object source)
        {
            var vm = (PdfNodeViewModel) DataContext;
            nodeTpl.Dispose();
            vm.Controller.Disposed -= ControllerOnDisposed;
        }


        private void OnEditInk(object sender, RoutedEventArgs e)
        {
            //  nodeTpl.ToggleInkMode();

        }

        private async void OnPageLeftClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel) this.DataContext;
            await vm.FlipLeft();
            //(nodeTpl.inkCanvas.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            e.Handled = true;

            // nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
//nodeTpl.inkCanvas.ReRenderLines();

        }

        private async void OnPageRightClick(object sender, TappedRoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel) this.DataContext;
            await vm.FlipRight();
            //(nodeTpl.inkCanvas.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            e.Handled = true;

            //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //     nodeTpl.inkCanvas.ReRenderLines();
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel) this.DataContext;
            vm.Controller.RequestDelete();
        }

        private void OnDuplicateClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel) DataContext;
            vm.Controller.RequestDuplicate(vm.Model.X, vm.Model.Y);
        }

        private void PageRight_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(xRenderedPdf, width, height);
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
                Canvas.SetLeft(TempRegion, e.GetCurrentPoint((UIElement)sender).Position.X);
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

                var leftRatio = Canvas.GetLeft(TempRegion) / width;
                var topRatio = Canvas.GetTop(TempRegion) / height;
                var widthRatio = TempRegion.Width / width;
                var heightRatio = TempRegion.Height / height;

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
            if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed && _drawingRegion)
            {
                TempRegion.Height = e.GetCurrentPoint((UIElement)sender).Position.Y - Canvas.GetTop(TempRegion);
                TempRegion.Width = e.GetCurrentPoint((UIElement)sender).Position.X - Canvas.GetLeft(TempRegion);
            }
        }
    }


}
