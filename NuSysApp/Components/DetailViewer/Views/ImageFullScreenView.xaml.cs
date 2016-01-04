using System;
using System.Collections.Generic;
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
    public sealed partial class ImageFullScreenView : UserControl
    {
        private InqCanvasView _inqCanvasView;

        public ImageFullScreenView(ImageNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;


            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                var sw = SessionController.Instance.SessionView.ActualWidth /2 ;
                var sh = SessionController.Instance.SessionView.ActualHeight /2;

                var ratio = xImg.ActualWidth > xImg.ActualHeight ? xImg.ActualWidth/sw : xImg.ActualHeight/sh;
                xImg.Width = xImg.ActualWidth/ratio;
                xImg.Height = xImg.ActualHeight/ratio;
                //  xBorder.Width = xImg.Width + 5;
                // xBorder.Height = xImg.Height +5;

                _inqCanvasView = new InqCanvasView(new InqCanvasViewModel((vm.Model as NodeModel).InqCanvas, new Size(xImg.Width, xImg.Height)));
                xWrapper.Children.Add(_inqCanvasView);
                _inqCanvasView.IsEnabled = true;
                _inqCanvasView.Background = new SolidColorBrush(Colors.Aqua);
                _inqCanvasView.Width = xImg.Width;
                _inqCanvasView.Height = xImg.Height;
         

            };
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
