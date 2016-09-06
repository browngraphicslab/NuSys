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
using Windows.UI.Xaml.Media;
using NuSysApp.Util;
using System.Diagnostics;
using NusysIntermediate;

namespace NuSysApp
{
    public class ImageElementViewModel : ElementViewModel
    {

        public Size ImageSize { get; set; } = new Size(1,1);

        public ImageElementViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }
        
        public override async Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
        }

        public override double GetRatio()
        {
            var lib = (Controller.LibraryElementModel as ImageLibraryElementModel);
            var newRatio = lib.Ratio * (lib.NormalizedWidth / lib.NormalizedHeight);
            return newRatio;

        }
        /// <summary>
        /// This is the function that recieves a set size event from the element controller and appropriately 
        /// resizes the node that is attached to the controller according to the aspect ratio
        /// </summary>
        /// <param name="source"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected override void OnSizeChanged(object source, double width, double height)
        {
            if (height * GetRatio() < Constants.MinNodeSize)
            {
                return; // If the height becomes smaller than the minimum node size then we don't apply the size changed, applying the height's change causes weird behaviour
            }

            SetSize(height * GetRatio(), height);

        }
    }
}