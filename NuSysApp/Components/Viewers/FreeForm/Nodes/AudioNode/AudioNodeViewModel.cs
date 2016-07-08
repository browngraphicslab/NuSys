using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Nodes.AudioNode;
using Windows.Storage;
using Windows.Storage.Streams;
using NAudio;
using NAudio.Wave;
using System.Net;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Linq;
using Windows.UI.Xaml.Controls.Primitives;

namespace NuSysApp
{
    public class AudioNodeViewModel: ElementViewModel, Sizeable
    {
        private Grid _visualGrid;

        public delegate void BlockHitEventHandler(LinkedTimeBlockViewModel timeBlock);
        public event BlockHitEventHandler OnBlockHitEventHandler;

        public delegate void BlockLeaveEventHandler(LinkedTimeBlockViewModel timeBlock);
        public event BlockHitEventHandler OnBlockLeaveEventHandler;

        public delegate void VisualizationLoadedEventHandler();
        public event VisualizationLoadedEventHandler OnVisualizationLoaded;
        public delegate void OnRegionSeekPassingHandler(double time);
        public event OnRegionSeekPassingHandler OnRegionSeekPassing;
        public double AudioDuration { set; get; }
        public ObservableCollection<AudioRegionView> Regions { private set; get; }
        public AudioNodeViewModel(ElementController controller) : base(controller)
        {
            Width = controller.Model.Width;
            Height = controller.Model.Height;
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            Regions = new ObservableCollection<AudioRegionView>();

            this.CreateAudioRegionViews();

            controller.Disposed += ControllerOnDisposed;
            Controller.LibraryElementController.RegionAdded += LibraryElementControllerOnRegionAdded;
            Controller.LibraryElementController.RegionRemoved += LibraryElementControllerOnRegionRemoved;
            Controller.SizeChanged += Controller_SizeChanged;
            Controller.LibraryElementController.Loaded += LibraryElementController_Loaded;
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            foreach (var rv in Regions)
            {
                var regionViewViewModel = rv.DataContext as AudioRegionViewModel;
                regionViewViewModel?.ChangeSize(this, width, height);
            }
        }

        private void LibraryElementControllerOnRegionRemoved(object source, Region region)
        {
            var audioRegion = region as TimeRegionModel;
            if (audioRegion == null)
            {
                return;
            }

            foreach (var regionView in Regions.ToList<AudioRegionView>())
            {
                if ((regionView.DataContext as AudioRegionViewModel).Model.Id == audioRegion.Id)
                    Regions.Remove(regionView);
            }


            RaisePropertyChanged("Regions");
        }

        private void LibraryElementController_Loaded(object sender)
        {
            this.CreateAudioRegionViews();
        }


        public void CreateAudioRegionViews()
        {
            var elementController = Controller.LibraryElementController;
            var regionHashSet = elementController.LibraryElementModel.Regions;

            if (regionHashSet == null)
            {
                return;
            }

            Regions.Clear();

            foreach (var model in regionHashSet)
            {
                var audioModel = model as TimeRegionModel;
                AudioRegionController regionController;

                if (SessionController.Instance.RegionsController.GetRegionController(audioModel.Id) == null)
                {
                    var factory = new RegionControllerFactory();
                    regionController = factory.CreateFromSendable(audioModel, ContentId) as AudioRegionController;
                }
                else {
                    regionController = SessionController.Instance.RegionsController.GetRegionController(audioModel.Id) as AudioRegionController;
                }

                var viewmodel = new AudioRegionViewModel(audioModel, elementController, regionController, this);
                viewmodel.Editable = false;
                var view = new AudioRegionView(viewmodel);
                view.OnRegionSeek += View_OnRegionSeek;

                Regions.Add(view);


            }
            RaisePropertyChanged("Regions");
        }





        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double position = e.NewValue / AudioDuration;
            foreach (var regionview in Regions)
            {
                if (((regionview.DataContext as AudioRegionViewModel).Model as TimeRegionModel).Start <= position &&
                    ((regionview.DataContext as AudioRegionViewModel).Model as TimeRegionModel).End >= position)
                {
                    regionview.Select();
                }
                else
                {
                    regionview.Deselect();

                }
            }
        }

