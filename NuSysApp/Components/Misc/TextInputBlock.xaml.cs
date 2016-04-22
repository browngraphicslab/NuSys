using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using MyToolkit.UI;
using SharpDX;
using Point = Windows.Foundation.Point;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextInputBlock : AnimatableUserControl
    {
        #region Private Members
        private bool _recordMode;
        private bool _inkMode;

        private bool _isRecording;
        private bool _isInking;
        private bool _isActivated;

        private string _text;
        private string _savedForInking="";
        private InqCanvasView _inqView;

        private Task<String> _input;

        #endregion

        #region EventHandlers
        public delegate void TextInputBlockChangedHandler(object source, string title);
        public event TextInputBlockChangedHandler TextChanged;
        #endregion

        #region Public Properties
        public static readonly DependencyProperty LeftJustifiedProperty = DependencyProperty.RegisterAttached("LeftJustified", typeof(bool), typeof(TextInputBlock), null);
        public static readonly DependencyProperty BubbleTopProperty = DependencyProperty.RegisterAttached("BubbleLocation", typeof(bool), typeof(TextInputBlock), null);
        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached("SetHeight", typeof(double), typeof(TextInputBlock), null);
        public static readonly DependencyProperty ButtonBgProperty = DependencyProperty.RegisterAttached("ButtonBg", typeof(Windows.UI.Color), typeof(TextInputBlock), null);
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TextInputBlock), null);
        #endregion

        public TextInputBlock()
        {
            _text = "";
            _recordMode = false;
            _inkMode = false;
            _isRecording = false;
            _isInking = false;
            _isActivated = false;
            this.InitializeComponent();

            TextBox.KeyUp += TextBoxOnKeyUp;
            RecordButton.AddHandler(PointerPressedEvent, new PointerEventHandler(RecordButton_OnClick), true);
            RecordButton.AddHandler(PointerReleasedEvent, new PointerEventHandler(RecordButton_Released), true);
            this.SetUpInking();
        }

        #region Text Handling
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                this.TextBox.Text = value;
                _text = value;
            }
        }

        private void TextBoxOnKeyUp(object sender, KeyRoutedEventArgs keyRoutedEventArgs)
        {
            TextChanged?.Invoke(this, this.Text);
        }

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs args)
        {
            this.Text = this.TextBox.Text;
            TextChanged?.Invoke(this, this.Text);
        }

        #endregion

        #region Settings

        public Windows.UI.Color ButtonBg
        {
            set
            {
                RecordButton.Background = new SolidColorBrush(value);
                InkButton.Background = new SolidColorBrush(value);
            }
        }

        public bool LeftJustified
        {
            set
            {
                if (value)
                {
                    Grid.SetColumn(ButtonMenu, 1);
                    Grid.SetColumn(Input, 0);
                    MainGrid.ColumnDefinitions[1].Width = new GridLength(85);
                }
                else
                {
                    Grid.SetColumn(ButtonMenu, 0);
                    Grid.SetColumn(Input, 1);
                    MainGrid.ColumnDefinitions[0].Width = new GridLength(85);

                }
            }
        }

        public double SetHeight
        {
            set
            {
                MainGrid.Height = value;
                TextBox.Height = value;
                TextBox.FontSize = value - 15;
                RecordButton.Height = value;
                RecordImg.Height = value - 10;
                RecordImg.Width = value - 10;
                InkButton.Height = value;
                InkImg.Height = value - 10;
                InkImg.Width = value - 10;
            }
        }

        public bool BubbleTop
        {
            set
            {
                if (value)
                {
                    TranslateTransform top = new TranslateTransform();
                    top.X = -40;
                    top.Y = -80;
                    InkBubble.RenderTransform = top;
                    SetImage("ms-appx:///Assets/menububble2.png", bubble);

                }
                else
                {
                    TranslateTransform bottom = new TranslateTransform();
                    bottom.X = -40;
                    bottom.Y = 50;
                    InkBubble.RenderTransform = bottom;
                    Thickness m = Inker.Margin;
                    m.Top = 3;
                    Inker.Margin = m;
                    SetImage("ms-appx:///Assets/menububblebtm.png", bubble);

                }
            }
        }

        #endregion

        #region Speech to Text

        private async void RecordButton_OnClick(object sender, PointerRoutedEventArgs e)
        {
            RecordModeOn();
            SetImage("ms-appx:///Assets/icon_audionode_record.png", RecordImg);

            var session = SessionController.Instance;
            if (!session.IsRecording)
            {
                _input =  session.TranscribeVoice();
                //if (session.SpeechString != null)
                //{
                //    TextBox.Text = session.SpeechString;
                //}
            }
        }

        private async void RecordButton_Released(object sender, PointerRoutedEventArgs e)
        {
            TextBox.Text = await _input;
            TextChanged?.Invoke(this, this.Text);
            SetImage("ms-appx:///Assets/node icons/record.png", RecordImg);

        }

        #endregion

        #region Ink to Text

        private void SetUpInking()
        {
            _savedForInking = _text;
            var inqModel = new InqCanvasModel(SessionController.Instance.GenerateId());
            var inqViewModel = new InqCanvasViewModel(inqModel, new Size(Inker.Width, Inker.Height));

            _inqView = new InqCanvasView(inqViewModel);
            _inqView.IsEnabled = true;

            ResetInkingCanvas();
            TextBox.Width = Width - 100;
            List<InqLineModel> _lines = new List<InqLineModel>();

            inqModel.LineFinalizedLocally += async delegate (InqLineModel model)
            {
                var nm = model.GetScaled(Constants.MaxCanvasSize);
                _lines.Add(nm);
                var texts = await InkToText(_lines);
                if (texts.Count > 0)
                {
                    TextBox.Text = _savedForInking + " " + texts[0];
                    this.Text = this.TextBox.Text.Trim();
                }
                TextChanged?.Invoke(this, TextBox.Text.Trim());
            };
        }


        private void ResetInkingCanvas()
        {
            Inker.Children.Clear();
            Windows.UI.Xaml.Shapes.Rectangle curr = new Windows.UI.Xaml.Shapes.Rectangle();
            Windows.UI.Xaml.Shapes.Rectangle marker = new Windows.UI.Xaml.Shapes.Rectangle();
            curr.Opacity = 0.5;
            marker.Opacity = 0.1;
            marker.Height = Inker.Height;
            marker.Width = 30;
            marker.HorizontalAlignment = HorizontalAlignment.Left;
            curr.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(1, 242, 242, 242));
            marker.Stroke = new SolidColorBrush(Colors.LightSlateGray);

            marker.Fill = new SolidColorBrush(Windows.UI.Color.FromArgb(1, 242, 242, 242));
            Inker.Children.Add(curr);
            Inker.Children.Add(_inqView);
            Inker.Children.Add(marker);
            marker.PointerPressed += InkerClick;
        }

        private void InkerClick(Object sender, PointerRoutedEventArgs e)
        {
            SetUpInking();
            e.Handled = false;
            _inqView.OnPointerPressed(sender, e);
        }

        private void InkButton_OnClick(object sender, RoutedEventArgs e)
        {
            InkModeOn();
            TextBox.Visibility = Visibility.Visible;
            if (!_isInking)
            {
                InkBubble.Visibility = Visibility.Visible;
                SetUpInking();
                SetImage("ms-appx:///Assets/icon_whitex.png", InkImg);
            }
            else
            {
                InkBubble.Visibility = Visibility.Collapsed;
                this.Text = this.TextBox.Text.Trim();
                TextChanged?.Invoke(this, TextBox.Text.Trim());
                SetImage("ms-appx:///Assets/icon_node_ink.png", InkImg);
            }
            _isInking = !_isInking;
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

        #endregion

        #region Modes
        public bool IsActivated
        {
            get { return _isActivated; }
            set { _isActivated = value; }
        }

        public void Activate()
        {
            IsActivated = true;
            ButtonMenu.Visibility = Visibility.Visible;
        }

        public void DeActivate()
        {
            IsActivated = false;
            ButtonMenu.Visibility = Visibility.Collapsed;

        }

        public void RecordModeOn()
        {
            _recordMode = true;
            _inkMode = false;
        }

        public void InkModeOn()
        {
            _recordMode = false;
            _inkMode = true;
        }

        public void SetImage(String url, Image buttonName)
        {
            Uri imageUri = new Uri(url, UriKind.Absolute);
            BitmapImage imageBitmap = new BitmapImage(imageUri);
            buttonName.Source = imageBitmap;
        }
        #endregion

    }
}