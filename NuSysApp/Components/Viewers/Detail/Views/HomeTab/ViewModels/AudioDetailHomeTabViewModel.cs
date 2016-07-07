using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AudioDetailHomeTabViewModel : DetailHomeTabViewModel, Sizeable
    {
        public LibraryElementController Controller { get; }
        public ObservableCollection<Region> Regions;
        public ObservableCollection<AudioRegionView> RegionViews { set; get; }
        
        public AudioDetailHomeTabViewModel(LibraryElementController controller, HashSet<Region> regionsToLoad) : base(controller, regionsToLoad)
        {
            Controller = controller;
            Regions = new ObservableCollection<Region>();
            RegionViews = new ObservableCollection<AudioRegionView>();
            _regionsToLoad = regionsToLoad;
        }
        public void RegionAdded(Region newRegion, AudioDetailHomeTabView contentview)
        {
            //var rectangle = JsonConvert.DeserializeObject<Region>(newRegion.ToString());
           // Regions.Add(newRegion);
            //RegionViews.Add(new AudioRegionView(new AudioRegionViewModel(newRegion as TimeRegionModel, contentview)));
            //RaisePropertyChanged("RegionViews");
        }

        public override void AddRegion(object sender, RegionController controller)
        {
            var AudioRegion = controller.Model as TimeRegionModel;
                if (AudioRegion == null)
            {
                return;
            }
            var vm = new AudioRegionViewModel(AudioRegion, Controller, controller, this);
            vm.Editable = this.Editable;
            var view = new AudioRegionView(vm);
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
            return View.ActualWidth;
        }

        public double GetHeight()
        {
            return View.ActualHeight;
        }

        public override void SetExistingRegions()
        {
            if (_regionsToLoad == null)
            {
                return;
            }
            foreach (var regionModel in _regionsToLoad)
            {
                var AudioRegion = regionModel as TimeRegionModel;
                if (AudioRegion == null)
                {
                    return;
                }
            var regionController = new RegionController(regionModel);
                var vm = new AudioRegionViewModel(AudioRegion, Controller, regionController, this);
                vm.Editable = this.Editable;
                var view = new AudioRegionView(vm);
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");
        }

        public override Region GetNewRegion()
        {
            var region = new TimeRegionModel("name", 0, 1);
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
