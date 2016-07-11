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
        public ObservableCollection<AudioRegionView> RegionViews { set; get; }
        public delegate void OnRegionSeekPassingHandler(double time);
        public event OnRegionSeekPassingHandler OnRegionSeekPassing;
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
        
        public void ScrubBarOnValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            double position = e.NewValue / Duration;
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
            }
        }
        


        public override void AddRegion(object sender, RegionController controller)
        {
            var audioRegion = controller.Model as TimeRegionModel;
            if (audioRegion == null)
            {
                return;
            }
            var vm = new AudioRegionViewModel(audioRegion, Controller, controller as AudioRegionController, this);
            vm.Editable = this.Editable;
            var view = new AudioRegionView(vm);
            RegionViews.Add(view);
            view.OnRegionSeek += View_OnRegionSeek;
            RaisePropertyChanged("RegionViews");
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            var audioRegion = displayedRegion as TimeRegionModel;
            if (audioRegion == null)
            {
                return;
            }

            foreach (var regionView in RegionViews.ToList<AudioRegionView>())
            {
                if ((regionView.DataContext as AudioRegionViewModel).Model.Id == audioRegion.Id)
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
            return View.ActualWidth;
        }

        public double GetHeight()
        {
            return View.ActualWidth;
        }

        public override void SetExistingRegions()
        {
            if (_regionsToLoad == null)
            {
                return;
            }
            RegionViews.Clear();
            foreach (var regionModel in _regionsToLoad)
            {
                var audioRegion = regionModel as TimeRegionModel;
                if (audioRegion == null)
                {
                    return;
                }
                AudioRegionController regionController;
                if (SessionController.Instance.RegionsController.GetRegionController(audioRegion.Id) == null)
                {
                    //Debug.Fail("Did not load");
                    regionController = SessionController.Instance.RegionsController.AddRegion(audioRegion, Controller.LibraryElementModel.LibraryElementId) as AudioRegionController;
                    }
                else {
                    regionController = SessionController.Instance.RegionsController.GetRegionController(audioRegion.Id) as AudioRegionController;
                }
                var vm = new AudioRegionViewModel(audioRegion, Controller, regionController, this);
                vm.Editable = this.Editable;
                var view = new AudioRegionView(vm);
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
            var region = new TimeRegionModel("Untitled Region", 0.25, 0.75);
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
