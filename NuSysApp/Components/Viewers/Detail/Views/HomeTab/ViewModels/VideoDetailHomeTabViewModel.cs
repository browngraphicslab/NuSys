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
        public LibraryElementController LibraryElementController { get; }
        public ObservableCollection<VideoRegionView> RegionViews { set; get; }
        
        public VideoDetailHomeTabViewModel(LibraryElementController controller) :  base(controller)
        {
            LibraryElementController = controller;
            RegionViews = new ObservableCollection<VideoRegionView>();            
        }


        public void VideoMediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override void AddRegion(object sender, RegionLibraryElementController libraryElementController)
        {
            var videoRegion = libraryElementController?.LibraryElementModel as VideoRegionModel;
            var videoRegionController = libraryElementController as VideoRegionLibraryElementController;
            if (videoRegion == null || videoRegionController == null)
            {
                return;
            }
            var vm = new VideoRegionViewModel(videoRegion, videoRegionController, this);
            vm.Editable = this.Editable;
            var view = new VideoRegionView(vm);
            RegionViews.Add(view);
            view.OnRegionSeek += OnRegionSeek;
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
                if ((regionView.DataContext as VideoRegionViewModel).Model.LibraryElementId == videoRegion.LibraryElementId)
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

            RegionViews.Clear();
            foreach (var regionId in SessionController.Instance.RegionsController.GetClippingParentRegionLibraryElementIds(LibraryElementController.LibraryElementModel.LibraryElementId))
            {
                var videoRegionController = SessionController.Instance.ContentController.GetLibraryElementController(regionId) as VideoRegionLibraryElementController;
                if (videoRegionController == null)
                {
                    return;
                }
                var vm = new VideoRegionViewModel(videoRegionController.VideoRegionModel, videoRegionController, this);
                vm.Editable = this.Editable;
                var view = new VideoRegionView(vm);
                view.OnRegionSeek += OnRegionSeek;
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");
            
        }

        public void OnRegionSeek(double time)
        {
            OnRegionSeekPassing?.Invoke(time);
        }

        public override Message GetNewRegionMessage()
        {
            var m = new Message();
            m["rectangle_location"] = new Point(.25, .25);
            m["rectangle_width"] = .5;
            m["rectangle_height"] = .5;
            m["start"] = .25;
            m["end"] = .75;
            return m;
        }
    }
}
