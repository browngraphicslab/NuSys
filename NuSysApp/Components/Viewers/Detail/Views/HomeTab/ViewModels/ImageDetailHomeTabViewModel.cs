﻿using System;
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
    public class ImageDetailHomeTabViewModel : DetailHomeTabViewModel, Sizeable
    {
        public LibraryElementController LibraryElementController { get; }
        public LibraryElementModel Model { get; }
        public ObservableCollection<ImageRegionView> RegionViews { set; get; }
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
            RegionViews = new ObservableCollection<ImageRegionView>();
            Editable = true;
            
        }

        public override void AddRegion(object sender, RegionController regionController)
        {
            var rectRegionController = regionController as RectangleRegionController;
            var imageRegion = rectRegionController.Model as RectangleRegion;
            if (imageRegion == null)
            {
                return;
            }
            var vm = new ImageRegionViewModel(imageRegion, LibraryElementController, rectRegionController, this);
            if (!Editable)
                vm.Editable = false;
            var view = new ImageRegionView(vm);
            RegionViews.Add(view);

            RaisePropertyChanged("RegionViews");
        }


        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            var imageRegion = displayedRegion as RectangleRegion;
            if (imageRegion == null)
            {
                return;
            }

            foreach (var regionView in RegionViews.ToList<ImageRegionView>())
            {
                if ((regionView.DataContext as ImageRegionViewModel).Model == imageRegion)
                    RegionViews.Remove(regionView);
            }

            RaisePropertyChanged("RegionViews");
        }

        public override void SizeChanged(object sender, double width, double height)
        {
            var newHeight = View.ActualHeight;
            var newWidth = View.ActualWidth;

            foreach (var rv in RegionViews)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(sender, newWidth, newHeight);
            }
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
        public override void SetExistingRegions(HashSet<Region> regions)
        {
            if (regions == null)
            {
                return;
            }

            RegionViews.Clear();

            foreach (var regionModel in regions)
            {
                var imageRegion = regionModel as RectangleRegion;
                if (imageRegion == null)
                {
                    return;
                }
                RectangleRegionController regionController;
                if (SessionController.Instance.RegionsController.GetRegionController(imageRegion.Id) == null)
                {
                    var factory = new RegionControllerFactory();
                    regionController = factory.CreateFromSendable(regionModel) as RectangleRegionController;
                    SessionController.Instance.RegionsController.Add(regionController);
                }
                else {
                    regionController = SessionController.Instance.RegionsController.GetRegionController(imageRegion.Id) as RectangleRegionController;
                }


                //var regionController = new RegionController(imageRengion);
                var vm = new ImageRegionViewModel(imageRegion, LibraryElementController, regionController, this);
                if (!Editable)
                    vm.Editable = false;

                
                var view = new ImageRegionView(vm);
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");
        }

        public override Region GetNewRegion()
        {
            var region = new RectangleRegion(new Point(.25, .25), new Point(.75, .75));
            return region;
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
    }
}
