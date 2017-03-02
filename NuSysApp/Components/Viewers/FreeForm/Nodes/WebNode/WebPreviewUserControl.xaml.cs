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
        
        /// <summary>
        /// Navigates to the url string
        /// </summary>
        /// <param name="urlString"></param>
        public void Navigate(String urlString)
        {
            Uri uri;
            try
            {
                uri = new Uri(urlString);
            }
            catch
            {
                uri = new Uri("http://google.com");
            }
            xWebView.Navigate(uri);
        }

        /// <summary>
        /// Moves the web node preview when the top bar is dragged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XTopBar_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            Canvas.SetLeft(this, Canvas.GetLeft(this) + e.Delta.Translation.X);
            Canvas.SetTop(this, Canvas.GetTop(this) + e.Delta.Translation.Y);
        }

        /// <summary>
        /// Resizes the webnode
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XResizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var newHeight = Height + e.Delta.Translation.Y;
            var newWidth = Width + e.Delta.Translation.X;
            Height = newHeight;
            Width = newWidth;
            e.Handled = true;
            
        }

        /// <summary>
        /// This is so that you cant click through the web page and interact with whats behind it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XGrid_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CapturePointer(e.Pointer);
            e.Handled = true;
        }

        /// <summary>
        /// This is so that you cant click through the web page and interact with whats behind it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XGrid_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            ReleasePointerCapture(e.Pointer);
            e.Handled = true;
        }

        /// <summary>
        /// When the close button is tapped, remove this from its parents children.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XCloseButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            //((Canvas)this.Parent)?.Children?.Remove(this);
            Visibility = Visibility.Collapsed;
        }


        /// <summary>
        /// Opens the actual browser to the same url as the nusys browser
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XOpenBrowserButton_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            Windows.System.Launcher.LaunchUriAsync(xWebView.Source);
        }
    }
}
