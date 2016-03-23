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
        private String  _url ;
        public WebNodeView(WebNodeViewModel vm)
        {
            InitializeComponent();
            Canvas.SetZIndex(xWebView, -10);
            DataContext = vm;
            xUrlBox.ManipulationMode = ManipulationModes.All;
            xWebView.RenderTransform = new CompositeTransform
            {
                ScaleX = vm.Zoom,
                ScaleY = vm.Zoom
            };


            Loaded += delegate(object sender, RoutedEventArgs args)
            {
                String url = vm.Controller.LibraryElementModel.Data;
                url = url ?? "http://www.google.com";
                vm.Controller.LibraryElementModel?.SetContentData(vm, url);

                xUrlBox.ManipulationDelta += delegate(object o, ManipulationDeltaRoutedEventArgs eventArgs)
                {
                    eventArgs.Handled = true;
                };
            };


            vm.Controller.LibraryElementModel.OnContentChanged += delegate (ElementViewModel originalSenderViewModel)
            {
                var url = vm.Controller.LibraryElementModel.Data;
                OnUrlChanged(url);
            };


            vm.PropertyChanged += OnPropertyChanged;
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            vm.Controller.RequestDelete();
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var vm = (WebNodeViewModel) DataContext;
            //   await xWebView.InvokeScriptAsync("eval", new string[] { "ZoomFunction(" + vm.Zoom + ");" });
            vm.Url = _url;
            var c = xWebView.RenderTransform as CompositeTransform;
            c.ScaleX = vm.Zoom;
            c.ScaleY = vm.Zoom;
        }

        private void OnUrlChanged(string url)
        {
            url = url ?? "http://www.google.com";
            xWebView.Navigate(new Uri(url));
        }

        private void OnKeyUp(object sender, KeyRoutedEventArgs e)
        {
          if (e.Key == VirtualKey.Enter)
            {
                var vm = (WebNodeViewModel)DataContext;

                //(vm.Model as WebNodeModel).Url = xUrlBox.Text;
                var url  = this.checkIfUrlRight(xUrlBox.Text);
                vm.Controller.LibraryElementModel?.SetContentData(vm, url);

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
