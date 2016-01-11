using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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

            Loaded += async delegate(object sender, RoutedEventArgs args)
            {

                vm.PropertyChanged += delegate(object o, PropertyChangedEventArgs eventArgs)
                {
                    if (eventArgs.PropertyName != "ImageSource")
                        return;

                    var sw = SessionController.Instance.SessionView.ActualWidth / 1;
                    var sh = SessionController.Instance.SessionView.ActualHeight / 1;
                    var ratio = vm.Width > vm.Height ? vm.Width / sw : vm.Height/ sh;
                    xImg.Width = vm.Width / ratio;
                    xImg.Height = vm.Height / ratio;
                    xBorder.Width = xImg.Width + 5;
                    xBorder.Height = xImg.Height + 5;
                    
                };


                await vm.InitPdfViewer();


                _inqCanvasView = new InqCanvasView(new InqCanvasViewModel((vm.Model as NodeModel).InqCanvas, new Size(xImg.Width, xImg.Height)));
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

            };
        }

        private async void OnPageLeftClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            await vm.FlipLeft();
            (_inqCanvasView.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
            //  nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //  nodeTpl.inkCanvas.ReRenderLines();

        }

        private async void OnPageRightClick(object sender, RoutedEventArgs e)
        {
            var vm = (PdfNodeViewModel)this.DataContext;
            await vm.FlipRight();
            (_inqCanvasView.DataContext as InqCanvasViewModel).Model.Page = vm.CurrentPageNumber;
           // (_inqCanvasView.DataContext as InqCanvasViewModel).Lines.Clear();
            //   nodeTpl.inkCanvas.ViewModel.Model.Lines = vm.RenderedLines;
            //   nodeTpl.inkCanvas.ReRenderLines();
        }
        
    }
}
