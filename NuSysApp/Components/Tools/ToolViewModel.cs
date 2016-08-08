using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NusysIntermediate;
using NuSysApp.Tools;

namespace NuSysApp
{
    public abstract class ToolViewModel : BaseINPC, ToolLinkable
    {
        public delegate void PropertiesToDisplayChangedEventHandler();
        /// <summary>
        /// Listened to by view to know when the properties to display have changed
        /// </summary>
        public event PropertiesToDisplayChangedEventHandler PropertiesToDisplayChanged;
        /// <summary>
        /// Listened to by links to know when the anchor changes.
        /// </summary>
        public event EventHandler<Point2d> ToolAnchorChanged;
        /// <summary>
        /// Listened to by links to know when to delete the link
        /// </summary>
        public event EventHandler<string> Disposed;
        /// <summary>
        /// Listened to by links to know when to replace the tool it is connected to
        /// </summary>
        public event EventHandler<ToolLinkable> ReplacedToolLinkAnchorPoint;

        protected ToolController _controller;
        private double _width;
        private double _height;
        private double _x;
        private double _y;
        private CompositeTransform _transform = new CompositeTransform();
        private Point2d _anchor;
        public ObservableCollection<ToolModel.ParentOperatorType> ParentOperatorList = new ObservableCollection<ToolModel.ParentOperatorType>() {ToolModel.ParentOperatorType.And, ToolModel.ParentOperatorType.Or};
       
        public Point2d ToolAnchor { get { return _anchor; } }
        public ToolController Controller { get { return _controller; } }
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

        public ToolViewModel(ToolController toolController)
        {
            CalculateAnchorPoint();
            _controller = toolController;
            _controller.IdsToDisplayChanged += ControllerOnLibraryIdsToDisplayChanged;
            Controller.SizeChanged += OnSizeChanged;
            Controller.LocationChanged += OnLocationChanged;
            Height = 400;
            Width = 260;
        }

        /// <summary>
        ///So that subclasses can fire the event
        /// </summary>
        public void FireReplacedToolLinkAnchorPoint(ToolLinkable newTool)
        {
            ReplacedToolLinkAnchorPoint?.Invoke(this, newTool);
        }

        /// <summary>
        /// So that sublcasses can invoke properties to display changed event
        /// </summary>
        public void InvokePropertiesToDisplayChanged()
        {
            PropertiesToDisplayChanged?.Invoke();
        }

        /// <summary>
        /// Returns the tool startable
        /// </summary>
        /// <returns></returns>
        public ToolStartable GetToolStartable()
        {
            return Controller;
        }

        /// <summary>
        /// Calculates the tool anchor point of as the top center of the node
        /// </summary>
        public void CalculateAnchorPoint()
        {
            _anchor = new Point2d(X + Width / 2 + 60, Y + 20);
        }

        /// <summary>
        /// Creates a collection from this tools output library ids
        /// </summary>
        public async void CreateCollection(double x, double y)
        {
            Task.Run(async delegate
            {
                var collectionID = SessionController.Instance.GenerateId();
                var request = new CreateNewLibraryElementRequest(collectionID, "", NusysConstants.ElementType.Collection,
                    "Tool-Generated Collection");
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                var m = new Message();
                m["width"] = "300";
                m["height"] = "300";
                m["type"] = NusysConstants.ElementType.Collection.ToString();
                m["x"] = x;
                m["y"] = y;
                m["contentId"] = collectionID;
                m["autoCreate"] = true;
                m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Model.LibraryId;
                var collRequest = new NewElementRequest(m);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(collRequest);
                foreach (var id in Controller.Model.OutputLibraryIds)
                {
                    var lem = SessionController.Instance.ContentController.GetLibraryElementModel(id);
                    if (lem == null || lem.Type == NusysConstants.ElementType.Link)
                    {
                        continue;
                    }
                    var dict = new Message();
                    dict["title"] = lem.Title;
                    dict["width"] = "300";
                    dict["height"] = "300";
                    dict["type"] = lem.Type.ToString();
                    dict["x"] = "50000";
                    dict["y"] = "50000";
                    dict["contentId"] = lem.LibraryElementId;
                    dict["autoCreate"] = true;
                    dict["creator"] = collectionID;
                    var elementRequest = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);
                }

            });
        }

