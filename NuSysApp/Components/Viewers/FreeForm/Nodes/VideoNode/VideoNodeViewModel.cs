using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Nodes.AudioNode;

namespace NuSysApp
{
    public class VideoNodeViewModel : ElementViewModel, Sizeable
    {
        private ObservableCollection<VideoRegionView> _regionViews = new ObservableCollection<VideoRegionView>();

        public delegate double GetWidthEventHandler(object sender);
        public event GetWidthEventHandler OnGetMediaPlayerWidth;
        public delegate double GetHeightEventHandler(object sender);
        public event GetHeightEventHandler OnGetMediaPlayerHeight;
        public ObservableCollection<VideoRegionView> RegionViews
        {
            get
            {
                _regionViews.Clear();
                var elementController = Controller.LibraryElementController;
                var regionHashSet = elementController.LibraryElementModel.Regions;

                if (regionHashSet == null)
                {
                    return _regionViews;
                }

                foreach (var model in regionHashSet)
                {
                    var videoRegionModel = model as VideoRegionModel;
                    if (videoRegionModel == null)
                    {
                        return _regionViews;
                    }
                    var regionController = SessionController.Instance.RegionsController.GetRegionController(model.Id);
                    Debug.Assert(regionController is VideoRegionController);
                    regionController.RegionUpdated += LibraryElementControllerOnRegionUpdated;
                    var viewmodel = new VideoRegionViewModel(videoRegionModel, elementController, regionController as VideoRegionController, this);
                    viewmodel.Editable = false;
                    var view = new VideoRegionView(viewmodel);
                    _regionViews.Add(view);
                }
                return _regionViews;

            }
        }

        public VideoNodeViewModel(ElementController controller) : base(controller)
        {
            this.Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 100, 175, 255));
            Controller.LibraryElementController.RegionAdded += LibraryElementControllerOnRegionAdded;
            //Controller.LibraryElementController.RegionUpdated += LibraryElementControllerOnRegionUpdated;
            Controller.SizeChanged += Controller_SizeChanged;
            Controller.LibraryElementController.Loaded += LibraryElementController_Loaded;
        }

        private void LibraryElementController_Loaded(object sender)
        {
            RaisePropertyChanged("RegionViews");
        }

        private void Controller_SizeChanged(object source, double width, double height)
        {
            RaisePropertyChanged("RegionViews");
        }

        private void LibraryElementControllerOnRegionUpdated(object source, Region region)
        {
            RaisePropertyChanged("RegionViews");
        }

        private void LibraryElementControllerOnRegionAdded(object source, RegionController regionController)
        {
            RaisePropertyChanged("RegionViews");
        }

        public override void Dispose()
        {
            Controller.LibraryElementController.Loaded -= LibraryElementModelOnOnLoaded;
            base.Dispose();
        }

        public override async Task Init()
        {
            if (Controller.LibraryElementController.IsLoaded)
            {
                Controller.SetSize(Model.Width, Model.Height);
            }
            else
            {
                Controller.LibraryElementController.Loaded += LibraryElementModelOnOnLoaded;
            }
        }

        private void LibraryElementModelOnOnLoaded(object sender)
        {
            Controller.SetSize(Model.Width, Model.Height);
        }

        public override void SetSize(double width, double height)
        {
            var model = (VideoNodeModel)Model;
            if (model.ResolutionX < 1)
            {
                return;
            }
            if (width > height)
            {
                var r = model.ResolutionY / (double)model.ResolutionX;
                base.SetSize(width, width * r + 150);
            }
            else
            {

                var r = model.ResolutionX / (double)model.ResolutionY;
                base.SetSize(height * r, height + 150);
            }
        }

        public ObservableCollection<LinkedTimeBlockModel> LinkedTimeModels
        {
            get { return (Model as VideoNodeModel).LinkedTimeModels; }
        }


        public void AddLinkTimeModel(LinkedTimeBlockModel model)
        {
            (Model as VideoNodeModel).LinkedTimeModels.Add(model);
        }

        protected override void OnSizeChanged(object source, double width, double height)
        {
            SetSize(width, height);
        }
        public double GetWidth()
        {
            var width = OnGetMediaPlayerWidth?.Invoke(this);
            if (width != null)
            {
                return width.Value;
            }
            return 0;
        }

        public double GetHeight()
        {
            var height = OnGetMediaPlayerHeight?.Invoke(this);
            if (height != null)
            {
                return height.Value;
            }
            return 0;
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
