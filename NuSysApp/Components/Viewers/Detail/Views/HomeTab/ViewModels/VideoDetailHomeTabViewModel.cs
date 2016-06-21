using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        }

        public override void AddRegion(object sender, Region region)
        {
            var videoRegion = region as VideoRegionModel;
            if (videoRegion == null)
            {
                return;
            }
            var regionController = new RegionController(region);
            var vm = new VideoRegionViewModel(videoRegion, Controller, regionController, this);
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
        }

        public double GetWidth()
        {
            return View.ActualWidth;
        }

        public double GetHeight()
        {
            return View.ActualHeight;
        }

        public override void SetExistingRegions(HashSet<Region> regions)
        {
            foreach (var regionModel in regions)
            {
                var VideoRegion = regionModel as VideoRegionModel;
                if (VideoRegion == null)
                {
                    return;
                }
                var regionController = new RegionController(regionModel);
                var vm = new VideoRegionViewModel(VideoRegion, Controller, regionController, this);
                var view = new VideoRegionView(vm);
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");
        }
    }
}
