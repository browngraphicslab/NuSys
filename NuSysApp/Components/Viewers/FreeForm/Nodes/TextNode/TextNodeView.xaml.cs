using Windows.UI;
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

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    public sealed partial class TextNodeView : AnimatableUserControl, IThumbnailable
    {
        private MediaCapture _mediaCapture;
        private bool _isRecording;
        private Image _dragItem;

        private enum DragMode { Duplicate, Tag, Link};
        private DragMode _currenDragMode = DragMode.Duplicate;
        private List<InqLineModel> _lines = new List<InqLineModel>(); 

        public TextNodeView(TextNodeViewModel vm)
        {
            InitializeComponent();

            TextNodeWebView.Navigate(new Uri("ms-appx-web:///Components/TextEditor/textview.html"));
            DataContext = vm;

            /*
            var contentId = (vm.Model as ElementModel).ContentId;
            var content = SessionController.Instance.ContentController.Get(contentId);
            if (content != null)
            {
                if (content.Loaded)
                {
                    UpdateText(content.Data);
                }
                else
                {
                    content.OnContentChanged += delegate
                    {
                        UpdateText(content.Data);
                    };
                }
            }*/
            var navigated = false;

            TextNodeWebView.NavigationCompleted += delegate
            {
                navigated = true;
            };

            (vm as TextNodeViewModel).TextBindingChanged += delegate(object source, string text)
            {
                if (navigated)
                {
                    UpdateText(text);
                }
                else
                {
                    TextNodeWebView.NavigationCompleted += delegate
                    {
                        UpdateText(text);
                    };
                }
            };

            var inqModel = new InqCanvasModel(SessionController.Instance.GenerateId());
            var inqViewModel = new InqCanvasViewModel(inqModel, new Size(vm.Width, vm.Height));
            
            var inqView = new InqCanvasView(inqViewModel);
            inqView.IsEnabled = true;
            rr.Children.Add(inqView);

            TextNodeWebView.ScriptNotify += wvBrowser_ScriptNotify;

            vm.Controller.ContentChanged += delegate(object source, LibraryElementModel data)
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

            vm.ScreenVisibility += OnScreenVisibilityChanged;

            inqModel.LineFinalizedLocally += async delegate(InqLineModel model)
            {
                var nm = model.GetScaled(Constants.MaxCanvasSize);
                _lines.Add(nm);
                
                var texts = await InkToText(_lines); 
                if (texts.Count > 0)
                    UpdateText(texts[0]);
            };


            EditText.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            EditText.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            DuplicateElement.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            DuplicateElement.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

            Link.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            Link.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

        }

        private async void OnScreenVisibilityChanged(object source, bool isOnScreen)
        {
            if (isOnScreen)
            {
                nodeTpl.HideBitmapRender();
            }
            else
            {
                await nodeTpl.ShowBitmapRender(this);
            }
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

        

        private bool _isopen;
        private string _text = string.Empty;

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

        void wvBrowser_ScriptNotify(object sender, NotifyEventArgs e)
        {
            // The string received from the JavaScript code can be found in e.Value
            string data = e.Value;
            if (data != "")
            {
                var vm = DataContext as ElementViewModel;
                var controller = (TextNodeController)vm.Controller;
                controller.ContentModel?.SetContentData(vm,data);
            }
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
                vm.Controller.RequestDuplicate(r.X, r.Y);
            }

            if (_currenDragMode == DragMode.Tag)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
               // hitsStart = hitsStart.Where(uiElem => uiElem);
                hitsStart = hitsStart.Where(ui => (ui as FrameworkElement).Name == "Tags");
                if (hitsStart.Any())
                {
                    var el = (FrameworkElement)hitsStart.First();
                    var vm = (ElementViewModel) el.DataContext;
                    var tags = (List<string>)vm.Model.GetMetaData("tags");
                    tags.Add(_text);
                    vm.Controller.SetMetadata("tags", tags);

                } else { 

                    var contentId = SessionController.Instance.GenerateId();

                    var dict = new Message();
                    dict["width"] = 300;
                    dict["height"] = 150;
                    dict["nodeType"] = ElementType.Tag.ToString();
                    dict["x"] = r.X;
                    dict["y"] = r.Y;
                    dict["title"] = _text;
                    dict["contentId"] = contentId;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                    dict["creatorContentID"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;

                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new CreateNewLibraryElementRequest(contentId, "", ElementType.Tag, dict.ContainsKey("title") ? dict["title"].ToString() : null));
                }
            }

            if (_currenDragMode == DragMode.Link)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem as FrameworkElement).DataContext is ElementViewModel).ToList();
                if (hitsStart.Any()) { 
                    var first = (FrameworkElement)hitsStart.First();
                    var dc = (ElementViewModel) first.DataContext;
                    var vm = (ElementViewModel)DataContext;
                    vm.Controller.RequestLinkTo(dc.Id);
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


        private void RecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            xMediaRecotder.Visibility = Visibility.Visible;
            TextNodeWebView.Visibility = Visibility.Collapsed;
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
    }
}