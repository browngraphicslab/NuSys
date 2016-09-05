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

        public override CreateNewLibraryElementRequestArgs GetNewCreateLibraryElementRequestArgs()
        {
            var imageLibraryElement = (LibraryElementController as ImageLibraryElementController)?.ImageLibraryElementModel;
            Debug.Assert(imageLibraryElement != null);

            var args = new CreateNewImageLibraryElementRequestArgs();
            args.NormalizedX = .25 * imageLibraryElement.NormalizedX;
            args.NormalizedY = .25 * imageLibraryElement.NormalizedY;
            args.NormalizedHeight = .5 * imageLibraryElement.NormalizedHeight;
            args.NormalizedWidth = .5 * imageLibraryElement.NormalizedWidth;

            return args;
        }
    }
}
