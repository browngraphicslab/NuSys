using Windows.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Diagnostics;
using Windows.UI.Xaml.Media.Imaging;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class TextNodeView : AnimatableUserControl, IThumbnailable
    {

        private List<Image> _images = new List<Image>();
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private Image _dragItem;

        private enum DragMode { Duplicate, Tag, Link};
        private DragMode _currenDragMode = DragMode.Duplicate;

        public TextNodeView(TextNodeViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;

            var contentId = (vm.Model as ElementModel).ContentId;
            var content = SessionController.Instance.ContentController.Get(contentId);
            if (content != null)
            {
                rtfTextBox.SetRtfText(content.Data);
            }

            (vm.Model as TextElementModel).TextChanged += delegate (object source, string text)
            {
                rtfTextBox.SetRtfText(text);
                // rtfTextBox.SetRtfText();
            };

            var inqView = new InqCanvasView(new InqCanvasViewModel(new InqCanvasModel(SessionController.Instance.GenerateId()),
                new Size(vm.Width, vm.Height)));
            inqView.IsEnabled = true;
            rr.Children.Add(inqView);

            

            vm.Controller.ContentChanged += delegate(object source, NodeContentModel data)
            {
                if (xMediaRecotder.Visibility == Visibility.Collapsed)
                    return;

                var memoryStream = new InMemoryRandomAccessStream();
                var byteArray = Convert.FromBase64String(data.Data);
                memoryStream.AsStreamForWrite().Write(byteArray, 0, byteArray.Length);
                memoryStream.Seek(0);
                playbackElement.SetSource(memoryStream, "video/mp4");
                _isRecording = false;
            };


            EditText.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            EditText.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            DuplicateElement.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            DuplicateElement.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            Link.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            Link.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

        }




        private async Task TranscribeVoice()
        {
            // Create an instance of SpeechRecognizer. 
            var speechRecognizer = new Windows.Media.SpeechRecognition.SpeechRecognizer();
            //speechRecognizer.
            //speechRecognizer.UIOptions.ShowConfirmation = false;
            //speechRecognizer.UIOptions.IsReadBackEnabled = false;
            //speechRecognizer.UIOptions.AudiblePrompt = "";
            // Compile the dictation grammar that is loaded by default. = ""; 
            await speechRecognizer.CompileConstraintsAsync();
            string spokenString = "";
            // Start recognition. 
            try
            {
                Windows.Media.SpeechRecognition.SpeechRecognitionResult speechRecognitionResult = await speechRecognizer.RecognizeAsync();
                // If successful, display the recognition result. 
                if (speechRecognitionResult.Status == Windows.Media.SpeechRecognition.SpeechRecognitionResultStatus.Success)
                {
                    spokenString = speechRecognitionResult.Text;
                }
            }
            catch (Exception exception)
            {
            }
            speechRecognizer.Dispose();
            //this.mdTextBox.Text = spokenString;
            var vm = (TextNodeViewModel)this.DataContext;
            vm.Init();
        }


        private bool _isopen;

        private async void OnEditClick(object sender, RoutedEventArgs e)
        {
 
        }

        private void OnInkClick(object sender, RoutedEventArgs e)
        {
            if (!_isopen)
            {
                FlipOpen.Begin();
            }
            else
            {
                FlipClose.Begin();
            }
            _isopen = !_isopen;
        }

        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(null).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));

            if (_currenDragMode == DragMode.Duplicate)
            {
                var vm = (ElementViewModel)DataContext;
                vm.Controller.Duplicate(r.X, r.Y);
            }

            if (_currenDragMode == DragMode.Link)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel).ToList();
                if (hitsStart.Any()) { 
                    var first = (FrameworkElement)hitsStart.First();
                    var dc = (ElementViewModel) first.DataContext;
                    var vm = (ElementViewModel)DataContext;
                    vm.Controller.LinkTo(dc.Id);
                }
            }
            
            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta));

        }

        private void BtnAddOnManipulationDelta(object sender, PointerRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var p = args.GetCurrentPoint(xCanvas).Position;
            t.TranslateX = p.X - _dragItem.ActualWidth/2;
            t.TranslateY = p.Y - _dragItem.ActualHeight / 2;
        }


        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {
            CapturePointer(args.Pointer);

            if (sender == DuplicateElement)
            {
                _currenDragMode = DragMode.Duplicate;
            }

            if (sender == EditText)
            {
                _currenDragMode = DragMode.Tag;
            }
            if (sender == Link)
            {
                _currenDragMode = DragMode.Link;
            }

            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            _dragItem = new Image();
            _dragItem.Source = bmp;
            _dragItem.Width = 50;
            _dragItem.Height = 50;
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            (sender as FrameworkElement).AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta), true);
        }

        

        public NodeTemplate NodeTpl
        {
            get { return nodeTpl; }
        }

        private void OnDeleteClick(object sender, RoutedEventArgs e)
        {
            var vm = (ElementViewModel)this.DataContext;
            vm.Controller.Delete();
        }


        public async Task<RenderTargetBitmap> ToThumbnail(int width, int height)
        {
            var r = new RenderTargetBitmap();
            await r.RenderAsync(rtfTextBox, width, height);
            return r;
        }

        private void borderRect_OnPointerEntered(object sender, PointerRoutedEventArgs e)
        {
            borderRect.Opacity = 1;
        }

        private void borderRect_OnPointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (rtfTextBox.GetNonRtfText() == null || rtfTextBox.GetNonRtfText() == "" || rtfTextBox.GetNonRtfText().Trim() == "")
            {
                borderRect.Opacity = 1;
            }
            else
            {
                borderRect.Opacity = 0;
            }
        }


        private void RecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            xMediaRecotder.Visibility = Visibility.Visible;
            rtfTextBox.Visibility = Visibility.Collapsed;
        }
    }
}