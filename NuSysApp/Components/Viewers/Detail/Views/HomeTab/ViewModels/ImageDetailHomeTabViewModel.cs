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
        public ObservableCollection<Rectangle> Regions;
        public Uri Image { get; }
        public ImageDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
            Regions = new ObservableCollection<Rectangle>();
            controller.RegionAdded += RegionAdded;
            controller.RegionRemoved += RegionRemoved;
            Image = controller.GetSource();
        }

        private void RegionAdded(object sender, string newRegion)
        {
            var rectangle = JsonConvert.DeserializeObject<Rectangle>(newRegion);
            Regions.Add(rectangle);
        }
        private void RegionRemoved(object sender, string oldRegion)
        {
            //figure out which rectangle to remeove
            //remove it
        }
    }
}
