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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class TextDetailView : UserControl
    {
        //private SpeechRecognizer _recognizer;
        //private bool _isRecording;
        private ObservableCollection<String> sizes = new ObservableCollection<String>();
        private ObservableCollection<FontFamily> fonts = new ObservableCollection<FontFamily>();
        private string _modelContentId;
        private string _modelId;
        private string _modelText = "";
        public TextDetailView(TextNodeViewModel vm)
        {

            InitializeComponent();


            DataContext = vm;

            var model = (TextElementModel)vm.Model;


            List<Uri> AllowedUris = new List<Uri>();
            AllowedUris.Add(new Uri("ms-appx-web:///Components/TextEditor/texteditor.html"));
            MyWebView.ScriptNotify += wvBrowser_ScriptNotify;

            Loaded += async delegate (object sender, RoutedEventArgs args)
            {
                await SessionController.Instance.InitializeRecog();
            };

            MyWebView.Navigate(new Uri("ms-appx-web:///Components/TextEditor/texteditor.html"));
            MyWebView.NavigationCompleted += delegate (WebView w, WebViewNavigationCompletedEventArgs e)
            {
                if (model.Text != "")
                {
                    UpdateText(model.Text);
                }
                OpenTextBox(model.Text);

            };

            _modelContentId = model.ContentId;
            _modelId = model.Id;

        }

        private void WebView_OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            string url = sender.Source.AbsoluteUri;

        }

        /*
        Updates text in editor when necessary
        */
        private async void UpdateText(String str)
        {
            if (str != "")
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

            String[] myString = { str };
            IEnumerable<String> s = myString;
            MyWebView.InvokeScriptAsync("InsertText", s);
            MyWebView.InvokeScriptAsync("clickableLinks", null);

        }



        /* 
        Two values are received from JS: link clicks and entire document HTML updates
        Method parses these options, calls other methods accordingly
         */
        void wvBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // The string received from the JavaScript code can be found in e.Value
            string data = e.Value;
            Debug.WriteLine(data);

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
            m["nodeType"] = ElementType.Web;
            m["autoCreate"] = true;
            m["creators"] = new List<string>() { SessionController.Instance.ActiveFreeFormViewer.Id };
            m["id"] = nodeid;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(m));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, ""));


        }

        /*
        Updates text value (in HTML) in Model as text is added and edited in Text Editor
        */
        private void UpdateModelText(String s)
        {
            if (s != "")
            {
                var vm = DataContext as ElementViewModel;
                var model = (TextElementModel)vm.Model;
                model.Text = s;
            }
        }

        public void Dispose()
        {
            UpdateModelText(_modelText);
        }

        private async void OnRecordClick(object sender, RoutedEventArgs e)
        {
            //if (!_isRecording)

            var session = SessionController.Instance;
            if (!session.IsRecording)
            {
                //var oldColor = this.RecordVoice.Background;
                Color c = new Color();
                c.A = 255;
                c.R = 199;
                c.G = 84;
                c.B = 82;
                //     this.RecordVoice.Background = new SolidColorBrush(c);
                //await TranscribeVoice();
                await session.TranscribeVoice();
                //     this.RecordVoice.Background = oldColor;
                var vm = (TextNodeViewModel)DataContext;
                ((TextElementModel)vm.Model).Text = session.SpeechString;
            }
            else
            {
                var vm = this.DataContext as TextNodeViewModel;
                //   this.RecordVoice.Background = vm.Color;
            }
        }

        //private async Task TranscribeVoice()
        //{
        //    string spokenString = "";
        //    // Create an instance of SpeechRecognizer. 
        //    // Start recognition. 

        //    try
        //    {
        //       // this.RecordVoice.Click += stopTranscribing;
        //        _isRecording = true;
        //        SpeechRecognitionResult speechRecognitionResult = await _recognizer.RecognizeAsync();
        //        _isRecording = false;
        //      //  this.RecordVoice.Click -= stopTranscribing;
        //        // If successful, display the recognition result. 
        //        if (speechRecognitionResult.Status == SpeechRecognitionResultStatus.Success)
        //        {
        //            spokenString = speechRecognitionResult.Text;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        const int privacyPolicyHResult = unchecked((int)0x80045509);
        //        const int networkNotAvailable = unchecked((int)0x80045504);

        //        if (ex.HResult == privacyPolicyHResult)
        //        {
        //            // User has not accepted the speech privacy policy
        //            string error = "In order to use dictation features, we need you to agree to Microsoft's speech privacy policy. To do this, go to your Windows 10 Settings and go to Privacy - Speech, inking, & typing, and enable data collection.";
        //            var messageDialog = new Windows.UI.Popups.MessageDialog(error);
        //            messageDialog.ShowAsync();

        //        }
        //        else if (ex.HResult == networkNotAvailable)
        //        {
        //            string error = "In order to use dictation features, NuSys requires an internet connection";
        //            var messageDialog = new Windows.UI.Popups.MessageDialog(error);
        //            messageDialog.ShowAsync();
        //        }
        //    }
        //    //_recognizer.Dispose();
        //   // this.mdTextBox.Text = spokenString;

        //    Debug.WriteLine(spokenString);

        //    var vm = (TextNodeViewModel)DataContext;
        //    (vm.Model as TextNodeModel).Text = spokenString;
        //}

        //private async void stopTranscribing(object o, RoutedEventArgs e)
        //{
        //    _recognizer.StopRecognitionAsync();
        //    _isRecording = false;
        //   // this.RecordVoice.Click -= stopTranscribing;
        //}

        private void UpdateText()
        {
            var request = new ChangeContentRequest(_modelId, _modelContentId, _modelText);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request, NetworkClient.PacketType.UDP);
        }

        private async void OnGoToSource(object sender, RoutedEventArgs e)
        {
            var model = (TextElementModel)((TextNodeViewModel)DataContext).Model;
            string token = model.GetMetaData("Token")?.ToString();

            if (!Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.ContainsItem(token?.ToString()))
            {
                return;
            }

            string ext = Path.GetExtension(model.GetMetaData("FilePath").ToString());
            StorageFolder toWriteFolder = NuSysStorages.OpenDocParamsFolder;

            if (Constants.WordFileTypes.Contains(ext))
            {
                string bookmarkId = model.GetMetaData("BookmarkId").ToString();
                StorageFile writeBookmarkFile = await StorageUtil.CreateFileIfNotExists(NuSysStorages.OpenDocParamsFolder, token);

                using (StreamWriter writer = new StreamWriter(await writeBookmarkFile.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(bookmarkId);
                }

                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimeWord.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(token);
                }
            }
            else if (Constants.PowerpointFileTypes.Contains(ext))
            {
                using (StreamWriter writer = new StreamWriter(await NuSysStorages.FirstTimePowerpoint.OpenStreamForWriteAsync()))
                {
                    writer.WriteLineAsync(token);
                }
            }

            await AccessList.OpenFile(token);
        }
    }
}