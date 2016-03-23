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
        private WebNodeViewModel _viewMod;

        public WebDetailView(WebNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            _viewMod = vm;

            Loaded += delegate (object sender, RoutedEventArgs args)
            {
                SetDimension(SessionController.Instance.SessionView.ActualWidth / 2 - 30, SessionController.Instance.SessionView.ActualHeight);

                //(vm.Model as WebNodeModel).Url = "http://www.google.com";
                //(vm.Model as WebNodeModel).Url = vm.Url;

                //vm.Controller.LibraryElementModel?.SetContentData(vm, vm.Url);

            };
            SetDimension(SessionController.Instance.SessionView.ActualWidth / 2 - 30, SessionController.Instance.SessionView.ActualHeight);
            var url = vm.Controller.LibraryElementModel.Data;
            OnUrlChanged(url);

            vm.Controller.LibraryElementModel.OnContentChanged += delegate (ElementViewModel originalSenderViewModel)
            {
                url = vm.Controller.LibraryElementModel.Data;
                OnUrlChanged(url);
            };
        }
    
        public void SetDimension(double parentWidth, double parentHeight)
        {
            //webViewPanel.Width = parentWidth*0.8;
            //webViewPanel.Height = parentHeight*0.6;
            xWebView.Width = parentWidth;
            xWebView.Height = parentHeight;
            //xWebView.MaxHeight = SessionController.Instance.SessionView.ActualHeight - 370;
            //xScrollViewer.Height = xWebView.Height;
            //xScrollViewer.Width = xWebView.Width;
            //webTopBar.Width = xWebView.ActualWidth;
            //this.Height = xWebView.ActualHeight + 37;
            //this.Width = xWebView.ActualWidth;
            Canvas.SetZIndex(Refresh,20);
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
                var url = this.checkIfUrlRight(xUrlBox.Text);
                xWebView.Navigate(new Uri(url));
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
            if (_viewMod.Controller.LibraryElementModel.Data != url)
            {
                (DataContext as WebNodeViewModel).Url = url;
               // (DataContext as WebNodeViewModel).History.Add(new WebNodeModel.Webpage(url, GetTimestamp(DateTime.Now)));
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

