using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace NuSysApp
{
    public class ImageRegionViewModel : RegionViewModel
    {

        private double _height;
        private double _width;
        private string _name;
        private bool _editable;

        
        public string Name {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        public double Height
        {
            get { return _height; }
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
        }

        public double Width
        {
            get { return _width; }
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
        }

        public bool Editable
        {
            set
            {

                _editable = value;

                RaisePropertyChanged("Editable");
            }
            get
            {
                return _editable;
            }
        }
        public bool Selected { set; get; }
        public double OriginalHeight { get; set; }
        public double OriginalWidth { get; set; }
        public double ContainerHeight { get; set; }
        public double ContainerWidth { get; set; }

        public delegate void SizeChangedEventHandler(object sender, double width, double height);
        public event SizeChangedEventHandler SizeChanged;

        public delegate void LocationChangedEventHandler(object sender, Point topLeft);
        public event LocationChangedEventHandler LocationChanged;


        public ImageRegionViewModel(RectangleRegion model, LibraryElementController libraryElementController, RectangleRegionController regionController, Sizeable sizeable) : base(model,libraryElementController, regionController,sizeable)
        {
            if (model == null)
            {
                return;
            }
            ContainerSizeChanged += BaseSizeChanged;
            ContainerHeight = sizeable.GetHeight();
            ContainerWidth = sizeable.GetWidth();
            Height = model.Height * ContainerHeight;
            Width = model.Width * ContainerWidth;

            regionController.SizeChanged += RegionController_SizeChanged;
            regionController.LocationChanged += RegionController_LocationChanged;
            regionController.TitleChanged += RegionController_TitleChanged;
            regionController.OnSelect += RegionController_OnSelect;
            Name = Model.Name;
            Editable = true;
            Selected = false;


        }

        //Not currently implemented
        private void RegionController_OnSelect(RegionController regionController)
        {
            RaisePropertyChanged("Selected");
            Selected = true;

        }

        private void RegionController_TitleChanged(object source, string title)
        {
            Name = title;
        }

        private void RegionController_LocationChanged(object sender, Point topLeft)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }


            var denormalizedTopLeft = new Point(model.TopLeftPoint.X * ContainerViewModel.GetWidth(), model.TopLeftPoint.Y * ContainerViewModel.GetHeight());

            LocationChanged?.Invoke(this, denormalizedTopLeft);

        }

        
        private void RegionController_SizeChanged(object sender, double width, double height)
        {
            

            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            
            Height = model.Height * ContainerViewModel.GetHeight();
            Width = model.Width * ContainerViewModel.GetWidth();
            SizeChanged?.Invoke(this, Width, Height);

        }
        

        //NO LONGER USED
        /*
        private void Controller_RegionUpdated(object source, Region region)
        {
            if (region.Id != Model.Id)
            {
                return;
            }
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            Height = model.BottomRightPoint.Y * ContainerViewModel.GetHeight() - model.TopLeftPoint.Y * ContainerViewModel.GetHeight();
            Width = model.BottomRightPoint.X * ContainerViewModel.GetWidth() - model.TopLeftPoint.X * ContainerViewModel.GetWidth();


            RegionChanged?.Invoke(this, Height, Width);
            var topLeft = new Point(model.TopLeftPoint.X * ContainerViewModel.GetWidth(), model.TopLeftPoint.Y * ContainerViewModel.GetHeight());
            var bottomRight = new Point(model.BottomRightPoint.X * ContainerViewModel.GetWidth(), model.BottomRightPoint.Y * ContainerViewModel.GetHeight());
            SizeChanged?.Invoke(this, topLeft, bottomRight);


            //RaisePropertyChanged("Height");
            //RaisePropertyChanged("Width");

        }
        */

        private void resetRegions()
        {
            var model = Model as RectangleRegion;
            LocationChanged?.Invoke(this, new Point(model.TopLeftPoint.X, model.TopLeftPoint.Y));
        }

        private void BaseSizeChanged(object sender, double width, double height)
        {
            
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }

            Point topLeft;

            if (ContainerViewModel is ImageDetailHomeTabViewModel)
            {
                Debug.WriteLine("Detail view: Base Size Changed getting called with widht: " + width);
                //var detailVM = ContainerViewModel as ImageDetailHomeTabViewModel;
                //var diff = detailVM.GetViewWidth() - detailVM.GetWidth();

                Height = model.Height * height;
                Width = model.Width * width;
                ContainerHeight = height;
                ContainerWidth = width;
                topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y * height);

                //ContainerHeight = detailVM.GetHeight();
                //ContainerWidth = detailVM.GetWidth();

                //Height = model.Height * ContainerHeight;
                //Width = model.Width * ContainerWidth;
            }
            else {
                Debug.WriteLine("Normal view: Base Size Changed getting called with widht: " + width);
                Height = model.Height * height;
                Width = model.Width * width;
                ContainerHeight = height;
                ContainerWidth = width;
                topLeft = new Point(model.TopLeftPoint.X * width, model.TopLeftPoint.Y * height);
            }
            
            //TODO: HOOK THIS UP
            //SizeChanged?.Invoke(this, Width, Height); //don't need this???

            LocationChanged?.Invoke(this, topLeft);
            RaisePropertyChanged("Height");
            RaisePropertyChanged("Width");
            //RaisePropertyChanged("ContainerHeight");
            //RaisePropertyChanged("ContainerWidth");
        }

        public void SetNewLocation(Point topLeft)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var normalTopLeftX = topLeft.X / ContainerViewModel.GetWidth();
            var normalTopLeftY = topLeft.Y / ContainerViewModel.GetHeight();

            var tlp = new Point(normalTopLeftX, normalTopLeftY);

            (RegionController as RectangleRegionController).SetLocation(tlp);
        }

        public void SetNewSize(double width, double height)
        {
            var model = Model as RectangleRegion;
            if (model == null)
            {
                return;
            }
            var normalWidth = width / ContainerViewModel.GetWidth();
            var normalHeight = height / ContainerViewModel.GetHeight();

            

            (RegionController as RectangleRegionController).SetSize(normalWidth, normalHeight);
        }

        public void SetNewName(string text)
        {
            Name = text;
            RegionController.SetTitle(Name);
        }
    }
}
