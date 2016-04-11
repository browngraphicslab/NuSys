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
using SharpDX;
using Point = Windows.Foundation.Point;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class TextInputBlock : AnimatableUserControl
    {
        private bool _recordMode;
        private bool _inkMode;
        //private bool _textMode;
        private bool _isRecording;
        private bool _isInking;
        private bool _isActivated;
        private string _savedForInking="";
        private string _text;

        public delegate void TextInputBlockChangedHandler(object source, string title);
        public event TextInputBlockChangedHandler TextChanged;

        public static readonly DependencyProperty LeftJustifiedProperty = DependencyProperty.RegisterAttached("LeftJustified", typeof(bool), typeof(TextInputBlock), null);
        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached("SetHeight", typeof(double), typeof(TextInputBlock), null);
        public static readonly DependencyProperty ButtonBgProperty = DependencyProperty.RegisterAttached("ButtonBg", typeof(Windows.UI.Color), typeof(TextInputBlock), null);
        public static readonly DependencyProperty TextProperty = DependencyProperty.RegisterAttached("Text", typeof(string), typeof(TextInputBlock), null);


        public TextInputBlock()
        {
            _text = "";
           // _textMode = true;
            _recordMode = false;
            _inkMode = false;
            _isRecording = false;
            _isInking = false;
            _isActivated = false;
            this.InitializeComponent();

            TextBox.KeyUp += TextBoxOnKeyUp;
            this.SetUpInking();
        }

        private void TextBoxOnKeyUp(object sender, KeyRoutedEventArgs keyRoutedEventArgs)
        {
            TextChanged?.Invoke(this, this.Text);
        }

        public Windows.UI.Color ButtonBg
        {
            set
            {
               // MenuButton.Background = new SolidColorBrush(value);
               // TextButton.Background = new SolidColorBrush(value);
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

        private void TextBox_OnTextChanged(object sender, TextChangedEventArgs args)
        {
            this.Text = this.TextBox.Text;
        }

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

        public double SetHeight
        {
            set
            {
                MainGrid.Height = value;
                TextBox.Height = value;
                TextBox.FontSize = value - 15;
                //MenuButton.Height = value;
                //ButtonImg.Height = value - 15;
                //ButtonImg.Width = value - 15;
                //TextButton.Height = value;
                //TextImg.Height = value - 10;
                //TextImg.Width = value - 10;
                RecordButton.Height = value;
                RecordImg.Height = value - 10;
                RecordImg.Width = value - 10;
                InkButton.Height = value;
                InkImg.Height = value - 10;
                InkImg.Width = value - 10;
            }
        }

        private void SetUpInking()
        {
            var inqModel = new InqCanvasModel(SessionController.Instance.GenerateId());
            var inqViewModel = new InqCanvasViewModel(inqModel, new Size(Inker.Width, Inker.Height));

            var inqView = new InqCanvasView(inqViewModel);
            inqView.IsEnabled = true;
            Inker.Children.Clear();
            Windows.UI.Xaml.Shapes.Rectangle r = new Windows.UI.Xaml.Shapes.Rectangle();
            Inker.Children.Add(r);
            Inker.Children.Add(inqView);

            TextBox.Width = Width - 100;
            List<InqLineModel> _lines = new List<InqLineModel>();

            inqModel.LineFinalizedLocally += async delegate (InqLineModel model)
            {
                var nm = model.GetScaled(Constants.MaxCanvasSize);

                _lines.Add(nm);

                var texts = await InkToText(_lines);
                if (texts.Count > 0)
                    TextBox.Text=_savedForInking + " " + texts[0];
            };
        }

        //private void MenuButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    if (!IsActivated)
        //        return;
        //    if (_inkMode)
        //    {
        //        if (!_isInking)
        //        {
        //            InkBubble.Visibility = Visibility.Visible;
        //            SetUpInking();
        //            FlipOpen.Begin();
        //        }
        //        else
        //        {
        //            InkBubble.Visibility = Visibility.Collapsed;

        //            TextBox.Text = inkText;
        //            TextChanged?.Invoke(this, TextBox.Text);
        //            inkText = "";
        //           ResetTextFromInk();
        //        }
        //        _isInking = !_isInking;
        //    }
        //    else if (InputMenu.Visibility == Visibility.Collapsed)
        //    {
        //        InputMenu.Visibility = Visibility.Visible;
        //    }
        //    else
        //    {
        //        InputMenu.Visibility = Visibility.Collapsed;
        //    }
        //}

        private async void RecordButton_OnClick(object sender, RoutedEventArgs e)
        {
            //record functionality
            RecordModeOn();
            //InputMenu.Visibility = Visibility.Collapsed;
           // this.SetButton("ms-appx:///Assets/icon_audionode_record.png");
            var session = SessionController.Instance;
            if (!session.IsRecording)
            {
                await session.TranscribeVoice();
                if (session.SpeechString != null)
                {
                    TextBox.Text = session.SpeechString;
                }
            }
           // TextModeOn();
           // this.SetButton("ms-appx:///Assets/icon_node_text.png");
        }

        //private void ResetTextFromInk()
        //{
           // FlipClose.Begin();
            //TextModeOn();
           // this.SetButton("ms-appx:///Assets/icon_node_text.png");
       // }

        private void InkButton_OnClick(object sender, RoutedEventArgs e)
        {
            //ink functionality
            InkModeOn();
            _savedForInking = _text;
            //InputMenu.Visibility = Visibility.Collapsed;
            TextBox.Visibility = Visibility.Visible;
            //this.SetButton("ms-appx:///Assets/icon_node_ink.png");
            if (!_isInking)
            {
                InkBubble.Visibility = Visibility.Visible;
                SetUpInking();
               // FlipOpen.Begin();
            }
            else
            {
                InkBubble.Visibility = Visibility.Collapsed;

            }
            _isInking = !_isInking;
        }

        //private void TextButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    //inkText default functionality
        //    TextModeOn();
        //    InputMenu.Visibility = Visibility.Collapsed;
        //    this.SetButton("ms-appx:///Assets/icon_node_text.png");
        //    TextBox.Visibility = Visibility.Visible;
        //}

        public bool IsActivated
        {
            get { return _isActivated; }
            set { _isActivated = value; }
        }

        public void Activate()
        {
            IsActivated = true;
            //InputMenu.Visibility = Visibility.Visible;
            ButtonMenu.Visibility = Visibility.Visible;
        }

        public void DeActivate()
        {
            IsActivated = false;
            //InputMenu.Visibility = Visibility.Collapsed;
            ButtonMenu.Visibility = Visibility.Collapsed;

        }

        //public void TextModeOn()
        //{
        // _textMode = true;
        //_recordMode = false;
        //_inkMode = false;
        // TextButton.Visibility = Visibility.Collapsed;
        //    InkButton.Visibility = Visibility.Visible;
        //    RecordButton.Visibility = Visibility.Visible;
        //}

        public void RecordModeOn()
        {
           // _textMode = false;
            _recordMode = true;
            _inkMode = false;
           // TextButton.Visibility = Visibility.Visible;
            //InkButton.Visibility = Visibility.Visible;
            //RecordButton.Visibility = Visibility.Collapsed;
        }

        public void InkModeOn()
        {
           // _textMode = false;
            _recordMode = false;
            _inkMode = true;
           // TextButton.Visibility = Visibility.Visible;
        //    InkButton.Visibility = Visibility.Collapsed;
        //    RecordButton.Visibility = Visibility.Visible;
        }

        //public void SetButton(String url)
        //{
        //    Uri imageUri = new Uri(url, UriKind.Absolute);
        //    BitmapImage imageBitmap = new BitmapImage(imageUri);
            //ButtonImg.Source = imageBitmap;
       // }


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