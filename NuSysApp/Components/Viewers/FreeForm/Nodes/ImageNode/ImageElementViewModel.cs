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
using System.Diagnostics;

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
                if ((regionView.DataContext as ImageRegionViewModel).Model.LibraryElementId == imageRegion.LibraryElementId)
                    Regions.Remove(regionView);
            }
            

            RaisePropertyChanged("Regions");
        }

        public void CreateRegionViews(){
            Regions.Clear();

            var regionLibraryElementIds =
                SessionController.Instance.RegionsController.GetRegionLibraryElementIds(Controller.LibraryElementModel.LibraryElementId);
            foreach (var regionLibraryElementId in regionLibraryElementIds)
            {
                var regionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(regionLibraryElementId) as RectangleRegionLibraryElementController;

                Debug.Assert(regionLibraryElementController != null);
                Debug.Assert(regionLibraryElementController.LibraryElementModel is RectangleRegion);

                var viewmodel = new ImageRegionViewModel(regionLibraryElementController.LibraryElementModel as RectangleRegion, regionLibraryElementController, this);
                viewmodel.Editable = false;
                var view = new ImageRegionView(viewmodel);
                Regions.Add(view);
            }
            RaisePropertyChanged("Regions");
        }
        public void SizeChanged(object sender, double width, double height)
        {

        }
        public override void Dispose()
        {
            if (Controller != null)
            {
                Controller.LibraryElementController.Loaded -= LibraryElementModelOnOnLoaded;
            }
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

            var newHeight = View.GetHeight();
            var newWidth = View.GetWidth();

            foreach (var rv in Regions)
            {
                var regionViewViewModel = rv.DataContext as RegionViewModel;
                regionViewViewModel?.ChangeSize(this, width, height);
            }

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