using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using NusysIntermediate;
using NuSysApp.Nodes.AudioNode;
using NuSysApp.Util;

namespace NuSysApp
{
    public class VideoNodeViewModel : ElementViewModel, Sizeable
    {
        private ObservableCollection<VideoRegionView> _regionViews = new ObservableCollection<VideoRegionView>();

        public delegate double GetWidthEventHandler(object sender);
        public event GetWidthEventHandler OnGetMediaPlayerWidth;
        public delegate double GetHeightEventHandler(object sender);
        public event GetHeightEventHandler OnGetMediaPlayerHeight;

        public delegate void OnRegionSeekPassingHandler(double time);
        public event OnRegionSeekPassingHandler OnRegionSeekPassing;
        public ObservableCollection<VideoRegionView> RegionViews
        {
            get
            {
                _regionViews.Clear();
                foreach (var regionId in SessionController.Instance.RegionsController.GetClippingParentRegionLibraryElementIds(Controller.LibraryElementModel.LibraryElementId))
                {
                    var videoRegionController = SessionController.Instance.ContentController.GetLibraryElementController(regionId) as VideoRegionLibraryElementController;
                    if (videoRegionController == null)
                    {
                        return _regionViews;
                    }
                    var vm = new VideoRegionViewModel(videoRegionController.VideoRegionModel, videoRegionController, this);
                    vm.Editable = false;
                    var view = new VideoRegionView(vm);
                    view.OnRegionSeek += View_OnRegionSeek;
                    _regionViews.Add(view);

                }
                return _regionViews;
                
            }
        }

        private void View_OnRegionSeek(double time)
        {
            OnRegionSeekPassing?.Invoke(time);
        }

        public double VideoDuration { get; set; }

        public VideoNodeViewModel(ElementController controller) : base(controller)
        {
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            //LibraryElementController.LibraryElementController.RegionUpdated += LibraryElementControllerOnRegionUpdated;
            Controller.SizeChanged += Controller_SizeChanged;
            Controller.LibraryElementController.Loaded += LibraryElementController_Loaded;

        }
        //Eventually, we will refactor video regions to be more like audio/image/pdf so we don' thave to do this.
        public void UpdateRegions()
        {
            RaisePropertyChanged("RegionViews");
        }

        private void LibraryElementController_Loaded(object sender)
        {
            RaisePropertyChanged("RegionViews");
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            RaisePropertyChanged("RegionViews");
        }
       

        public override void Dispose()
        {
            Controller.LibraryElementController.Loaded -= LibraryElementModelOnOnLoaded;
            base.Dispose();
        }
        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double position = e.NewValue/VideoDuration;
            foreach (var regionview in _regionViews)
            {
                if (((regionview.DataContext as VideoRegionViewModel).Model as VideoRegionModel).Start <= position &&
                    ((regionview.DataContext as VideoRegionViewModel).Model as VideoRegionModel).End >= position)
                {
                    regionview.RegionRectangle.Visibility = Visibility.Visible;
                    regionview.Select();
                }
                else
                {
                    regionview.RegionRectangle.Visibility = Visibility.Collapsed;
                    regionview.Deselect();

                }
            }
        }
        public override async Task Init()
        {
            if (Controller.LibraryElementController.IsLoaded)
            {
                Controller.SetSize(Model.Width, Model.Height);
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
        }

        private void LibraryElementModelOnOnLoaded(object sender)
        {
            Controller.SetSize(Model.Width, Model.Height);
        }

        public override void SetSize(double width, double height)
        {
            var model = (VideoNodeModel)Model;
            if (model.ResolutionX < 1)
            {
                return;
            }
            if (width > height)
            {
                var r = model.ResolutionY / (double)model.ResolutionX;
                base.SetSize(width, width * r + 150);
            }
            else
            {

                var r = model.ResolutionX / (double)model.ResolutionY;
                base.SetSize(height * r, height + 150);
            }
        }

        protected override void OnSizeChanged(object source, double width, double height)
        {
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

            SetSize(width, height);
        }
        public double GetWidth()
        {
            var width = OnGetMediaPlayerWidth?.Invoke(this);
            if (width != null)
            {
                return width.Value;
            }
            return 0;
        }

        public double GetHeight()
        {
            var height = OnGetMediaPlayerHeight?.Invoke(this);
            if (height != null)
            {
                return height.Value;
            }
            return 0;
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
