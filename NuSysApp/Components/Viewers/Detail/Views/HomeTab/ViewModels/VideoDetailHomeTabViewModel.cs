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
        
        public VideoDetailHomeTabViewModel(LibraryElementController controller) :  base(controller)
        {
            LibraryElementController = controller;
        }


        public void VideoMediaPlayer_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public override void AddRegion(object sender, RegionLibraryElementController libraryElementController)
        {

        }
        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            //double position = e.NewValue / VideoDuration;
            //foreach (var regionview in RegionViews)
            //{
            //    if (((regionview.DataContext as VideoRegionViewModel).Model as VideoRegionModel).Start <= position &&
            //        ((regionview.DataContext as VideoRegionViewModel).Model as VideoRegionModel).End >= position)
            //    {
            //        regionview.RegionRectangle.Visibility = Visibility.Visible;
            //        regionview.Select();
            //    }
            //    else
            //    {
            //        regionview.RegionRectangle.Visibility = Visibility.Collapsed;
            //        regionview.Deselect();
            //    }
            //}
        }

        public double VideoDuration { get; set; }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {

        }

        public override void SizeChanged(object sender, double width, double height)
        {
            width = View.ActualWidth;
            height = View.ActualHeight;

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
            if (LibraryElementController is VideoRegionLibraryElementController)
            {
                var region = LibraryElementController.LibraryElementModel as VideoRegionModel;
                m["start"] = region.Start + (region.End - region.Start) * 0.25;
                m["end"] = region.Start + (region.End - region.Start) * 0.75;
            }

            return m;
        }
    }
}
