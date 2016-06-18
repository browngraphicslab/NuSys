using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AudioDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public ObservableCollection<Region> Regions;
        public ObservableCollection<AudioRegionView> RegionViews { set; get; }
        public AudioDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Regions = new ObservableCollection<Region>();
            RegionViews = new ObservableCollection<AudioRegionView>();
        }
        public void RegionAdded(Region newRegion, AudioDetailHomeTabView contentview)
        {
            //var rectangle = JsonConvert.DeserializeObject<Region>(newRegion.ToString());
            Regions.Add(newRegion);
            RegionViews.Add(new AudioRegionView(new AudioRegionViewModel(newRegion as TimeRegionModel, contentview)));
            RaisePropertyChanged("RegionViews");
        }

        public override void AddRegion(object sender, Region region)
        {
            throw new NotImplementedException();
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            throw new NotImplementedException();
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            throw new NotImplementedException();
        }
    }
}
