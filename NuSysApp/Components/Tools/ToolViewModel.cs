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


        protected ToolController _controller;
        private double _width;
        private double _height;
        private double _x;
        private double _y;
        private CompositeTransform _transform = new CompositeTransform();
        private Point2d _anchor;
        public ObservableCollection<ToolModel.ParentOperatorType> ParentOperatorList = new ObservableCollection<ToolModel.ParentOperatorType>() {ToolModel.ParentOperatorType.And, ToolModel.ParentOperatorType.Or};
       
        public ToolController Controller { get { return _controller; } }
        public ToolViewModel(ToolController toolController):base(toolController)
        {
            _controller = toolController;
            _controller.IdsToDisplayChanged += ControllerOnLibraryIdsToDisplayChanged;
        }
        

        /// <summary>
        /// So that sublcasses can invoke properties to display changed event
        /// </summary>
        public void InvokePropertiesToDisplayChanged()
        {
            PropertiesToDisplayChanged?.Invoke();
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
        /// Switches from basic tool view to all metadata tool view. Transfers all parents from basic tool view to metadata toolview. Fires events to let 
        /// children know they have a new parent and let the links know to replace the basic tool view with the new metadata tool view. After, it disposes of the 
        /// basic tool.
        /// </summary>
        public async Task SwitchToAllMetadataTool()
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
                            //var linkModel = new ToolLinkModel();
                            //linkModel.InAtomId = parentController.Id;
                            //linkModel.OutAtomId = model.Id;
                            //var linkController = new ToolLinkController(linkModel, parentController, controller);
                            //var linkViewModel = new ToolLinkViewModelWin2d(linkController);
                            //SessionController.Instance.ActiveFreeFormViewer.AddToolLink(linkViewModel);
                        }
                    }
                    controller.SetSize(Width, Height);
                    controller.SetPosition(X, Y);
                    SessionController.Instance.ActiveFreeFormViewer.AddTool(viewmodel);
                    Controller.FireFilterTypeAllMetadataChanged(viewmodel);
                });
            }
        }

        //Switches to the basic tool view from metadatatoolview. It will not do anything if it is already a basic tool
        public async Task SwitchToBasicTool(ToolModel.ToolFilterTypeTitle filter)
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
                        var parentController = ToolController.ToolControllers[id] as ToolStartable;
                        Debug.Assert(parentController != null);
                        controller.AddParent(parentController);
                    }
                    viewmodel.Filter = filter;
                    controller.SetSize(Width, Height);
                    controller.SetPosition(X, Y);

                    SessionController.Instance.ActiveFreeFormViewer.AddTool(viewmodel);
                    Controller.FireFilterTypeAllMetadataChanged(viewmodel);

                });
            }
        }

        /// <summary>
        /// Removes all the listeners and calls dispose on the controller
        /// </summary>
        public override void Dispose()
        {
            _controller.IdsToDisplayChanged -= ControllerOnLibraryIdsToDisplayChanged;
            Controller.SizeChanged -= OnSizeChanged;
            //Controller.Dispose();
            base.Dispose();
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

    }
}