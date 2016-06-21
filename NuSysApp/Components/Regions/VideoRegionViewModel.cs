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
        public delegate void DoubleChanged(object sender, double e);
        public event DoubleChanged WidthChanged;
        public event DoubleChanged HeightChanged;
        public double Height { get; set; }
        public double Width{ get; set; }
        public Boolean Editable { get; set; }

        public VideoRegionViewModel(VideoRegionModel model, LibraryElementController controller, Sizeable sizeable) : base(model,controller, sizeable)
        {
            ContainerSizeChanged += BaseSizeChanged;
            Height = sizeable.GetHeight();
            Width = sizeable.GetWidth();
            Editable = true;
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
            WidthChanged?.Invoke(this,Width);
            HeightChanged?.Invoke(this,Height);
        }

        private void BaseSizeChanged(object sender, double width, double height)
        {
            var model = Model as VideoRegionModel;
            if (model == null)
            {
                return;
            }
            Width = width;
            Height = height;
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
        }

   }

}