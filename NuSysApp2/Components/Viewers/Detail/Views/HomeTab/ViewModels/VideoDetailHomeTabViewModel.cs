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
        
        public VideoDetailHomeTabViewModel(LibraryElementController controller) :  base(controller)
        {
            Controller = controller;
            Controller.Loaded += Controller_Loaded;
            
        }

        private void Controller_Loaded(object sender)
        {
            SetExistingRegions();
            //RaisePropertyChanged("RegionViews");
        }
        public void VideoMediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
            SetExistingRegions();
        }

        public override void AddRegion(object sender, RegionController controller)
        {
            var videoRegion = controller?.LibraryElementModel as VideoRegionModel;
            var videoRegionController = controller as VideoRegionController;
            if (videoRegion == null || videoRegionController == null)
            {
                return;
            }
          //  var vm = new VideoRegionViewModel(videoRegion, Controller, videoRegionController, this);
          //  vm.Editable = this.Editable;
          //  var view = new VideoRegionView(vm);
          //  RegionViews.Add(view);
           // View. += View_OnRegionSeek;
            RaisePropertyChanged("RegionViews");
        }
        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
          /*  double position = e.NewValue / VideoDuration;
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
            }*/
        }

        public double VideoDuration { get; set; }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
        /*    var videoRegion = displayedRegion as VideoRegionModel;
            if (videoRegion == null)
            {
                return;
            }

            foreach (var regionView in RegionViews.ToList<VideoRegionView>())
            {
                if ((regionView.DataContext as VideoRegionViewModel).Model.Id == videoRegion.Id)
                    RegionViews.Remove(regionView);
            }

            RaisePropertyChanged("RegionViews");*/
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            /*width = View.ActualWidth;
            height = View.ActualHeight;
            foreach (var rv in RegionViews)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(sender,width,height);
            }*/
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


        public void View_OnRegionSeek(double time)
        {
            OnRegionSeekPassing?.Invoke(time);
        }

        public override Message GetNewRegionMessage()
        {
            Message m = new Message();
            m["rectangle_top_left_point"] = new Point(.25, .25);
            m["rectangle_width"] = 0.5;
            m["rectangle_height"] = 0.5;
            m["start"] = 0.25;
            m["end"] = 0.75;

            return m;
        }

        public double GetViewWidth()
        {
            throw new NotImplementedException();
        }

        public double GetViewHeight()
        {
            throw new NotImplementedException();
        }

        public override void SetExistingRegions()
        {
            throw new NotImplementedException();
        }
    }
}
