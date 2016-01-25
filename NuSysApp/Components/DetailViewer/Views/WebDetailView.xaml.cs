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
                //xWebView.Width = (xWebView.Width/ratio);
                xWebView.Height = (xWebView.Height/ratio);
                //xWebView.Height = xWebView.Height/ratio;
                xBorder.Width = xWebView.Width + 5;
                xBorder.Height = xWebView.Height + 5;
                xScrollViewer.Width = xWebView.Width;
                xScrollViewer.Height = xWebView.Height;

                //(vm.Model as WebNodeModel).Url = "http://www.google.com";
                (vm.Model as WebNodeModel).Url = vm.Url;
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
                //(vm.Model as WebNodeModel).Url = xUrlBox.Text;
                xWebView.Navigate(new Uri(this.checkIfUrlRight(xUrlBox.Text)));
                //(vm.Model as WebNodeModel).Url = this.checkIfUrlRight(xUrlBox.Text);
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
            if (((DataContext as WebNodeViewModel).Model as WebNodeModel).Url != url)
            {
                (DataContext as WebNodeViewModel).Url = url;
                (DataContext as WebNodeViewModel).History.Add(new WebNodeModel.Webpage(url, GetTimestamp(DateTime.Now)));
                Back.IsEnabled = true;

                var message = new Message();
                message["url"] = url;
                message["id"] = (DataContext as WebNodeViewModel).Id;
                var request = new SendableUpdateRequest(message);
                SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
            }

        }

        public static String GetTimestamp(DateTime value)
        {
            return value.ToString("yyyyMMddHHmmss");
        }

        private void Back_OnClick(object sender, RoutedEventArgs e)
        {
            if (xWebView.CanGoBack == true)
            {
                xWebView.GoBack();
                Forward.IsEnabled = true;
            }
            else
            {
                Back.IsEnabled = false;
            }
        }

        private void Forward_OnClick(object sender, RoutedEventArgs e)
        {
            if (xWebView.CanGoForward == true)
            {
                xWebView.GoForward();
                Back.IsEnabled = true;
            }
            else
            {
                Forward.IsEnabled = false;
            }
            
        }

        private void Refresh_OnClick(object sender, RoutedEventArgs e)
        {
            xWebView.Refresh();
        }
    }
}
