using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Newtonsoft.Json;

namespace NuSysApp
{
    public class ImageDetailHomeTabViewModel : DetailHomeTabViewModel
    {
        public LibraryElementController Controller { get; }
        public LibraryElementModel Model { get; }
        public ObservableCollection<Region> Regions;
        public ObservableCollection<ImageRegionView> RegionViews { set; get; }
        public Uri Image { get; }
        public ImageDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
            Regions = new ObservableCollection<Region>();
           // controller.RegionAdded += RegionAdded;
           // controller.RegionRemoved += RegionRemoved;
            Image = controller.GetSource();
            RegionViews = new ObservableCollection<ImageRegionView>();
        }

        public void RegionAdded(Region newRegion, ImageDetailHomeTabView contentview)
        {
            //var rectangle = JsonConvert.DeserializeObject<Region>(newRegion.ToString());
            Regions.Add(newRegion);
            RegionViews.Add(new ImageRegionView(newRegion as RectangleRegion, contentview));
            Controller.AddRegion(newRegion);
            RaisePropertyChanged("RegionViews");
            
        }
        public void RegionRemoved(Region oldRegion, ImageDetailHomeTabView contentview)
        {
            Regions.Remove(oldRegion);
            Controller.RemoveRegion(oldRegion);
            RegionViews.Remove(new ImageRegionView(oldRegion as RectangleRegion, contentview)); 
            RaisePropertyChanged("RegionViews");
        }
    }
}
