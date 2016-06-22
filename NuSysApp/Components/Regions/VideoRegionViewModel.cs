using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
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
        public double AudioRegionWidth{ get; set; }

        public double AudioRegionHeight
        {
            get
            {
                return Height - 150;
            }
        }

        public Boolean Editable { get; set; }
        public double LeftHandleX
        {
            get
            {
                var model = Model as VideoRegionModel;
                return ContainerViewModel.GetWidth() * model.Start;
            } 
        }
        public double RightHandleX
        {
            get
            {
                var model = Model as VideoRegionModel;
                return ContainerViewModel.GetWidth() * model.End;
            } 
        }
        public Point TopLeft 
        {
            get
            {
                var model = Model as VideoRegionModel;
                return new Point(model.TopLeft.X* ContainerViewModel.GetWidth(), model.TopLeft.Y * ContainerViewModel.GetHeight());
            } 
        }
        public Point BottomRight 
        {
            get
            {
                var model = Model as VideoRegionModel;
                return new Point(model.BottomRight.X* ContainerViewModel.GetWidth(), model.BottomRight.Y * ContainerViewModel.GetHeight());
            } 
        }

        public VideoRegionViewModel(VideoRegionModel model, LibraryElementController controller, RegionController regionController,Sizeable sizeable) : base(model,controller, regionController,sizeable)
        {
            ContainerSizeChanged += BaseSizeChanged;
            Height = sizeable.GetHeight();
            Width = sizeable.GetWidth();
            AudioRegionWidth = (model.End - model.Start)*sizeable.GetWidth();
            Editable = true;
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
            RaisePropertyChanged("AudioRegionWidth");
            RaisePropertyChanged("AudioRegionHeight");
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

        internal void SetNewPoints(double Start, double End, Point TopLeft, Point BottomRight)
        {
            var model = Model as VideoRegionModel;
            if (model == null)
            {
                return;
            }
            model.Start += Start / ContainerViewModel.GetWidth();
            model.End += End / ContainerViewModel.GetWidth();
            model.BottomRight = new Point(model.BottomRight.X+(BottomRight.X/ContainerViewModel.GetWidth()),model.BottomRight.Y+(BottomRight.Y/ContainerViewModel.GetHeight()));
            model.TopLeft = new Point(model.TopLeft.X+(TopLeft.X / ContainerViewModel.GetWidth()),model.TopLeft.Y+(TopLeft.Y / ContainerViewModel.GetHeight()));
            AudioRegionWidth = (model.End - model.Start)*ContainerViewModel.GetWidth();
            LibraryElementController.UpdateRegion(model);

            RaisePropertyChanged("LeftHandleX");
            RaisePropertyChanged("RightHandleX");
            RaisePropertyChanged("TopLeft");
            RaisePropertyChanged("BottomRight");
            RaisePropertyChanged("AudioRegionWidth");
            RaisePropertyChanged("AudioRegionHeight");
        }
    }

}