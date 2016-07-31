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
        
        public ImageDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            LibraryElementController = controller;
            Model = controller.LibraryElementModel;

            Image = controller.GetSource();
            Editable = true;
           
        }

        public override void AddRegion(object sender, RegionLibraryElementController regionLibraryElementController)
        {
          //  var rectRegionController = regionLibraryElementController as RectangleRegionLibraryElementController;
          //  var imageRegion = rectRegionController?.LibraryElementModel as RectangleRegion;
          //  if (imageRegion == null)
          //  {
          //      return;
          //  }
          ////  var vm = new ImageRegionViewModel(imageRegion, rectRegionController, this);


          //  RaisePropertyChanged("RegionViews");
        }


        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            //var imageRegion = displayedRegion as RectangleRegion;
            //if (imageRegion == null)
            //{
            //    return;
            //}

            //foreach (var regionView in RegionViews.ToList<ImageRegionView>())
            //{
            //    if ((regionView.DataContext as ImageRegionViewModel).Model == imageRegion)
            //        RegionViews.Remove(regionView);
            //}

            //RaisePropertyChanged("RegionViews");
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            if (!Editable)
            {
                Debug.WriteLine("The detail view size is: " + width);
                Debug.WriteLine("Image Width: " + this.GetWidth());

            }
            
            //foreach (var rv in RegionViews)
            //{
            //    var regionViewViewModel = rv.DataContext as ImageRegionViewModel;
            //    regionViewViewModel?.ChangeSize(sender, this.GetWidth(), this.GetHeight());
            //}
            
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
        public override void SetExistingRegions()
        {
            //RegionViews.Clear();

            //var regionsLibraryElementIds=
            //    SessionController.Instance.RegionsController.GetClippingParentRegionLibraryElementIds(
            //        LibraryElementController.LibraryElementModel.LibraryElementId);
            //foreach (var regionLibraryElementId in regionsLibraryElementIds)
            //{
            //    var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementId) as RectangleRegionLibraryElementController;
            //    Debug.Assert(regionLibraryElementController != null);
            //    Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);
            //    var vm = new ImageRegionViewModel(regionLibraryElementController.LibraryElementModel as RectangleRegion, regionLibraryElementController, this);
            //    if (!Editable)
            //        vm.Editable = false;
            //    var view = new ImageRegionView(vm);
            //    RegionViews.Add(view);

            //}
            //RaisePropertyChanged("RegionViews");
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

        public void HighlightRegion(RectangleRegion rectangleRegion)
        {
            if (rectangleRegion == null)
            {
                return;
            }
            //foreach (var view in RegionViews.ToList<ImageRegionView>())
            //{
            //    if ((view.DataContext as ImageRegionViewModel).Model.LibraryElementId == rectangleRegion.LibraryElementId)
            //    {
            //        view.Select();
            //    }
            //}
        }
    }
}
