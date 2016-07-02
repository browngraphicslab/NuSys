using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class VideoDetailHomeTabViewModel : DetailHomeTabViewModel, Sizeable
    {
        public LibraryElementController Controller { get; }
        public ObservableCollection<VideoRegionView> RegionViews { set; get; }
        public VideoDetailHomeTabViewModel(LibraryElementController controller) :  base(controller)
        {
            Controller = controller;
            RegionViews = new ObservableCollection<VideoRegionView>();
            Controller.Loaded += Controller_Loaded;
            
        }

        private void Controller_Loaded(object sender)
        {
            RaisePropertyChanged("RegionViews");
        }
        public void VideoMediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            RegionViews.Clear();
            SetExistingRegions(Controller.LibraryElementModel.Regions);
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
            var view = new VideoRegionView(vm);
            RegionViews.Add(view);
            RaisePropertyChanged("RegionViews");
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            throw new NotImplementedException();
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

        }

        public override void SetExistingRegions(HashSet<Region> regions)
        {
            foreach (var regionModel in regions)
            {
                var videoRegion = regionModel as VideoRegionModel;
                if (videoRegion == null)
                {
                    return;
                }
                var regionController = new VideoRegionController(videoRegion);
                var vm = new VideoRegionViewModel(videoRegion, Controller, regionController, this);
                var view = new VideoRegionView(vm);
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");
        }

        public override Region GetNewRegion()
        {
            var region = new VideoRegionModel(new Point(.25,.25), new Point(.75, .75) );
            return region;
        }
    }
}
