using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Nodes.AudioNode;
using NAudio;
using NAudio.Wave;

namespace NuSysApp
{
    public class AudioNodeViewModel: ElementViewModel
    {
        private Grid _visualGrid;
        private IRandomAccessStream _stream;

        public delegate void BlockHitEventHandler(LinkedTimeBlockViewModel timeBlock);
        public event BlockHitEventHandler OnBlockHitEventHandler;

        public delegate void BlockLeaveEventHandler(LinkedTimeBlockViewModel timeBlock);
        public event BlockHitEventHandler OnBlockLeaveEventHandler;

        public double PlaybackElement 
        {
            get { return ((AudioNodeModel)Model).Controller.PlaybackElement.Position.TotalMilliseconds; }
        }


        public delegate void VisualizationLoadedEventHandler();
        public event VisualizationLoadedEventHandler OnVisualizationLoaded;

        public AudioNodeViewModel(ElementController model) : base(model)
        {
            Width = 300;
            Height = 200;
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));

        }

        public void addTimeBlockChange(
            System.Collections.Specialized.NotifyCollectionChangedEventHandler onCollectionChanged)
        {
            (Model as AudioNodeModel).LinkedTimeModels.CollectionChanged += onCollectionChanged;
        }







        public override void SetSize(double width, double height)
        {
            var model = Model as VideoNodeModel;
            if (height < 200)
            {
                height = 200;
            }
            if (width < 150)
            {
                width = 150;
            }
            base.SetSize(width, height);
        }

        public IRandomAccessStream AudioSource
        {
            get { return _stream; }
        }
        public override async Task Init()
        {
            if (SessionController.Instance.ContentController.ContainsAndLoaded(ContentId))
            {
                InitWhenReady();
            }
            else
            {
                Controller.ContentLoaded += InitWhenReady;
            }
        }

        private void InitWhenReady(object source = null, LibraryElementModel data = null)
        {
            var byteArray = Convert.FromBase64String(SessionController.Instance.ContentController.Get(ContentId).Data);
            MemoryStream s = new MemoryStream(byteArray);
            _stream = s.AsRandomAccessStream();
            Visualize();
        }

        private async void Visualize()
        {
            WaveStream waveStream = new MediaFoundationReaderUniversal(_stream);
            int bytesPerSample = (waveStream.WaveFormat.BitsPerSample / 8) * waveStream.WaveFormat.Channels;
            waveStream.Position = 0;
            int bytesRead = 1;
            int samplesPerPixel = 1024;

            if (waveStream.TotalTime.TotalMinutes > 15)
            {
                samplesPerPixel = 65536;
            }
            else if (waveStream.TotalTime.TotalMinutes > 8)
            {
                samplesPerPixel = 32768;
            }
            else if (waveStream.TotalTime.TotalMinutes > 5)
            {
                samplesPerPixel = 16384;
            }
            else if (waveStream.TotalTime.TotalMinutes > 3)
            {
                samplesPerPixel = 8192;
            }
            else if (waveStream.TotalTime.TotalMinutes > 0.5)
            {
                samplesPerPixel = 2048;
            }

            byte[] waveData = new byte[samplesPerPixel * bytesPerSample];
            _visualGrid = new Grid();
            float x = 0;
            while (bytesRead != 0)
            {
                short low = 0;
                short high = 0;
                bytesRead = waveStream.Read(waveData, 0, samplesPerPixel * bytesPerSample);

                for (int n = 0; n < bytesRead; n += 2)
                {
                    short sample = BitConverter.ToInt16(waveData, n);
                    if (sample < low) low = sample;
                    if (sample > high) high = sample;
                }
                float lowPercent = ((((float)low) - short.MinValue) / ushort.MaxValue);
                float highPercent = ((((float)high) - short.MinValue) / ushort.MaxValue);

                Line line = new Line();
                line.X1 = x;
                line.X2 = x;
                line.Y1 = 100 * (highPercent);
                line.Y2 = 100 * (lowPercent);
                line.Stroke = new SolidColorBrush(Colors.Crimson);
                line.StrokeThickness = 1;
                x++;
                _visualGrid.Children.Add(line);

            }
            _visualGrid.Height = 100;
            _visualGrid.Width = x;
            Line middleLine = new Line();
            middleLine.X1 = 0;
            middleLine.X2 = x;
            middleLine.Y1 = _visualGrid.Height / 2;
            middleLine.Y2 = _visualGrid.Height / 2;

            middleLine.Stroke = new SolidColorBrush(Colors.Crimson);
            middleLine.StrokeThickness = 1;
            _visualGrid.Children.Add(middleLine);

            OnVisualizationLoaded?.Invoke();
        }
        public string FileName
        {
            get { return ((AudioNodeModel)Model).FileName; }
            set { ((AudioNodeModel)Model).FileName = value; }
        }

        public Grid VisualGrid
        {
            get { return _visualGrid; }
        }

        public ObservableCollection<LinkedTimeBlockModel> LinkedTimeModels
        {
            get { return (Model as AudioNodeModel).LinkedTimeModels; }
        }


        public void AddLinkTimeModel(LinkedTimeBlockModel model)
        {
            (Model as AudioNodeModel).LinkedTimeModels.Add(model);
        }
    }
}
