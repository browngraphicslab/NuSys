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
        public Uri Image { get; }
        public ImageDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
            Regions = new ObservableCollection<Region>();
            controller.RegionAdded += RegionAdded;
            controller.RegionRemoved += RegionRemoved;
            Image = controller.GetSource();
        }

        private void RegionAdded(object sender, Region newRegion)
        {
            //var rectangle = JsonConvert.DeserializeObject<Region>(newRegion.ToString());
            Regions.Add(newRegion);
        }
        private void RegionRemoved(object sender, Region oldRegion)
        {
            //figure out which rectangle to remeove
            //remove it
        }
    }
}
