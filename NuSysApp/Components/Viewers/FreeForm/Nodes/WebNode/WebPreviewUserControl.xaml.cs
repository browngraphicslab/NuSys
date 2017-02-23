using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class WebPreviewUserControl : UserControl
    {
        public WebPreviewUserControl()
        {
            this.InitializeComponent();
            
        }
        

        public void Navigate(String urlString)
        {
            Uri uri = new Uri(urlString);
            xWebView.Navigate(uri);
        }

        private void XTopBar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.Delta.Translation.X);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.Delta.Translation.Y);
        }

        private void XResizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var newHeight = Height + e.Delta.Translation.Y;
            var newWidth = Width + e.Delta.Translation.X;
            Height = newHeight;
            Width = newWidth;
            e.Handled = true;
            
        }

        private void XGrid_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CapturePointer(e.Pointer);
            e.Handled = true;
        }

        private void XGrid_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        private void XCloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
