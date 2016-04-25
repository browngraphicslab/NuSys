using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class PdfDetailView : UserControl
    {
        private InqCanvasView _inqCanvasView;

        public PdfDetailView(PdfNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            
          //  xImg.ManipulationMode = ManipulationModes.All;
          //  xImg.ManipulationDelta += OnManipulationDelta;
            
            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                _inqCanvasView = new InqCanvasView(new InqCanvasViewModel(vm.Model.InqCanvas, new Size(xImg.Width, xImg.Height)));
                xWrapper.Children.Insert(1, _inqCanvasView);
                _inqCanvasView.IsEnabled = true;
                _inqCanvasView.HorizontalAlignment = HorizontalAlignment.Left;
                _inqCanvasView.VerticalAlignment = VerticalAlignment.Top;
                _inqCanvasView.Background = new SolidColorBrush(Colors.Aqua);
                _inqCanvasView.Width = xImg.Width;
                _inqCanvasView.Height = xImg.Height;

                (_inqCanvasView.DataContext as InqCanvasViewModel).CanvasSize = new Size(xImg.Width, xImg.Height);

                _inqCanvasView.Clip = new RectangleGeometry
                {
                    Rect = new Rect { X = 0, Y = 0, Width = _inqCanvasView.Width, Height = _inqCanvasView.Height }
                };

                xBorder.SizeChanged += XBorderOnSizeChanged;


            };

            vm.MakeTagList();

            vm.Controller.Disposed += ControllerOnDisposed;
        }

        private void XBorderOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            xBorder.Clip = new RectangleGeometry {Rect= new Rect(0,0,e.NewSize.Width, e.NewSize.Height)};
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (PdfNodeViewModel)DataContext;
            vm.Controller.Disposed += ControllerOnDisposed;
            DataContext = null;
        }


        private async void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipLeft();
            (_inqCanvasView.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            //  nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //  nodeTpl.inkCanvas.ReRenderLines();

        }

        private async void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            if (vm == null)
                return;
            await vm.FlipRight();
            (_inqCanvasView.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            // (_inqCanvasView.DataContext as InqCanvasViewModel).Lines.Clear();
            //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //   nodeTpl.inkCanvas.ReRenderLines();
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (PdfNodeModel)((PdfNodeViewModel)DataContext).Model;
            string token = model.GetMetaData("Token")?.ToString();
            await AccessList.OpenFile(token);
        }

        protected void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (e.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Pen)
                return;

            var compositeTransform = (CompositeTransform)xImg.RenderTransform;

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

            var minY = 0;
            var maxY = Math.Max(xBorder.ActualHeight - xImg.ActualHeight*compositeTransform.ScaleY, 0);
            compositeTransform.TranslateY = Math.Max(compositeTransform.TranslateY, minY);

            e.Handled = true;

        }
    }
}