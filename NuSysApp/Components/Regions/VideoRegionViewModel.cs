using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class VideoRegionViewModel : RegionViewModel 
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public VideoRegionViewModel(VideoRegionModel model, LibraryElementController controller, Sizeable sizeable) : base(model,controller, sizeable)
        {
            ContainerSizeChanged += BaseSizeChanged;
        }
        private void BaseSizeChanged(object sender, double width, double height)
        {
            var model = Model as TimeRegionModel;
            if (model == null)
            {
                return;
            }
        }
   }

}
