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
        public AudioDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Regions = new ObservableCollection<Region>();
            RegionViews = new ObservableCollection<AudioRegionView>();
            
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
            foreach (var regionview in RegionViews)
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
            }
        }
        


        public override void AddRegion(object sender, RegionLibraryElementController libraryElementController)
        {
            var audioRegion = libraryElementController.LibraryElementModel as AudioRegionModel;
            if (audioRegion == null)
            {
                return;
            }
            var vm = new AudioRegionViewModel(audioRegion, libraryElementController as AudioRegionLibraryElementController, this);
            vm.Editable = this.Editable;
            var view = new AudioRegionView(vm);
            RegionViews.Add(view);
            view.OnRegionSeek += OnRegionSeek;
            RaisePropertyChanged("RegionViews");
        }

        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            var audioRegion = displayedRegion as AudioRegionModel;
            if (audioRegion == null)
            {
                return;
            }

            foreach (var regionView in RegionViews.ToList<AudioRegionView>())
            {
                if ((regionView.DataContext as AudioRegionViewModel)?.Model.LibraryElementId == audioRegion.LibraryElementId)
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

            RegionViews.Clear();
            foreach (var regionId in SessionController.Instance.RegionsController.GetRegionLibraryElementIds(Controller.LibraryElementModel.LibraryElementId))
            {
                var audioRegionController = SessionController.Instance.ContentController.GetLibraryElementController(regionId) as AudioRegionLibraryElementController;
                if (audioRegionController == null)
                {
                    return;
                }
                var vm = new AudioRegionViewModel(audioRegionController.AudioRegionModel, audioRegionController, this);
                vm.Editable = this.Editable;
                var view = new AudioRegionView(vm);
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
