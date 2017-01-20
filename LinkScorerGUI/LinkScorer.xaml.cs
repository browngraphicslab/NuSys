using ParserHelper;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace LinkScorerGUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SiteScore _score;
        private bool _alreadySaved;
        private List<List<DataHolder>> _models;
        private NaiveBayesClassifier classifier;
        private double count = 0;
        private double countright = 0;
        private double prediction = -1;
        private string currenturl;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void Button_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            _models = await HtmlImporter.RunWithSearch(url.Text);
            RenderNewTest();
        }

        private async void RenderNewTest()
        {
            _alreadySaved = false;
            xItems.Items.Clear();

            ScoreDisplay.Text = //"TextImageRatio: " + _score.TextImageRatio + "\nAverageImageSize: " +
                //_score.AverageImageSize +
                //"\nHeaderTextRatio: " + _score.HeaderTextRatio + "\navgTextBlockSize: " +
                //_score.AverageTextBlockSize + "\nprediction: " +
                /*prediction + " with: " + classifier.LastProbability+"\nAccuracy: "+(countright/(count==0?1:count)*100)+"%\n"+*/
                _models.First().First().Title;


            foreach (var dh in _models[1])
            {
                var stack = new StackPanel();
                //stack.Width = 500;

                var cap = new TextBlock { Text = dh.Title ?? "", FontSize = 20 };
                switch (dh.DataType)
                {
                    case DataType.Text:
                        var tb = new TextBlock
                        {
                            Text = (dh as TextDataHolder).Text,
                            TextWrapping = TextWrapping.WrapWholeWords
                        };
                        var links = new TextBlock();
                        foreach (var link in (dh as TextDataHolder).links)
                        {
                            links.Text += link + "\n";
                        }
                        stack.Children.Add(tb);
                        stack.Children.Add(links);
                        break;
                    case DataType.Image:
                        var im = new BitmapImage() { UriSource = (dh as ImageDataHolder).Uri };
                        stack.Children.Add(new Image() { Source = im, Stretch = Stretch.Uniform, Height = 150 });
                        cap.Text += "\n" + (dh as ImageDataHolder).Uri.AbsoluteUri;
                        break;
                    case DataType.Video:
                        var me = new MediaElement();
                        me.Source = (dh as VideoDataHolder).Uri;
                        me.Play();
                        stack.Children.Add(me);
                        cap.Text += "\n" + "video";
                        break;
                    case DataType.Audio:
                        var mea = new MediaElement();
                        mea.Source = (dh as AudioDataHolder).Uri;
                        mea.Play();
                        cap.Text += "\n" + "audio";
                        stack.Children.Add(mea);
                        break;
                    case DataType.Pdf:
                        var capu = new TextBlock { Text = (dh as PdfDataHolder).Uri.OriginalString };
                        stack.Children.Add(capu);
                        break;
                }
                stack.Children.Add(cap);
                stack.DataContext = dh;
                xItems.Items.Add(stack);
            }
            currenturl = _models.First().First().Title;
            _models.First().Remove(_models.First().First());
            _models.Remove(_models[1]);
        }

        private void GoodButtonOnTapped(object sender, TappedRoutedEventArgs e)
        {
            count++;
            if (prediction == 1)
                countright++;
            if (_alreadySaved)
            {
                return;
            }
            if (_score == null)
            {
                if (_models[0].Count != 0)
                {
                    RenderNewTest();
                    return;
                }
                xItems.Items.Clear();
                ScoreDisplay.Text = "";
                return;
            }
            _score.Score = 1;
            saveScore();
        }

        private void BadButtonOnTapped(object sender, TappedRoutedEventArgs e)
        {
            count++;
            if (prediction == 0)
                countright++;
            if (_alreadySaved)
            {
                return;
            }
            if (_score == null)
            {
                if (_models[0].Count == 0)
                {
                    xItems.Items.Clear();
                    ScoreDisplay.Text = "";
                    return;
                }
                RenderNewTest();
                return;
            }
            _score.Score = 0;
            saveScore();
        }

        private async void saveScore()
        {
            string path = @"C:\Users\Sahil Mishra\Pictures\PARSERDATASITES.txt";
            string pathalreadydone = @"C:\Users\Sahil Mishra\Pictures\PARSERURLANDSCORE.txt";
            /*await Task.Run(() =>
            {
                if (!File.Exists(path))
                {
                    File.Create(path);

                }
                File.AppendAllText(path, _score.ToString() + Environment.NewLine);
                if (!File.Exists(pathalreadydone))
                {
                    File.Create(pathalreadydone);

                }
                File.AppendAllText(pathalreadydone, currenturl+","+_score.Score + Environment.NewLine);
            });*/
            if (_models[0].Count == 0)
            {
                xItems.Items.Clear();
                ScoreDisplay.Text = "";
                return;
            }
            RenderNewTest();


        }


        private async void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
        {
            classifier = new NaiveBayesClassifier();
            string path = @"C:\Users\Sahil Mishra\Pictures\PARSERDATASITES.txt";
            List<string> lines = null;
            await Task.Run(() =>
            {
                return lines = System.IO.File.ReadAllLines(path).ToList();
            });
            lines.Remove(lines.Last());
            var data = lines.Select(s => s.Split(',').ToList().Select(r => Convert.ToDouble(r)).ToList()).ToList();
            classifier.Train(data);
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            GoodButtonOnTapped(null,null);
        }

        private void ButtonBase1_OnClick(object sender, RoutedEventArgs e)
        {
            BadButtonOnTapped(null,null);
        }
    }
}