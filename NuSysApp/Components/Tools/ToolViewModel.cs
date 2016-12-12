using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
    public abstract class ToolViewModel : ElementViewModel//, ToolLinkable
    {
        public ToolModel.ToolFilterTypeTitle Filter { get { return (_controller).ToolModel.Filter; } set { (_controller).SetFilter(value); } }


        public bool IsSelected { get { return false; } }
        public string Id { get { return _controller?.Model?.Id; } }

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
        public event EventHandler<ElementViewModel> ReplacedToolLinkAnchorPoint;

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
        //public double Height
        //{
        //    set
        //    {
        //        _height = value;
        //        RaisePropertyChanged("Height");
        //    }
        //    get
        //    {
        //        return _height;
        //    }
        //}


        //public double Width
        //{
        //    set
        //    {
        //        _width = value;
        //        RaisePropertyChanged("Width");
        //    }
        //    get
        //    {
        //        return _width;
        //    }
        //}

        //public double X
        //{
        //    set
        //    {
        //        _x = value;
        //        RaisePropertyChanged("X");
        //    }
        //    get
        //    {
        //        return _x;
        //    }
        //}
        //public double Y
        //{
        //    set
        //    {
        //        _y = value;
        //        RaisePropertyChanged("Y");
        //    }
        //    get
        //    {
        //        return _y;
        //    }
        //}
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

        public ToolViewModel(ToolController toolController):base(toolController)
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
        public void FireReplacedToolLinkAnchorPoint(ElementViewModel newTool)
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
                // the library element id of the collection we are creating, used as the parent collection id when adding elements to it later in the method
                var collectionLibElemId = SessionController.Instance.GenerateId();

                // We determine the access type of the tool generated collection based on the collection we're in and pass that in to the request
                NusysConstants.AccessType newCollectionAccessType;
                var currWorkSpaceAccessType = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;
                if (currWorkSpaceAccessType == NusysConstants.AccessType.Public)
                {
                    newCollectionAccessType = NusysConstants.AccessType.Public;
                }
                else
                {
                    newCollectionAccessType = NusysConstants.AccessType.Private;
                }
                // create a new library element args class to assist in creating the collection
                var createNewLibraryElementRequestArgs = new CreateNewLibraryElementRequestArgs
                {
                    ContentId = SessionController.Instance.GenerateId(),
                    LibraryElementType = NusysConstants.ElementType.Collection,
                    Title = "Tool-Generated Collection",
                    LibraryElementId = collectionLibElemId,
                    AccessType = newCollectionAccessType
                };

                // create a new content request args to assist in creating the collection
                var createNewContentRequestArgs = new CreateNewContentRequestArgs
                {
                    LibraryElementArgs = createNewLibraryElementRequestArgs
                };


                var args = new CreateNewCollectionServerRequestArgs();
                args.CreateNewContentRequestDictionary = createNewContentRequestArgs.PackToRequestKeys().ToDictionary(k => k.Key, v => v.Value);
                args.NewElementRequestDictionaries = new List<Dictionary<string, object>>();

                /*
                // create the content request, and execute it, adding the collection to the library
                var request = new CreateNewContentRequest(createNewContentRequestArgs);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                request.AddReturnedLibraryElementToLibrary();
                */

                int i = 0;
               // Add all the elements to the newly created collection
                foreach (var id in Controller.ToolModel.OutputLibraryIds)
                {
                    if (i > 14)
                    {
                        break;
                    }

                    // get the library element model which needs to be added to the stack
                    var lem = SessionController.Instance.ContentController.GetLibraryElementModel(id);

                    // if the library element model doesn't exist, or is a link don't add it to the collection
                    if (lem == null || lem.Type == NusysConstants.ElementType.Link)
                    {
                        continue;
                    }

                    // create a new element request args, and pass in the required fields
                    var newElementRequestArgs = new NewElementRequestArgs
                    {
                        // set the position
                        X = 50000,
                        Y = 50000,

                        // size
                        Width = Constants.DefaultNodeSize,
                        Height = Constants.DefaultNodeSize,

                        // ids
                        ParentCollectionId = collectionLibElemId,
                        LibraryElementId = lem.LibraryElementId
                    };

                    args.NewElementRequestDictionaries.Add( newElementRequestArgs.PackToRequestKeys().ToDictionary(k => k.Key, v => v.Value));

                    /*
                    // create and execute the request
                    var requestElemToCollection = new NewElementRequest(newElementRequestArgs);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(requestElemToCollection);
                    await requestElemToCollection.AddReturnedElementToSessionAsync();
                    */
                    i++;
                }

                var request = new CreateNewCollectionRequest(args);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                request.AddReturnedLibraryElementToLibrary();

                // add the collection to the current session
                var collectionLEM =  SessionController.Instance.ContentController.GetLibraryElementController(collectionLibElemId);
                collectionLEM.AddElementAtPosition(x, y);

            });
        }

        /// <summary>
        /// Creates a stack of elements from this tools output library ids
        /// </summary>
        public async void CreateStack(double x, double y)
        {
            Task.Run(async delegate
            {
                // use the i counter to offset each new element in the stack
                int i = 0;
                int offset = 40;
                foreach (var id in Controller.ToolModel.OutputLibraryIds)
                {
                    if (i > 14)
                    {
                        break;
                    }
                    // get the library element model which needs to be added to the stack
                    var lem = SessionController.Instance.ContentController.GetLibraryElementModel(id);

                    // if the library element model doesn't exist, is a link, or is greater than 20, don't add it to the session
                    if (lem == null || lem.Type == NusysConstants.ElementType.Link)
                    {
                        continue;
                    }

                    // create a new element request args, and pass in the required fields
                    var newElementRequestArgs = new NewElementRequestArgs
                    {
                        // set the position
                        X = x + i*offset,
                        Y = y + i*offset,

                        // size
                        Width = Constants.DefaultNodeSize,
                        Height = Constants.DefaultNodeSize,

                        // ids
                        ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                        LibraryElementId = lem.LibraryElementId
                    };

                    // execute the request
                    var request = new NewElementRequest(newElementRequestArgs);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
                    if (request.WasSuccessful() == true)
                    {
                        await request.AddReturnedElementToSessionAsync();
                    }

                    // increment to finish loop and perform offset
                    i++;
                }

            });
        }

        /// <summary>
        /// Opens the detail view of the selected value if possible (i.e. there is only 1 library id selected)
        /// </summary>
        public void OpenDetailView()
        {
            if (Controller.ToolModel.OutputLibraryIds.Count == 1)
            {
                var lem = SessionController.Instance.ContentController.GetLibraryElementController(Controller.ToolModel.OutputLibraryIds.First());
                SessionController.Instance.NuSessionView.ShowDetailView(lem);
            }
            
        }

        /// <summary>
        /// Returns a boolean representing if creating a tool chain from this tool to the passed in tool will create a loop
        /// </summary>
        public bool CreatesLoop(ToolViewModel toolViewModel)
        {
            bool createsLoop = false;
            var controllers = new List<ToolStartable>(Controller.ToolModel.ParentIds.Select(item => ToolController.ToolControllers.ContainsKey(item) ? ToolController.ToolControllers[item] : null));

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
        /// Switches from basic tool view to all metadata tool view. Transfers all parents from basic tool view to metadata toolview. Fires events to let 
        /// children know they have a new parent and let the links know to replace the basic tool view with the new metadata tool view. After, it disposes of the 
        /// basic tool.
        /// </summary>
        public async Task SwitchToAllMetadataTool(float x, float y)
        {
            if ((this as MetadataToolViewModel) == null)
            {
                await UITask.Run(() =>
                {
                    MetadataToolModel model = new MetadataToolModel();
                    MetadataToolController controller = new MetadataToolController(model);
                    MetadataToolViewModel viewmodel = new MetadataToolViewModel(controller);
                    viewmodel.Filter = ToolModel.ToolFilterTypeTitle.AllMetadata;
                    foreach (var id in Controller.GetParentIds())
                    {
                        controller.AddParent(ToolController.ToolControllers[id]);
                        var parentController = ToolController.ToolControllers[id] as ElementController;
                        Debug.Assert(parentController != null);
                        if (parentController != null)
                        {
                            var linkModel = new ToolLinkModel();
                            linkModel.InAtomId = parentController.Id;
                            linkModel.OutAtomId = model.Id;
                            var linkController = new ToolLinkController(linkModel, parentController, controller);
                            var linkViewModel = new ToolLinkViewModelWin2d(linkController);
                            SessionController.Instance.ActiveFreeFormViewer.AddToolLink(linkViewModel);
                        }
                    }
                    controller.SetSize(500, 500);
                    controller.SetPosition(x, y);
                    SessionController.Instance.ActiveFreeFormViewer.AddTool(viewmodel);
                });
                //var wvm = SessionController.Instance.ActiveFreeFormViewer;
                //wvm.AtomViewList.Add(view);

                //Controller.FireFilterTypeAllMetadataChanged(viewmodel);
                //this.FireReplacedToolLinkAnchorPoint(viewmodel);
            }
            //this.Dispose();
        }

        //Switches to the basic tool view from metadatatoolview. It will not do anything if it is already a basic tool
        public async Task SwitchToBasicTool(ToolModel.ToolFilterTypeTitle filter, float x, float y)
        {
            if ((this as BasicToolViewModel) == null)
            {
                await UITask.Run(() =>
                {
                    BasicToolModel model = new BasicToolModel();
                    BasicToolController controller = new BasicToolController(model);
                    BasicToolViewModel viewmodel = new BasicToolViewModel(controller);
                    foreach (var id in Controller.GetParentIds())
                    {
                        controller.AddParent(ToolController.ToolControllers[id]);

                        var parentController = ToolController.ToolControllers[id] as ElementController;
                        Debug.Assert(parentController != null);
                        if (parentController != null)
                        {
                            var linkModel = new ToolLinkModel();
                            linkModel.InAtomId = parentController.Id;
                            linkModel.OutAtomId = model.Id;
                            var linkController = new ToolLinkController(linkModel, parentController, controller);
                            var linkViewModel = new ToolLinkViewModelWin2d(linkController);
                            SessionController.Instance.ActiveFreeFormViewer.AddToolLink(linkViewModel);
                        }
                    }
                    viewmodel.Filter = filter;
                    controller.SetSize(500,500);
                    controller.SetPosition(x, y);

                    SessionController.Instance.ActiveFreeFormViewer.AddTool(viewmodel);
                });
                //var wvm = SessionController.Instance.ActiveFreeFormViewer;
                //wvm.AtomViewList.Add(view);

                //Controller.FireFilterTypeAllMetadataChanged(viewmodel);
                //this.FireReplacedToolLinkAnchorPoint(viewmodel);
            }
        }

        /// <summary>
        /// Will either add this tool as a parent if dropped on top of an existing tool, or create a brand new tool filter chooser view.
        /// </summary>
        public void FilterIconDropped(ToolWindow dragDestination, double x, double y)
        {
            //if (hitsStart.Where(uiElem => (uiElem is FrameworkElement) && (uiElem as FrameworkElement).DataContext is ToolViewModel).ToList().Any())
            //{
            //    var hitsStartList = hitsStart.Where(uiElem => (uiElem is AnimatableUserControl) && (uiElem as AnimatableUserControl).DataContext is ToolViewModel).ToList();
            //    AddFilterToExistingTool(hitsStartList, wvm);
            //}
            //else if (hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList().Any())
            //{
            //    var hitsStartList = hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList();
            //    AddFilterToFilterToolView(hitsStartList, wvm);
            //}

            if (dragDestination != null)
            {
                var toolViewModel = dragDestination.Vm;
                AddFilterToExistingTool(toolViewModel, null); //FIX THIS SHIT
            }
            else
            {
                AddNewFilterTool(x, y);
            }
        }

        /// <summary>
        ///creates new filter tool at specified location
        /// </summary>
        public void AddNewFilterTool(double x, double y)
        {
            //var toolFilter = new ToolFilterView(x, y, this);

            //var linkviewmodel = new ToolLinkViewModelWin2d(this, toolFilter);
            //var link = new ToolLinkView(linkviewmodel);

            //Canvas.SetZIndex(link, Canvas.GetZIndex(toolFilter) - 1);
            //wvm.AtomViewList.Add(toolFilter);
            //wvm.AtomViewList.Add(link);
            UITask.Run(() =>
            {

                BasicToolModel model = new BasicToolModel();
                BasicToolController controller = new BasicToolController(model);
                BasicToolViewModel viewmodel = new BasicToolViewModel(controller);
                viewmodel.Filter = ToolModel.ToolFilterTypeTitle.Title;
                viewmodel.Controller.AddParent(Controller);
                controller.SetPosition(x, y);
                controller.SetSize(500, 500);

                var linkModel = new ToolLinkModel();
                linkModel.InAtomId = this.Id;
                linkModel.OutAtomId = model.Id;
                var linkController = new ToolLinkController(linkModel, this.Controller, controller);
                var linkViewModel = new ToolLinkViewModelWin2d(linkController);
                

                //Controller.SetPosition(X, Y);
                SessionController.Instance.ActiveFreeFormViewer.AddTool(viewmodel);
                SessionController.Instance.ActiveFreeFormViewer.AddToolLink(linkViewModel);


            });
        }

        ///// <summary>
        /////Adds tool as parent to existing filter picker tool. 
        ///// </summary>
        //public void AddFilterToFilterToolView(List<UIElement> hitsStartList, FreeFormViewerViewModel wvm)
        //{
        //    var linkviewmodel = new ToolLinkViewModel(this, (hitsStartList.First() as ToolFilterView));
        //    var linkView = new ToolLinkView(linkviewmodel);
        //    Canvas.SetZIndex(linkView, Canvas.GetZIndex(hitsStartList.First()) - 1);
        //    (hitsStartList.First() as ToolFilterView).AddParentTool(this);
        //    wvm.AtomViewList.Add(linkView);
        //}

        /// <summary>
        ///Adds tool as parent to existing tool. 
        /// </summary>
        public void AddFilterToExistingTool(ToolViewModel toolViewModel, FreeFormViewerViewModel wvm)
        {
            if (toolViewModel != null && toolViewModel != this)
            {
                if (!CreatesLoop(toolViewModel))
                {
                    //var linkviewmodel = new ToolLinkViewModelWin2d(this, toolViewModel);
                    //var link = new ToolLinkView(linkviewmodel);
                    //Canvas.SetZIndex(link, Canvas.GetZIndex(hitsStartList.First()) - 1);
                    //wvm.AtomViewList.Add(link);
                    var linkModel = new ToolLinkModel();
                    linkModel.InAtomId = this.Id;
                    linkModel.OutAtomId = toolViewModel.Id;
                    var linkController = new ToolLinkController(linkModel, this.Controller, toolViewModel.Controller);
                    var linkViewModel = new ToolLinkViewModelWin2d(linkController);
                    SessionController.Instance.ActiveFreeFormViewer.AddToolLink(linkViewModel);
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

        /// <summary>
        /// Just reloads the properties to display based on the output library ids of the parents
        /// </summary>
        /// <param name="recursivelyRefresh"></param>
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
            return;
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