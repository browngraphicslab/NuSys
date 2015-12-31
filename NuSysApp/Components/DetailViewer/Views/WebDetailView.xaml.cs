using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
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
    public sealed partial class WebDetailView : UserControl
    {
        public WebDetailView(WebNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                var sw = SessionController.Instance.SessionView.ActualWidth/1.5;
                var sh = SessionController.Instance.SessionView.ActualHeight/2;

                var ratio = xWebView.Width > xWebView.Height ? xWebView.Width/sw : xWebView.Height/sh;
                xWebView.Width = xWebView.Width/ratio;
                //xWebView.Height = xWebView.Height/ratio;
                xBorder.Width = xWebView.Width + 5;
                xBorder.Height = xWebView.Height / ratio + 5;
                xScrollViewer.Width = xWebView.Width / ratio;
                xScrollViewer.Height = xWebView.Height / ratio;

                (vm.Model as WebNodeModel).Url = "http://www.google.com";
            };

            (vm.Model as WebNodeModel).UrlChanged += OnUrlChanged;
        }

        private void OnUrlChanged(object source, string url)
        {
            xWebView.Navigate(new Uri(url));
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                var vm = (WebNodeViewModel)DataContext;
                (vm.Model as WebNodeModel).Url = xUrlBox.Text;
            }
        }
    }
}
