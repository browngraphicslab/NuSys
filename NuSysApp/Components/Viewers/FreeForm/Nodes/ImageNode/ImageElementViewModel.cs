using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;
using Newtonsoft.Json;
using Windows.UI.Xaml.Media;
using NuSysApp.Util;

namespace NuSysApp
{
    public class ImageElementViewModel : ElementViewModel, Sizeable
    {
   
        public ObservableCollection<ImageRegionView> Regions { private set; get; }
        public Sizeable View { get; set; }

        public LibraryElementController LibraryElementController{get { return Controller.LibraryElementController; }}
        public ImageElementViewModel(ElementController controller) : base(controller)
        {
            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));       
            Regions = new ObservableCollection<ImageRegionView>();
            this.CreateRegionViews();

            Controller.LibraryElementController.RegionAdded += LibraryElementControllerOnRegionAdded;
            Controller.LibraryElementController.RegionRemoved += LibraryElementControllerOnRegionRemoved;


        }

        private void LibraryElementControllerOnRegionRemoved(object source, Region region)
        {
            var imageRegion = region as RectangleRegion;
            if (imageRegion == null)
            {
                return;
            }

            foreach (var regionView in Regions.ToList<ImageRegionView>())
            {
                if ((regionView.DataContext as ImageRegionViewModel).Model == imageRegion)
                    Regions.Remove(regionView);
            }
            

            RaisePropertyChanged("Regions");
        }

        private void CreateRegionViews()
        {
            var elementController = Controller.LibraryElementController;
            var regionHashSet = elementController.LibraryElementModel.Regions;

            if (regionHashSet == null)
                return ;

            Regions.Clear();

            foreach (var model in regionHashSet)
            {

                RectangleRegionController regionController;
                if (SessionController.Instance.RegionsController.GetRegionController(model.Id) == null)
                {
                    var factory = new RegionControllerFactory();
                    regionController = factory.CreateFromSendable(model, ContentId) as RectangleRegionController;
                }
                else {
                    regionController = SessionController.Instance.RegionsController.GetRegionController(model.Id) as RectangleRegionController;
                }

                var viewmodel = new ImageRegionViewModel(model as RectangleRegion, elementController, regionController, this);
                viewmodel.Editable = false;
                var view = new ImageRegionView(viewmodel);
                Regions.Add(view);
            }
            RaisePropertyChanged("Regions");
        }
        public void SizeChanged(object sender, double width, double height)
        {
            var newHeight = View.GetHeight();
            var newWidth = View.GetWidth();

            foreach (var rv in Regions)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(sender, newWidth, newHeight);
            }
        }

        private void LibraryElementControllerOnRegionAdded(object source, RegionController regionController)
        {
            var rectRegionController = regionController as RectangleRegionController;
            var imageRegion = rectRegionController?.Model as RectangleRegion;
            if (imageRegion == null)
            {
                return;
            }
            var vm = new ImageRegionViewModel(imageRegion, Controller.LibraryElementController, rectRegionController, this);
            var view = new ImageRegionView(vm);
            vm.Editable = false;
            Regions.Add(view);
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

        private async void LibraryElementModelOnOnLoaded(object sender)
        {
            await DisplayImage();
            this.CreateRegionViews();

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
            // don't edit if we are in exploration or presentation mode
            if (SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.EXPLORATION ||
                SessionController.Instance.SessionView.ModeInstance?.Mode == ModeType.PRESENTATION)
            {
                return;
            }

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

        public double GetViewWidth()
        {
            throw new NotImplementedException();
        }

        public double GetViewHeight()
        {
            throw new NotImplementedException();
        }
    }
}