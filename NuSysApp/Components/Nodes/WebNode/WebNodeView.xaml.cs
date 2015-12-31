using System;
using System.ComponentModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class WebNodeView : AnimatableNodeView
    {
        public WebNodeView(WebNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            xUrlBox.ManipulationMode = ManipulationModes.All;
            xWebView.RenderTransform = new CompositeTransform
            {
                ScaleX = vm.Zoom,
                ScaleY = vm.Zoom
            };

            (vm.Model as WebNodeModel).UrlChanged += OnUrlChanged;

            Loaded += delegate(object sender, RoutedEventArgs args)
            {
              

                (vm.Model as WebNodeModel).Url = "http://www.google.com";

                xUrlBox.ManipulationDelta += delegate(object o, ManipulationDeltaRoutedEventArgs eventArgs)
                {
                   // xUrlBox.CancelDirectManipulations();
                    eventArgs.Handled = true;
                };
            };

            vm.PropertyChanged += OnPropertyChanged;
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var vm = (WebNodeViewModel) DataContext;
            //   await xWebView.InvokeScriptAsync("eval", new string[] { "ZoomFunction(" + vm.Zoom + ");" });

            var c = xWebView.RenderTransform as CompositeTransform;
            c.ScaleX = vm.Zoom;
            c.ScaleY = vm.Zoom;
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
