using System;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class WebNodeView : AnimatableUserControl
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

                var model = (vm.Model as WebNodeModel);
                model.Url = model.Url == "" ? "http://www.google.com" : model.Url;

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
            var vm = (WebNodeViewModel)DataContext;
           
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
          if (e.Key == VirtualKey.Enter)
            {
                var vm = (WebNodeViewModel)DataContext;

                //(vm.Model as WebNodeModel).Url = xUrlBox.Text;
                (vm.Model as WebNodeModel).Url = this.checkIfUrlRight(xUrlBox.Text);

            }
        }
        private string checkIfUrlRight(string s)
        {
            string url = null;
            string searchterms = null;

            if (Uri.IsWellFormedUriString(s, UriKind.Absolute))
            {
                url = s;
                return url;
            }
            else
            {
                if (s.StartsWith("http://") == false && s.EndsWith(".com") == true)
                {
                    if (!s.Contains("www."))
                    {
                        s = "http://www." + s;
                        url = s;
                    }
                    else
                    {
                        s = "http://" + s;
                        url = s;
                    }

                    return url;
                }
                else if (!s.EndsWith(".com"))
                {
                    List<string> terms = new List<string>();
                    string[] separators = new string[] { ",", ".", "!", "\'", " ", "\'s" };
                    foreach (string word in s.Split(separators, StringSplitOptions.RemoveEmptyEntries))
                        terms.Add(word);
                    foreach (string word in terms)
                        searchterms = searchterms + word + "+";
                    if (!searchterms.EndsWith("+"))
                    {
                        searchterms = searchterms.Remove(-1);
                    }
                    url = "http://www.google.com/search?q=" + searchterms;
                    return url;
                }
                else
                {
                    url = s;
                    return url;
                }
            }
            
               
        }
        
        private void XWebView_OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            string url = sender.Source.AbsoluteUri;
            xUrlBox.Text = url;
        }


        private void XWebView_OnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            string url = sender.Source.AbsoluteUri;
            xUrlBox.Text = url;
        }
    }
}
