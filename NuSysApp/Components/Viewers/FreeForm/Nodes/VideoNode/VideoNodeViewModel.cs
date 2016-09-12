using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using NusysIntermediate;
using NuSysApp.Util;

namespace NuSysApp
{
    public class VideoNodeViewModel : ElementViewModel
    {
        public VideoNodeViewModel(ElementController controller) : base(controller)
        {
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
        }

        public override async Task Init()
        {
            if (!Controller.LibraryElementController.ContentLoaded)
            {
                await Controller.LibraryElementController.LoadContentDataModelAsync();
            }
            Controller.SetSize(Model.Width, Model.Height,false);
        }

        public override double GetRatio()
        {
            var lib = (Controller.LibraryElementModel as VideoLibraryElementModel);
            var newRatio = lib.Ratio;
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
