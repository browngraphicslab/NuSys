using NuSysApp.Util;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.Media.SpeechSynthesis;
using Windows.Storage;
using Windows.System;
using Windows.UI;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;


// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class TextDetailHomeTabView : UserControl
    {
        //private SpeechRecognizer _recognizer;
        //private bool _isRecording;
        private ObservableCollection<String> sizes = new ObservableCollection<String>();
        private ObservableCollection<FontFamily> fonts = new ObservableCollection<FontFamily>();
        private string _modelText="";

        public TextDetailHomeTabView(TextDetailHomeTabViewModel vm)
        {
            InitializeComponent();

            DataContext = vm;
          
            List<Uri> AllowedUris = new List<Uri>();
            AllowedUris.Add(new Uri("ms-appx-web:///Components/TextEditor/texteditor.html"));

            Loaded += TextDetailHomeTabView_Loaded;

            SizeChanged += TextDetailHomeTabView_SizeChanged;

            MyWebView.Navigate(new Uri("ms-appx-web:///Components/TextEditor/texteditor.html"));
            MyWebView.NavigationCompleted += MyWebViewOnNavigationCompleted;
            vm.TextChanged += VmOnTextBindingChanged;
            MyWebView.ScriptNotify += wvBrowser_ScriptNotify;

            vm.LibraryElementController.Disposed += DetailViewerView_Disposed;

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed += DetailViewerView_Disposed;
            createCount++;
        }

        private async void TextDetailHomeTabView_Loaded(object sender, RoutedEventArgs e)
        {
            await SessionController.Instance.InitializeRecog();
            SetHeight(SessionController.Instance.SessionView.ActualHeight / 2);
        }

        private void TextDetailHomeTabView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetHeight(SessionController.Instance.SessionView.ActualHeight / 2);
            SetDimension(SessionController.Instance.SessionView.DetailViewerView.ActualWidth);
        }

        private void DetailViewerView_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }

        public void SetHeight(double parentHeight)
        {
            MyWebView.Height = parentHeight;
        }

        private void MyWebViewOnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            if (((TextDetailHomeTabViewModel)DataContext).LibraryElementController.Data != "")
            {
                UpdateText(((TextDetailHomeTabViewModel)DataContext).LibraryElementController.Data);
            }
            OpenTextBox(((TextDetailHomeTabViewModel)DataContext).LibraryElementController.Data);
        }

        private void VmOnTextBindingChanged(object source, string text)
        {
            UpdateText(text);
        }


        /* 
        Two values are received from JS: link clicks and entire document HTML updates
        Method parses these options, calls other methods accordingly
         */
        void wvBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // The string received from the JavaScript code can be found in e.Value
            string data = e.Value;
            //Debug.WriteLine(data);

            if (data.ToLower().StartsWith("launchmylink:"))
            {
                String potentialLink = "http://" + data.Substring("LaunchMylink:".Length);
                NavigateToLink(potentialLink);

            }
            else if (data.ToLower().StartsWith("browseropen:"))
            {
                String potentialLink = "http://" + data.Substring("BrowserOpen:".Length);
                Launcher.LaunchUriAsync(new Uri(potentialLink));

            }
            else if (data != "")
            {
                UpdateModelText(data);

            }

        }
        
        public void SetDimension(double parentWidth)
        {
            MyWebView.Width = parentWidth * 0.9;
        }



        /*
        Updates text in editor when necessary
        */
        private async void UpdateText(String str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                String[] myString = { str };
                IEnumerable<String> s = myString;
                MyWebView.InvokeScriptAsync("InsertText", s);
            }
        }

        /*
        When text detail view is reopened, must be populated with text from model, and click listeners must be re-enabled
        */
        private async void OpenTextBox(String str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                String[] myString = {str};
                IEnumerable<String> s = myString;
                MyWebView.InvokeScriptAsync("InsertText", s);
                MyWebView.InvokeScriptAsync("clickableLinks", null);
            }

        }

        /*
        Opens up link from Text Detail View in new web node, in the network 
        */
        public async Task NavigateToLink(string url)
        {
            Message m = new Message();

            var width = SessionController.Instance.SessionView.ActualWidth;
            var height = SessionController.Instance.SessionView.ActualHeight;

            var centerpoint = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(new Point(width / 2, height / 2));

            var contentId = SessionController.Instance.GenerateId();
            var nodeid = SessionController.Instance.GenerateId();


            m["contentId"] = contentId;
            m["x"] = centerpoint.X - 200;
            m["y"] = centerpoint.Y - 200;
            m["width"] = 400;
            m["height"] = 400;
            m["url"] = url;
            m["nodetype"] = NusysConstants.ElementType.Web;
            m["autoCreate"] = true;
            m["creator"] = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId ;
            m["id"] = nodeid;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new NewElementRequest(m));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new CreateNewLibraryElementRequest(contentId, "",NusysConstants.ElementType.Web));


        }

        /*
        Updates text value (in HTML) in Model as text is added and edited in Text Editor
        */
        private void UpdateModelText(String s)
        {
            var vm = DataContext as TextDetailHomeTabViewModel;
            if (vm == null)
            {
                return;
            }
            vm.TextChanged -= VmOnTextBindingChanged;
            vm.LibraryElementController.ContentDataController.SetData(s);
            vm.TextChanged += VmOnTextBindingChanged;
        }

        public void Dispose()
        {
            disposeCount++;
            var vm = DataContext as TextDetailHomeTabViewModel;
            if (vm == null)
            {
                return;
            }
            MyWebView.NavigationCompleted -= MyWebViewOnNavigationCompleted;
            vm.TextChanged -= VmOnTextBindingChanged;
            MyWebView.ScriptNotify -= wvBrowser_ScriptNotify;

            vm.LibraryElementController.Disposed -= DetailViewerView_Disposed;

            var detailViewerView = SessionController.Instance.SessionView.DetailViewerView;
            detailViewerView.Disposed -= DetailViewerView_Disposed;
            vm.TextChanged -= VmOnTextBindingChanged;
            vm.Dispose();
            MyWebView = null;//This is essential because without it the webview does not dispose on time so it continues to keep taking up memory
            //UpdateModelText(_modelText);
        }
        public static int createCount = 0;
        public static int disposeCount = 0;
    }
}