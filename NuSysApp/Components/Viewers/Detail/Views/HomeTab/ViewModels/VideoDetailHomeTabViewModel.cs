using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace NuSysApp
{
    public class VideoDetailHomeTabViewModel : DetailHomeTabViewModel, Sizeable
    {
        public delegate void OnRegionSeekPassingHandler(double time);
        public event OnRegionSeekPassingHandler OnRegionSeekPassing;
        public LibraryElementController Controller { get; }
        public ObservableCollection<VideoRegionView> RegionViews { set; get; }
        
        public VideoDetailHomeTabViewModel(LibraryElementController controller, HashSet<Region> regionsToLoad) :  base(controller, regionsToLoad)
        {
            Controller = controller;
            RegionViews = new ObservableCollection<VideoRegionView>();
            Controller.Loaded += Controller_Loaded;
            
        }

        private void Controller_Loaded(object sender)
        {
            RegionViews.Clear();
            SetExistingRegions();
            //RaisePropertyChanged("RegionViews");
        }
        public void VideoMediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            RegionViews.Clear();
            SetExistingRegions();
        }

        public override void AddRegion(object sender, RegionController controller)
        {
            var videoRegion = controller?.Model as VideoRegionModel;
            var videoRegionController = controller as VideoRegionController;
            if (videoRegion == null || videoRegionController == null)
            {
                return;
            }
            var vm = new VideoRegionViewModel(videoRegion, Controller, videoRegionController, this);
            vm.Editable = this.Editable;
            var view = new VideoRegionView(vm);
            RegionViews.Add(view);
            view.OnRegionSeek += View_OnRegionSeek;
            RaisePropertyChanged("RegionViews");
        }
        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double position = e.NewValue / VideoDuration;
            foreach (var regionview in RegionViews)
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

        public double VideoDuration { get; set; }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            var videoRegion = displayedRegion as VideoRegionModel;
            if (videoRegion == null)
            {
                return;
            }

            foreach (var regionView in RegionViews.ToList<VideoRegionView>())
            {
                if ((regionView.DataContext as VideoRegionViewModel).Model.Id == videoRegion.Id)
                    RegionViews.Remove(regionView);
            }

            RaisePropertyChanged("RegionViews");
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            width = View.ActualWidth;
            height = View.ActualHeight;
            foreach (var rv in RegionViews)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(sender,width,height);
            }
        }

        public double GetWidth()
        {
            return (View as VideoDetailHomeTabView).VideoWidth;
        }

        public double GetHeight()
        {
            return (View as VideoDetailHomeTabView).VideoHeight;
        }
        public void MediaPlayerOnLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            SetExistingRegions();
        }

        public override void SetExistingRegions()
        {
            foreach (var regionModel in _regionsToLoad)
            {
                var videoRegion = regionModel as VideoRegionModel;
                if (videoRegion == null)
                {
                    return;
                }
                VideoRegionController regionController;
                if (SessionController.Instance.RegionsController.GetRegionController(regionModel.Id) == null)
                {
                    regionController = SessionController.Instance.RegionsController.AddRegion(regionModel, Controller.LibraryElementModel.LibraryElementId) as VideoRegionController;
                }
                else
                {
                    regionController = SessionController.Instance.RegionsController.GetRegionController(regionModel.Id) as VideoRegionController;
                }
                Debug.Assert(regionController is VideoRegionController);
                var vm = new VideoRegionViewModel(videoRegion, Controller, regionController as VideoRegionController, this);
                vm.Editable = this.Editable;
                var view = new VideoRegionView(vm);
                view.OnRegionSeek += View_OnRegionSeek;
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");
        }

        private void View_OnRegionSeek(double time)
        {
            OnRegionSeekPassing?.Invoke(time);
        }

        public override Region GetNewRegion()
        {
            var region = new VideoRegionModel(new Point(.25,.25), new Point(.75, .75) );
            return region;
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
