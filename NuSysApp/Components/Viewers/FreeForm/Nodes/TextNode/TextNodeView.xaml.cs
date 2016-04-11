﻿using Windows.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Storage.Streams;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using MyToolkit.Utilities;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class TextNodeView : AnimatableUserControl, IThumbnailable
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;


        private bool _isopen;
        private string _text = string.Empty;

        private string _savedForInking = string.Empty;

        private static int _count = 0;
        private bool navigated = false;
        private string speechString="";
        private Rectangle r;

        public TextNodeView(TextNodeViewModel vm)
        {
            _count++;
            Debug.WriteLine(_count);
            InitializeComponent();
            TextNodeWebView.Navigate(new Uri("ms-appx-web:///Components/TextEditor/textview.html"));
            DataContext = vm;

            this.SetUpInking();
  
            vm.Controller.Disposed += ControllerOnDisposed;
            vm.TextBindingChanged += TextChanged;
            vm.TextUnselected += Blur;
            TextNodeWebView.NavigationCompleted += TextNodeWebViewOnNavigationCompleted;
            TextNodeWebView.ScriptNotify += wvBrowser_ScriptNotify;
        }

        private void TextNodeWebViewOnNavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            var vm = (TextNodeViewModel)DataContext;
            navigated = true;
            UpdateText(vm.Text);
        }

        private void TextChanged(object source, string text)
        {

             if (navigated)
             {
                 UpdateText(text);
             }
             else
             {
                 TextNodeWebView.NavigationCompleted -= TextNodeWebViewOnNavigationCompleted;
                 TextNodeWebView.NavigationCompleted += TextNodeWebViewOnNavigationCompleted;
             }
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (TextNodeViewModel) DataContext;
            vm.TextBindingChanged -= TextChanged;
            TextNodeWebView.NavigationCompleted -= TextNodeWebViewOnNavigationCompleted;
            TextNodeWebView.ScriptNotify -= wvBrowser_ScriptNotify;
            nodeTpl.Dispose();
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
        }

        private void SetUpInking()
        {
            var vm = (TextNodeViewModel)DataContext;
            var inqModel = new InqCanvasModel(SessionController.Instance.GenerateId());
            var inqViewModel = new InqCanvasViewModel(inqModel, new Size(vm.Width, vm.Height));

            var inqView = new InqCanvasView(inqViewModel);
            inqView.IsEnabled = true;
            inkerCanvas.Children.Clear();
            r = new Rectangle();
            r.Opacity = 0.5;
            r.Height = 100;
            r.Fill = new SolidColorBrush(Colors.LightSlateGray);

            inkerCanvas.Children.Add(r);
            inkerCanvas.Children.Add(inqView);
            _savedForInking = _text;
            List<InqLineModel> lines = new List<InqLineModel>();
            inqModel.LineFinalizedLocally += async delegate (InqLineModel model)
            {
                var nm = model.GetScaled(Constants.MaxCanvasSize);

                lines.Add(nm);
                var texts = await InkToText(lines);
                if (texts.Count > 0)
                    UpdateText(_savedForInking + " " + texts[0]);
                UpdateController(_text);
            };
        }

        private async void UpdateText(String str)
        {
            if (!string.IsNullOrEmpty(str))
            {
                String[] myString = { str };
                IEnumerable<String> s = myString;
                TextNodeWebView.InvokeScriptAsync("InsertText", s);
            }
            _text = str;
        }

        private async void Blur(object source)
        {
            TextNodeWebView.InvokeScriptAsync("Blur", null);
        }



        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
            if (_isopen)
            {
                inker.Visibility = Visibility.Collapsed;

                //FlipClose.Begin();
                _isopen = false;
            }
        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            _savedForInking = _text;
            if (!_isopen)
            {
                SetUpInking();
                inker.Visibility = Visibility.Visible;
                //FlipOpen.Begin();
            }
            else
            {
                inker.Visibility = Visibility.Collapsed;

                //FlipClose.Begin();
            }
            _isopen = !_isopen;
        }

        void wvBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // The string received from the JavaScript code can be found in e.Value
            string data = e.Value;
            if (data != "")
            {
                UpdateController(data);             
            }
        }

        private void UpdateController(String s)
        {
            var vm = DataContext as ElementViewModel;
            var controller = (TextNodeController)vm.Controller;
            controller.LibraryElementModel?.SetContentData(vm, s);
        }

        
        
        public NodeTemplate NodeTpl
        {
            get { return nodeTpl; }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            vm.Controller.RequestDelete();
        }


        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(wee, width, height);
            return r;
        }

        private void borderRect_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
           // borderRect.Opacity = 1;
        }

        private void borderRect_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
         //       borderRect.Opacity = 0;
        }


        private async void RecordButton_OnClick(object sender, PointerRoutedEventArgs e)
        {
            _isRecording = true;
            if(_isopen)
            {
                inker.Visibility =Visibility.Collapsed;
                _isopen = false;
            }
            var session = SessionController.Instance;
            if (!session.IsRecording)
            {
                await session.TranscribeVoice();
                speechString = session.SpeechString;
            }
        }

        private void RecordButton_Released(object sender, PointerRoutedEventArgs e)
        {
            if (_isRecording)
            {
                Debug.WriteLine("RECORD RELEASED");
                UpdateText(_text + " " + speechString);
                UpdateController(_text);
            }
            speechString = "";
            _isRecording = false;
        }


        public async Task<List<string>> InkToText(List<InqLineModel> inqLineModels)
        {
            if (inqLineModels.Count == 0)
                return new List<string>();

            var im = new InkManager();
            var b = new InkStrokeBuilder();

            foreach (var inqLineModel in inqLineModels)
            {
                var pc = new PointCollection();
                foreach (var point2D in inqLineModel.Points)
                {
                    pc.Add(new Point(point2D.X, point2D.Y));
                }
                var stroke = b.CreateStroke(pc);
                im.AddStroke(stroke);
            }

            var result = await im.RecognizeAsync(InkRecognitionTarget.All);
            return result[0].GetTextCandidates().ToList();
        }

        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            inker.Height += e.Delta.Translation.Y;
            r.Height += e.Delta.Translation.Y;
        }
    }
}