        private void ControllerOnDisposed(object source)
        {
            Controller.LibraryElementController.Loaded -= InitWhenReady;
            Controller.Disposed -= ControllerOnDisposed;
        }

        public void removeTimeBlockChange(
                System.Collections.Specialized.NotifyCollectionChangedEventHandler onCollectionChanged)
        {
            (Model as AudioNodeModel).LinkedTimeModels.CollectionChanged -= onCollectionChanged;
        }

        public void addTimeBlockChange(
            System.Collections.Specialized.NotifyCollectionChangedEventHandler onCollectionChanged)
        {
            (Model as AudioNodeModel).LinkedTimeModels.CollectionChanged += onCollectionChanged;
        }




        public override void SetSize(double width, double height)
        {
            var model = Model as AudioNodeModel;
            if (height < 200)
            {
                height = 200;
            }
            if (width < 150)
            {
                width = 150;
            }
            base.SetSize(width, height);
            RaisePropertyChanged("Regions");
        }

        public Uri AudioSource
        {
            get
            {
                return Controller.LibraryElementController.GetSource();
            }
        }
        /*
        public ObservableCollection<AudioRegionView> Regions { get
            {
                var collection = new ObservableCollection<AudioRegionView>();
                var elementController = Controller.LibraryElementController;
                var regionHashSet = elementController.LibraryElementModel.Regions;

                if (regionHashSet == null)
                    return collection;
                
                foreach (var model in regionHashSet)
                {
                    var regionController = new RegionController(model);
                    regionController.RegionUpdated += LibraryElementControllerOnRegionUpdated;
                    var viewmodel = new AudioRegionViewModel(model as TimeRegionModel, elementController, regionController,this);
                    viewmodel.Editable = false;
                    var view = new AudioRegionView(viewmodel);
                    view.OnRegionSeek += View_OnRegionSeek;
                    collection.Add(view);
                }   
                return collection;
            }
        }
        */

        private void View_OnRegionSeek(double time)
        {
            OnRegionSeekPassing?.Invoke(time);
        }

        public override async Task Init()
        {
            if (SessionController.Instance.ContentController.ContainsAndLoaded(ContentId))
            {
                InitWhenReady(this);
            }
            else
            {
                Controller.LibraryElementController.Loaded += InitWhenReady;
            }
        }

        private async void InitWhenReady(object sender)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(AudioSource);
            HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync();
            Stream resStream = response.GetResponseStream();

            byte[] dataBytes = new byte[(int)response.ContentLength];
            resStream.Read(dataBytes, 0, (int)response.ContentLength);
            resStream.Dispose();
            //Visualize(dataBytes);
        }
        private void LibraryElementControllerOnRegionAdded(object source, RegionController regionController)
        {
            var audioRegionController = regionController as AudioRegionController;
            var audioRegion = audioRegionController?.Model as TimeRegionModel;
            if (audioRegion == null)
            {
                return;
            }
            var vm = new AudioRegionViewModel(audioRegion, Controller.LibraryElementController, audioRegionController, this);
            var view = new AudioRegionView(vm);
            vm.Editable = false;
            Regions.Add(view);
            RaisePropertyChanged("Regions");
        }

        private void LibraryElementControllerOnRegionUpdated(object source, Region region)
        {
            /*
            var imageRegion = region as RectangleRegion;
            if (imageRegion == null)
            {
                return;
            }

            foreach (var regionView in Regions.ToList<AudioRegionView>())
            {
                if ((regionView.DataContext as ImageRegionViewModel).Model == imageRegion)
                    Regions.Remove(regionView);
            }
            */
            

            RaisePropertyChanged("Regions");        }

        private async void Visualize(byte[] bytes)
        {
            MemoryStream s = new MemoryStream(bytes);
            var stream = s.AsRandomAccessStream();


            WaveStream waveStream = new MediaFoundationReaderUniversal(stream);
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

        public void AddTimeRegion(TimeRegionModel region)
        {
            Controller.LibraryElementController.AddRegion(region);   
        }

        public double GetWidth()
        {
            return Width;
        }

        public double GetHeight()
        {
            return Height;
        }

        public double GetViewWidth()
        {
            throw new NotImplementedException();
        }

        public double GetViewHeight()
        {
            throw new NotImplementedException();
        }
    }
}
