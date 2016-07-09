using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace NuSysApp
{
    public abstract class ToolViewModel : BaseINPC
    {
        public delegate void PropertiesToDisplayChangedEventHandler();
        public event PropertiesToDisplayChangedEventHandler PropertiesToDisplayChanged;

        public ToolController Controller { get { return _controller; } }
        protected ToolController _controller;
        private double _width;
        private double _height;
        private double _x;
        private double _y;
        private CompositeTransform _transform = new CompositeTransform();
        public double Width
        {
            set
            {
                _width = value;
                RaisePropertyChanged("Width");
            }
            get
            {
                return _width;
            }
        }

        public ObservableCollection<ToolModel.ParentOperatorType> ParentOperatorList = new ObservableCollection<ToolModel.ParentOperatorType>() {ToolModel.ParentOperatorType.And, ToolModel.ParentOperatorType.Or}; 

        public void InvokePropertiesToDisplayChanged()
        {
            PropertiesToDisplayChanged?.Invoke();
        }
        public double Height
        {
            set
            {
                _height = value;
                RaisePropertyChanged("Height");
            }
            get
            {
                return _height;
            }
        }

        public double X
        {
            set
            {
                _x = value;
                RaisePropertyChanged("X");
            }
            get
            {
                return _x;
            }
        }
        public double Y
        {
            set
            {
                _y = value;
                RaisePropertyChanged("Y");
            }
            get
            {
                return _y;
            }
        }
        public CompositeTransform Transform
        {
            get { return _transform; }
            set
            {
                if (_transform == value)
                {
                    return;
                }
                _transform = value;
                RaisePropertyChanged("Transform");
            }
        }

        public ToolViewModel(ToolController toolController)
        {
            _controller = toolController;
            _controller.ParentsLibraryIdsChanged += ControllerOnParentsLibraryLibraryIdsChanged;
            Controller.SizeChanged += OnSizeChanged;
            Controller.LocationChanged += OnLocationChanged;
            Height = 400;
            Width = 260;
        }

        public async void CreateCollection(double x, double y)
        {
            await Task.Run(async delegate
            {
                var collectionID = SessionController.Instance.GenerateId();
                var request = new CreateNewLibraryElementRequest(collectionID, "", ElementType.Collection,
                    "Tool-Generated Collection");
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                var m = new Message();
                m["width"] = "300";
                m["height"] = "300";
                m["nodeType"] = ElementType.Collection.ToString();
                m["x"] = x;
                m["y"] = y;
                m["contentId"] = collectionID;
                m["autoCreate"] = true;
                m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Model.LibraryId;
                var collRequest = new NewElementRequest(m);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(collRequest);
                foreach (var id in Controller.Model.LibraryIds)
                {
                    var lem = SessionController.Instance.ContentController.GetContent(id);
                    if (lem == null || lem.Type == ElementType.Link)
                    {
                        continue;
                    }
                    var dict = new Message();
                    dict["title"] = lem.Title;
                    dict["width"] = "300";
                    dict["height"] = "300";
                    dict["nodeType"] = lem.Type.ToString();
                    dict["x"] = "50000";
                    dict["y"] = "50000";
                    dict["contentId"] = lem.LibraryElementId;
                    dict["autoCreate"] = true;
                    dict["creator"] = collectionID;
                    var elementRequest = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(elementRequest);
                }

            });
        }

        public void OpenDetailView()
        {
            if (Controller.Model.LibraryIds.Count == 1)
            {
                var lem = SessionController.Instance.ContentController.GetLibraryElementController(Controller.Model.LibraryIds.First());
                SessionController.Instance.SessionView.ShowDetailView(lem);
            }
            
        }

        public bool CreatesLoop(ToolViewModel toolViewModel)
        {
            bool createsLoop = false;
            var controllers = new List<ToolController>(Controller.Model.ParentIds.Select(item => ToolController.ToolControllers.ContainsKey(item) ? ToolController.ToolControllers[item] : null));

            while (controllers != null && controllers.Count != 0)
            {
                if (controllers.Contains(toolViewModel.Controller))
                {
                    createsLoop = true;
                    break;
                }
                var tempControllers = new List<ToolController>();
                foreach (var controller in controllers)
                {
                    tempControllers = new List<ToolController>(tempControllers.Union(new List<ToolController>(
                            controller.Model.ParentIds.Select(
                                item =>
                                    ToolController.ToolControllers.ContainsKey(item)
                                        ? ToolController.ToolControllers[item]
                                        : null))));
                }
                controllers = tempControllers;
            }
            return createsLoop;
        }

        public void FilterIconDropped(IEnumerable<UIElement> hitsStart,  FreeFormViewerViewModel wvm, double x, double y)
        {
            if (hitsStart.Where(uiElem => (uiElem is FrameworkElement) && (uiElem as FrameworkElement).DataContext is ToolViewModel).ToList().Any())
            {
                var hitsStartList = hitsStart.Where(uiElem => (uiElem is AnimatableUserControl) && (uiElem as AnimatableUserControl).DataContext is ToolViewModel).ToList();
                AddFilterToExistingTool(hitsStartList, wvm);
            }
            else if (hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList().Any())
            {
                var hitsStartList = hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList();
                AddFilterToFilterToolView(hitsStartList, wvm);
            }
            else
            {
                AddNewFilterTool(x, y, wvm);
            }
        }

        public void AddNewFilterTool(double x, double y, FreeFormViewerViewModel wvm)
        {
            var toolFilter = new ToolFilterView(x, y, this);
            var toolFilterLinkViewModel = new ToolFilterLinkViewModel(this, toolFilter);
            var toolFilterLink = new ToolFilterLinkView(toolFilterLinkViewModel);
            Canvas.SetZIndex(toolFilterLink, Canvas.GetZIndex(toolFilter) - 1);
            toolFilter.AddLink(toolFilterLink);
            wvm.AtomViewList.Add(toolFilter);
            wvm.AtomViewList.Add(toolFilterLink);
        }

        public void AddFilterToFilterToolView(List<UIElement> hitsStartList, FreeFormViewerViewModel wvm)
        {
            ToolFilterLinkViewModel linkViewModel = new ToolFilterLinkViewModel(this, (hitsStartList.First() as ToolFilterView));
            ToolFilterLinkView linkView = new ToolFilterLinkView(linkViewModel);
            Canvas.SetZIndex(linkView, Canvas.GetZIndex(hitsStartList.First()) - 1);
            (hitsStartList.First() as ToolFilterView).AddLink(linkView);
            (hitsStartList.First() as ToolFilterView).AddParentTool(this);
            wvm.AtomViewList.Add(linkView);
        }

        public void AddFilterToExistingTool(List<UIElement> hitsStartList, FreeFormViewerViewModel wvm)
        {
            ToolViewModel toolViewModel = (hitsStartList.First() as AnimatableUserControl).DataContext as ToolViewModel;
            if (toolViewModel != null && toolViewModel != this)
            {
                if (!CreatesLoop(toolViewModel))
                {
                    var linkviewmodel = new ToolLinkViewModel(this, toolViewModel);
                    var link = new ToolLinkView(linkviewmodel);
                    Canvas.SetZIndex(link, Canvas.GetZIndex(hitsStartList.First()) - 1);
                    wvm.AtomViewList.Add(link);
                    toolViewModel.Controller.AddParent(Controller);
                }
            }
        }

        public Image InitializeDragFilterImage()
        {
            Image dragItem = new Image();
            dragItem.Source = new BitmapImage(new Uri("ms-appx:///Assets/filter.png"));
            dragItem.Height = 50;
            dragItem.Width = 50;
            return dragItem;
        }

        public void Dispose()
        {
            _controller.ParentsLibraryIdsChanged -= ControllerOnParentsLibraryLibraryIdsChanged;
            Controller.SizeChanged -= OnSizeChanged;
            Controller.LocationChanged -= OnLocationChanged;
            Controller.Dispose();
        }

        public void AddChildFilter(ToolController controller)
        {
            controller.AddParent(_controller);

        }

        private void ControllerOnParentsLibraryLibraryIdsChanged()
        {
            ReloadPropertiesToDisplay();
        }

        public abstract void ReloadPropertiesToDisplay();

        public void OnSizeChanged(object sender, double width, double height)
        {
            Width = width;
            Height = height;
        }

        public void OnLocationChanged(object sender, double x, double y)
        {
            X = x;
            Y = y;
            Transform.TranslateX = x;
            Transform.TranslateY = y;
            RaisePropertyChanged("Transform");
        }
    }
}