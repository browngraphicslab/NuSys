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
        public LibraryElementController Controller { get; }
        public LibraryElementModel Model { get; }
        public ObservableCollection<ImageRegionView> RegionViews { set; get; }
        public Uri Image { get; }
        //public Boolean Editable { get; set; }
        public ImageDetailHomeTabViewModel(LibraryElementController controller) : base(controller)
        {
            Controller = controller;
            Model = controller.LibraryElementModel;
            // controller.RegionAdded += RegionAdded;
            // controller.RegionRemoved += RegionRemoved;

            Image = controller.GetSource();
            RegionViews = new ObservableCollection<ImageRegionView>();
            Editable = true;
        }

        public override void AddRegion(object sender, Region region)
        {
            var imageRegion = region as RectangleRegion;
            if (imageRegion == null)
            {
                return;
            }
            var vm = new ImageRegionViewModel(imageRegion, Controller, this);
            if (!Editable)
                vm.Editable = false;
            var view = new ImageRegionView(vm);
            RegionViews.Add(view);

            RaisePropertyChanged("RegionViews");
        }


        public override void RemoveRegion(object sender, Region displayedRegion)
        {
            throw new NotImplementedException();
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
            return View.ActualHeight;
        }
        public double GetWidth()
        {
            return View.ActualWidth;
        }

        public override void SetExistingRegions(HashSet<Region> regions)
        {
            RegionViews.Clear();
            foreach (var regionModel in regions)
            {
                var imageRegion = regionModel as RectangleRegion;
                if (imageRegion == null)
                {
                    return;
                }
                var vm = new ImageRegionViewModel(imageRegion, Controller, this);
                if (!Editable)
                    vm.Editable = false;
                var view = new ImageRegionView(vm);
                
                RegionViews.Add(view);

            }
            RaisePropertyChanged("RegionViews");
        }
    }
}