        /// <summary>
        /// Creates a stack of elements from this tools output library ids
        /// </summary>
        public async void CreateStack(double x, double y)
        {
            Task.Run(async delegate
            {
                int i = 0;
                int offset = 40;
                foreach (var id in Controller.Model.OutputLibraryIds)
                {
                    var lem = SessionController.Instance.ContentController.GetLibraryElementModel(id);
                    if (lem == null || lem.Type == NusysConstants.ElementType.Link || i > 20)//TODO indicate to user than no more than 20 non-link items will be made
                    {
                        continue;
                    }
                    var dict = new Message();
                    dict["title"] = lem.Title;
                    dict["width"] = "300";
                    dict["height"] = "300";
                    dict["type"] = lem.Type.ToString();
                    dict["x"] = x + i*offset;
                    dict["y"] = y + i*offset;
                    dict["contentId"] = lem.LibraryElementId;
                    dict["autoCreate"] = true;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Model.LibraryId;
                    var elementRequest = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);
                    i++;
                }

            });
        }

        /// <summary>
        /// Opens the detail view of the selected value if possible (i.e. there is only 1 library id selected)
        /// </summary>
        public void OpenDetailView()
        {
            if (Controller.Model.OutputLibraryIds.Count == 1)
            {
                var lem = SessionController.Instance.ContentController.GetLibraryElementController(Controller.Model.OutputLibraryIds.First());
                SessionController.Instance.SessionView.ShowDetailView(lem);
            }
            
        }

        /// <summary>
        /// Returns a boolean representing if creating a tool chain from this tool to the passed in tool will create a loop
        /// </summary>
        public bool CreatesLoop(ToolViewModel toolViewModel)
        {
            bool createsLoop = false;
            var controllers = new List<ToolStartable>(Controller.Model.ParentIds.Select(item => ToolController.ToolControllers.ContainsKey(item) ? ToolController.ToolControllers[item] : null));

            while (controllers != null && controllers.Count != 0)
            {
                if (controllers.Contains(toolViewModel.Controller))
                {
                    createsLoop = true;
                    break;
                }
                var tempControllers = new List<ToolStartable>();
                foreach (var controller in controllers)
                {
                    tempControllers = new List<ToolStartable>(tempControllers.Union(new List<ToolStartable>(
                            controller.GetParentIds().Select(
                                item =>
                                    ToolController.ToolControllers.ContainsKey(item)
                                        ? ToolController.ToolControllers[item]
                                        : null))));
                }
                controllers = tempControllers;
            }
            return createsLoop;
        }

        /// <summary>
        /// Will either add this tool as a parent if dropped on top of an existing tool, or create a brand new tool filter chooser view. 
        /// </summary>
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

        /// <summary>
        ///creates new filter tool at specified location
        /// </summary>
        public void AddNewFilterTool(double x, double y, FreeFormViewerViewModel wvm)
        {
            

            var toolFilter = new ToolFilterView(x, y, this);

            var linkviewmodel = new ToolLinkViewModel(this, toolFilter);
            var link = new ToolLinkView(linkviewmodel);
            
            Canvas.SetZIndex(link, Canvas.GetZIndex(toolFilter) - 1);
            wvm.AtomViewList.Add(toolFilter);
            wvm.AtomViewList.Add(link);
        }

        /// <summary>
        ///Adds tool as parent to existing filter picker tool. 
        /// </summary>
        public void AddFilterToFilterToolView(List<UIElement> hitsStartList, FreeFormViewerViewModel wvm)
        {

            var linkviewmodel = new ToolLinkViewModel(this, (hitsStartList.First() as ToolFilterView));
            var linkView = new ToolLinkView(linkviewmodel);
            Canvas.SetZIndex(linkView, Canvas.GetZIndex(hitsStartList.First()) - 1);
            (hitsStartList.First() as ToolFilterView).AddParentTool(this);
            wvm.AtomViewList.Add(linkView);
        }

        /// <summary>
        ///Adds tool as parent to existing tool. 
        /// </summary>
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

        /// <summary>
        /// Returns the drag filter image with correct sizing etc.
        /// </summary>
        /// <returns></returns>
        public Image InitializeDragFilterImage()
        {
            Image dragItem = new Image();
            dragItem.Source = new BitmapImage(new Uri("ms-appx:///Assets/filter.png"));
            dragItem.Height = 50;
            dragItem.Width = 50;
            return dragItem;
        }

        /// <summary>
        /// Removes all the listeners and calls dispose on the controller
        /// </summary>
        public void Dispose()
        {
            _controller.IdsToDisplayChanged -= ControllerOnLibraryIdsToDisplayChanged;
            Controller.SizeChanged -= OnSizeChanged;
            Controller.LocationChanged -= OnLocationChanged;
            Controller.Dispose();
            Disposed?.Invoke(this, Controller.GetID());
        }

        /// <summary>
        ///Reloads the properties to to display
        /// </summary>
        private void ControllerOnLibraryIdsToDisplayChanged()
        {
            ReloadPropertiesToDisplay();
        }

        public abstract void ReloadPropertiesToDisplay();

        public void OnSizeChanged(object sender, double width, double height)
        {
            Width = width;
            Height = height;
            CalculateAnchorPoint();
            ToolAnchorChanged?.Invoke(this, _anchor);
        }

        public void OnLocationChanged(object sender, double x, double y)
        {
            X = x;
            Y = y;
            Transform.TranslateX = x;
            Transform.TranslateY = y;
            RaisePropertyChanged("Transform");
            CalculateAnchorPoint();
            ToolAnchorChanged?.Invoke(this, _anchor);

        }


    }
}