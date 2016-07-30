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
        public LibraryElementController LibraryElementController { get; }

        public delegate void OnRegionSeekPassingHandler(double time);
        public event OnRegionSeekPassingHandler OnRegionSeekPassing;
        public AudioDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            
        }
        public void RegionAdded(Region newRegion, AudioDetailHomeTabView contentview)
        {
            //var rectangle = JsonConvert.DeserializeObject<Region>(newRegion.ToString());
           // Regions.Add(newRegion);
            //RegionViews.Add(new AudioRegionView(new AudioRegionViewModel(newRegion as AudioRegionModel, contentview)));
            //RaisePropertyChanged("RegionViews");
        }
        
        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double position = e.NewValue / Duration;
          /*  foreach (var regionview in RegionViews)
            {
                if (((regionview.DataContext as AudioRegionViewModel).Model as AudioRegionModel).Start <= position &&
                    ((regionview.DataContext as AudioRegionViewModel).Model as AudioRegionModel).End >= position)
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
        


        public override void AddRegion(object sender, RegionLibraryElementController libraryElementController)
        {
        }

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
            return View.ActualWidth;
        }

        public double GetHeight()
        {
            return View.ActualWidth;
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
            m["start"] = .25;
            m["end"] = .75;
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
    }
}
