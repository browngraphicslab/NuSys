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

namespace NuSysApp
{
    public class ImageDetailHomeTabViewModel : DetailHomeTabViewModel, Sizeable
    {
        public LibraryElementController LibraryElementController { get; }
        public LibraryElementModel Model { get; }
        public Uri Image { get; }
        
        public double ImageWidth {
            set
            {
                _imageWidth = value;
                RaisePropertyChanged("ImageWidth");
            }
            get
            {
                return _imageWidth;
            }
        }

        private double _imageWidth;
        
        //public Boolean Editable { get; set; }
        public ImageDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            Model = controller.LibraryElementModel;

            Image = controller.GetSource();
            Editable = true;
           
        }



        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            var imageRegion = displayedRegion as RectangleRegion;
            if (imageRegion == null)
            {
                return;
            }

            foreach (var regionView in new HashSet<ImageRegionView>((View as ImageDetailHomeTabView).ImageGrid.Children.OfType<ImageRegionView>()))
            {
                if ((regionView.DataContext as ImageRegionViewModel).Model == imageRegion)
                    (View as ImageDetailHomeTabView).ImageGrid.Children.Remove(regionView);
            }

        }

        public override void SizeChanged(object sender, double width, double height)
        {
           
            if (!Editable)
            {
                Debug.WriteLine("The detail view size is: " + width);
                Debug.WriteLine("Image Width: " + this.GetWidth());

            }

            
         /*   foreach (var rv in RegionViews)
            {
                var regionViewViewModel = rv.DataContext as ImageRegionViewModel;
                regionViewViewModel?.ChangeSize(sender, this.GetWidth(), this.GetHeight());
            }*/
            
        }
        public double GetHeight()
        {
            var view = (View as ImageDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
            //return view.ActualHeight;
            return view.GetImgHeight();
        }


        public double GetWidth()
        {
            var view = (View as ImageDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
           //return view.ActualWidth;
            return view.GetImgWidth();
        }

        public override Message GetNewRegionMessage()
        {
            Message m = new Message();
            m["rectangle_top_left_point"] = new Point(.25, .25);
            m["rectangle_width"] = 0.5;
            m["rectangle_height"] = 0.5;

            return m;
        }


        public double GetViewWidth()
        {
            var view = (View as ImageDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
            return view.ActualWidth;
        }

        public double GetViewHeight()
        {
            var view = (View as ImageDetailHomeTabView);
            if (view == null)
            {
                return 0;
            }
            return view.ActualHeight;
        }

        public override void AddRegion(object sender, RegionController regionController)
        {
            throw new NotImplementedException();
        }

        public override void SetExistingRegions()
        {
            throw new NotImplementedException();
        }
    }
}
