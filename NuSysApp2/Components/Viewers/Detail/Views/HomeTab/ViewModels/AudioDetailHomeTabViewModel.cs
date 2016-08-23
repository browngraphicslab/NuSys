using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;

namespace NuSysApp
{
    public class AudioDetailHomeTabViewModel : DetailHomeTabViewModel, Sizeable
    {
        public double Duration { set; get; }
        public LibraryElementController Controller { get; }
        public ObservableCollection<Region> Regions;
        public delegate void OnRegionSeekPassingHandler(double time);
        public event OnRegionSeekPassingHandler OnRegionSeekPassing;
        public AudioDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Regions = new ObservableCollection<Region>();
     //       _regionsToLoad = regionsToLoad;
            
        }
        public void RegionAdded(Region newRegion, AudioDetailHomeTabView contentview)
        {
            //var rectangle = JsonConvert.DeserializeObject<Region>(newRegion.ToString());
           // Regions.Add(newRegion);
            //RegionViews.Add(new AudioRegionView(new AudioRegionViewModel(newRegion as TimeRegionModel, contentview)));
            //RaisePropertyChanged("RegionViews");
        }
        
        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
           /* double position = e.NewValue / Duration;
            foreach (var regionview in RegionViews)
            {
                if (((regionview.DataContext as AudioRegionViewModel).Model as TimeRegionModel).Start <= position &&
                    ((regionview.DataContext as AudioRegionViewModel).Model as TimeRegionModel).End >= position)
                {
                    //regionview.Visibility = Visibility.Visible;
                    regionview.Select();
                }
                else
                {
                    //regionview.RegionRectangle.Visibility = Visibility.Collapsed;
                    regionview.Deselect();
                }
            }*/
        }
        



        public override void SizeChanged(object sender, double width, double height)
        {
      /*      width = View.ActualWidth;
            height = View.ActualHeight;
            foreach (var rv in RegionViews)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(sender,width,height);
            }*/
        }

        public double GetWidth()
        {
            return View.ActualWidth;
        }

        public double GetHeight()
        {
            return View.ActualWidth;
        }

        private void View_OnRegionSeek(double time)
        {
            OnRegionSeekPassing?.Invoke(time);
        }

        public override Message GetNewRegionMessage()
        {
            Message m = new Message();
            m["title"] = "Untitled Region";
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

        public override void AddRegion(object sender, RegionController regionController)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            throw new NotImplementedException();
        }

        public override void SetExistingRegions()
        {
            throw new NotImplementedException();
        }
    }
}
