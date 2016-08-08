using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageDetailHomeTabViewModel : DetailHomeTabViewModel
    {

        public LibraryElementController LibraryElementController { get; }
        public LibraryElementModel Model { get; }
        public Uri Image { get; }    
 
        public ImageDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            Model = controller.LibraryElementModel;

            Image = new Uri(controller.Data);
            Editable = true;         
        }

        public override Message GetNewRegionMessage()
        {
            var m = new Message();
            m["rectangle_location"] = new Point(.25, .25);
            m["rectangle_width"] = .5;
            m["rectangle_height"] = .5;
           
            // if the library element LibraryElementController is a region noramlize top left point, height, and width for original content
            if (LibraryElementController is RectangleRegionLibraryElementController)
            {
                var imageRegionLibraryElementController =
                    LibraryElementController as RectangleRegionLibraryElementController;
                var rectangleRegionModel = imageRegionLibraryElementController?.RectangleRegionModel;

                // normalizes the top left point so that it is in the correct place on the original content
                m["rectangle_location"] = new Point(.25 * rectangleRegionModel.Width + rectangleRegionModel.TopLeftPoint.X, 
                                                    .25 * rectangleRegionModel.Height + rectangleRegionModel.TopLeftPoint.Y);
                // same for width and height
                m["rectangle_width"] = .5 * rectangleRegionModel.Width;
                m["rectangle_height"] = .5 * rectangleRegionModel.Height;
            }

            return m;
        }
    }
}
