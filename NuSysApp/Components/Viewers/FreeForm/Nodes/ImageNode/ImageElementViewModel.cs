using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ImageElementViewModel : ElementViewModel, Sizeable
    {
        public ObservableCollection<ImageRegionView> Regions { get
            {
                var collection = new ObservableCollection<ImageRegionView>();
                var elementController = Controller.LibraryElementController;
                var regionHashSet = elementController.LibraryElementModel.Regions;

                if (regionHashSet == null)
                    return collection;
                
                foreach (var model in regionHashSet)
                {
                    var viewmodel = new ImageRegionViewModel(model as RectangleRegion, elementController, this);
                    viewmodel.Editable = false;
                    var view = new ImageRegionView(viewmodel);
                    collection.Add(view);
                }
                return collection;
            }
        }

        public LibraryElementController LibraryElementController{get { return Controller.LibraryElementController; }}
        public ImageElementViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));       
            Controller.LibraryElementController.RegionAdded += LibraryElementControllerOnRegionAdded;
            Controller.LibraryElementController.RegionUpdated += LibraryElementControllerOnRegionUpdated;
            
        }

        private void LibraryElementControllerOnRegionUpdated(object source, Region region)
        {
            RaisePropertyChanged("Regions");
        }

        private void LibraryElementControllerOnRegionAdded(object source, Region region)
        {
            RaisePropertyChanged("Regions");
        }

        public override void Dispose()
        {
            Controller.LibraryElementController.Loaded -= LibraryElementModelOnOnLoaded;
            Image.ImageOpened -= UpdateSizeFromModel;
        }

        public BitmapImage Image { get; set; }


        public override async Task Init()
        {
            if (Controller.LibraryElementController.IsLoaded)
            {
                await DisplayImage();
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
            RaisePropertyChanged("Image");
        }

        private void LibraryElementModelOnOnLoaded(object sender)
        {
            DisplayImage();
        }

        private async Task DisplayImage()
        {
            var url = Controller.LibraryElementController.GetSource();
            Image = new BitmapImage();
            Image.UriSource = url;
            Image.ImageOpened += UpdateSizeFromModel;
            RaisePropertyChanged("Image");
        }
        private void UpdateSizeFromModel(object sender, object args)
        {
            var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
            Controller.SetSize(Controller.Model.Width, Controller.Model.Width * ratio);
        }
        /*
        public override void SetSize(double width, double height)
        {
            if (Image == null)
            {
                return;
            }

            var ratio = (double)Image.PixelHeight / (double)Image.PixelWidth;
            base.SetSize(width, width * ratio);
        }*/
        public override double GetRatio()
        {
            return Image == null ? 1 :(double)Image.PixelHeight / (double)Image.PixelWidth;
        }
        protected override void OnSizeChanged(object source, double width, double height)
        {
            SetSize(width,height);

        }

        public double GetWidth()
        {
            return Width;
        }

        public double GetHeight()
        {
            return Height;
        }
    }
}