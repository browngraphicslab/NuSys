using Windows.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Text;
using Windows.UI.Xaml.Media.Imaging;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.SpeechRecognition;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Shapes;
using MyToolkit.Converters;
using MyToolkit.Utilities;
using NuSysApp.Util;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class TextNodeView : AnimatableUserControl, IThumbnailable
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;

        private bool _isopen;
        private string _text = string.Empty;


        private static int _count = 0;
        private bool navigated = false;

        // Inking variables
        private string _saved = string.Empty;
        private InqCanvasView _inqView;
        private Rectangle _curr;
        private Rectangle _marker;


        //speech to text variables
        private static SpeechRecognizer _speechRecognizer;
        private CoreDispatcher _dispatcher;
        private StringBuilder _dictatedTextBuilder;
        private StringBuilder _hypothesisBuilder;



        public TextNodeView(TextNodeViewModel vm)
        {
            if (_speechRecognizer == null && !WaitingRoomView.IS_HUB)
            {
                _speechRecognizer = new SpeechRecognizer();
            }
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

            InitSpeechRecognition();

            Record.AddHandler(PointerPressedEvent, new PointerEventHandler(RecordButton_OnClick), true);
            Record.AddHandler(PointerReleasedEvent, new PointerEventHandler(RecordButton_Released), true);

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
            var vm = (TextNodeViewModel)DataContext;
            vm.TextBindingChanged -= TextChanged;
            TextNodeWebView.NavigationCompleted -= TextNodeWebViewOnNavigationCompleted;
            TextNodeWebView.ScriptNotify -= wvBrowser_ScriptNotify;
            nodeTpl.Dispose();
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
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
            _saved = _text;
            if (!_isopen)
            {
                SetUpInking();
                inker.Visibility = Visibility.Visible;
                //FlipOpen.Begin();
                SetImage("ms-appx:///Assets/icon_whitex.png", InkImg);
            }
            else
            {
                inker.Visibility = Visibility.Collapsed;
                SetImage("ms-appx:///Assets/node icons/pen.png", InkImg);
                //FlipClose.Begin();
            }
            _isopen = !_isopen;
        }



        private void SetUpInking()
        {
            var vm = (TextNodeViewModel)DataContext;
            var inqModel = new InqCanvasModel(SessionController.Instance.GenerateId());
            var inqViewModel = new InqCanvasViewModel(inqModel, new Size(vm.Width, vm.Height));

            _inqView = new InqCanvasView(inqViewModel);
            _inqView.IsEnabled = true;

            ResetInkingCanvas();
            //_inqView.PointerPressed += InkerClick;

            inkerCanvas.Children.Add(_curr);
            inkerCanvas.Children.Add(_inqView);
            inkerCanvas.Children.Add(_marker);

            _saved = _text;
            List<InqLineModel> lines = new List<InqLineModel>();
            inqModel.LineFinalizedLocally += async delegate (InqLineModel model)
            {
                var nm = model.GetScaled(Constants.MaxCanvasSize);

                lines.Add(nm);
                var texts = await InkToText(lines);
                if (texts.Count > 0)
                    UpdateText(_saved + " " + texts[0]);
                UpdateController(_text);
            };
        }

        private void ResetInkingCanvas()
        {
            inkerCanvas.Children.Clear();
            _curr = new Rectangle();
            _marker = new Rectangle();
            _curr.Opacity = 0.5;
            _marker.Opacity = 0.1;
            _curr.Height = 100;
            _marker.Height = 100;
            _marker.Width = 30;
            _marker.HorizontalAlignment = HorizontalAlignment.Left;
            _curr.Fill = new SolidColorBrush(Color.FromArgb(1, 242, 242, 242));
            _marker.Stroke = new SolidColorBrush(Colors.LightSlateGray);
            _marker.Fill = new SolidColorBrush(Color.FromArgb(1, 242, 242, 242));

            _marker.PointerPressed += InkerClick;
        }

        private void InkerClick(Object sender, PointerRoutedEventArgs e)
        {
            SetUpInking();
            e.Handled = false;
            _inqView.OnPointerPressed(sender, e);
        }


        private void OnInkSpace(object sender, RoutedEventArgs e)
        {
            SetUpInking();
        }

        private void OnInkPeriod(object sender, RoutedEventArgs e)
        {
            UpdateText(_text + ".");
            UpdateController(_text);
            SetUpInking();
        }

        private void OnInkBackspace(object sender, RoutedEventArgs e)
        {

        }

        private void OnInkReturn(object sender, RoutedEventArgs e)
        {
            UpdateText(_text + "<br>");
            UpdateController(_text);
            SetUpInking();
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
            var vm = DataContext as TextNodeViewModel;
            var controller = (TextNodeController)vm.Controller;
            vm.TextBindingChanged -= TextChanged;
            controller.LibraryElementController?.SetContentData(s);
            vm.TextBindingChanged += TextChanged;
            _text = s;
        }

        public void SetImage(String url, Image buttonName)
        {
            Uri imageUri = new Uri(url, UriKind.Absolute);
            BitmapImage imageBitmap = new BitmapImage(imageUri);
            buttonName.Source = imageBitmap;
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

        #region Speech Recognition


        private async Task InitSpeechRecognition()
        {
            if (WaitingRoomView.IS_HUB)
            {
                return;
            }
            this._dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            this._dictatedTextBuilder = new StringBuilder();
            this._hypothesisBuilder = new StringBuilder();

            ((ElementViewModel)DataContext).PropertyChanged += propertyChanged;

            await _speechRecognizer.CompileConstraintsAsync();
        }

        private void propertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsSelected")
            {
                if (((ElementViewModel)DataContext).IsSelected)
                {
                    _speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;

                    _speechRecognizer.ContinuousRecognitionSession.ResultGenerated +=
                ContinuousRecognitionSession_ResultGenerated;

                    _speechRecognizer.ContinuousRecognitionSession.Completed +=
              ContinuousRecognitionSession_Completed;
                }
                else
                {
                    _speechRecognizer.HypothesisGenerated -= SpeechRecognizer_HypothesisGenerated;

                    _speechRecognizer.ContinuousRecognitionSession.ResultGenerated -=
                ContinuousRecognitionSession_ResultGenerated;

                    _speechRecognizer.ContinuousRecognitionSession.Completed -=
              ContinuousRecognitionSession_Completed;
                }
            }
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;
            string textboxContent = _hypothesisBuilder.ToString() + " " + hypothesis + "...";

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateText(_saved + " " + textboxContent);
                UpdateController(_text);
            });
        }

        private async void ContinuousRecognitionSession_ResultGenerated(
      SpeechContinuousRecognitionSession sender,
      SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            //if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
            //  args.Result.Confidence == SpeechRecognitionConfidence.High)
            //{
            _dictatedTextBuilder.Append(args.Result.Text + " ");

            await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                UpdateText(_saved + " " + _dictatedTextBuilder.ToString());
                UpdateController(_text);
                Debug.WriteLine(_dictatedTextBuilder.ToString());
            });
            //}
        }

        private async void ContinuousRecognitionSession_Completed(
      SpeechContinuousRecognitionSession sender,
      SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (args.Status != SpeechRecognitionResultStatus.Success)
            {
                if (args.Status == SpeechRecognitionResultStatus.TimeoutExceeded)
                {
                    await _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        UpdateText(_saved + " " + _dictatedTextBuilder.ToString());
                        UpdateController(_text);

                    });
                }
            }
        }


        private async void RecordButton_OnClick(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            var vm = DataContext as TextNodeViewModel;
            if (vm == null) return;
            SessionController.Instance.SessionView.SpeechToTextBox.Instantiate(vm.Controller as TextNodeController, _text);

            /* old code
            _isRecording = true;
            _saved = _text;
            _dictatedTextBuilder = new StringBuilder();
            _hypothesisBuilder = new StringBuilder();
            if (_isopen)
            {
                inker.Visibility = Visibility.Collapsed;
                _isopen = false;
            }
            if (_speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await _speechRecognizer.ContinuousRecognitionSession.StartAsync();
            }
            */

            //if (!session.IsRecording)
            //{
            //    _voiceTranscription = session.TranscribeVoice();
            //    // speechString = session.SpeechString;
            //    Debug.WriteLine("SESSION returned with: " + _voiceTranscription);
            //}
        }



        private async void RecordButton_Released(object sender, PointerRoutedEventArgs e)
        {
            //if (String.IsNullOrEmpty(speechString))
            //{
            //    speechString = await _voiceTranscription;
            //    Debug.WriteLine("RECORD RELEASED WITH STRING: " + speechString);
            //    UpdateText(_text + " " + speechString);
            //    UpdateController(_text);
            //}
            //speechString = "";


            /* old code
            if (_speechRecognizer.State != SpeechRecognizerState.Idle)
            {
                await _speechRecognizer.ContinuousRecognitionSession.StopAsync();
            }
            _isRecording = false;
            */
        }

        #endregion



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
            _curr.Height += e.Delta.Translation.Y;
            _marker.Height += e.Delta.Translation.Y;
        }

        //private void XImage_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        //{
        //    if (_drawingRegion)
        //    {
        //        Debug.WriteLine("here");
        //        Canvas.Children.Add(TempRegion);
        //        Canvas.SetLeft(TempRegion, e.GetCurrentPoint((UIElement)sender).Position.X);
        //        Canvas.SetTop(TempRegion, e.GetCurrentPoint((UIElement)sender).Position.Y);
        //        TempRegion.Opacity = 1;
        //    }
        //}

        //private void XImage_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        //{
        //    if (_drawingRegion)
        //    {

        //        //add rectangle to model list
        //        //remove temp rectangle
        //        //have another method that reads all things from model and adds it.

        //        ImageElementViewModel vm = (ImageElementViewModel)DataContext;

        //        var width = vm.Model.Width;
        //        var height = vm.Model.Height;

        //        var leftRatio = Canvas.GetLeft(TempRegion) / width;
        //        var topRatio = Canvas.GetTop(TempRegion) / height;

        //        var widthRatio = TempRegion.Width / width;
        //        var heightRatio = TempRegion.Height / Height;

        //        RectanglePoints rectangle = new RectanglePoints(leftRatio, topRatio, widthRatio, heightRatio);

        //        // add to controller
        //        (DataContext as ImageElementViewModel).Controller.SetRegion(rectangle);
        //        Rectangle rect = rectangle.getRectangle();

        //        rect.Width = width * rectangle.getWidthRatio();
        //        rect.Height = height * rectangle.getHeightRatio();
        //        Canvas.Children.Add(rect);
        //        Canvas.SetLeft(rect, rectangle.getLeftRatio() * width);
        //        Canvas.SetTop(rect, rectangle.getTopRatio() * height);

        //        // works?
        //        Canvas.Children.Remove(TempRegion);

        //        //(DataContext as ImageElementViewModel).RegionsList.Add(rect);
        //        //(DataContext as ImageElementViewModel).Model.Regions.Add(rectangle);

        //        _drawingRegion = false;
        //        //this.AddRegionsToCanvas();
        //    }
        //}

        //private void XImage_OnPointerMoved(object sender, PointerRoutedEventArgs e)
        //{
        //    if (e.GetCurrentPoint((UIElement)sender).Properties.IsLeftButtonPressed && _drawingRegion)
        //    {
        //        TempRegion.Height = e.GetCurrentPoint((UIElement)sender).Position.Y - Canvas.GetTop(TempRegion);
        //        TempRegion.Width = e.GetCurrentPoint((UIElement)sender).Position.X - Canvas.GetLeft(TempRegion);
        //    }
        //}
    }
}