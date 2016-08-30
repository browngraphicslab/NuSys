using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NusysIntermediate;
using NuSysApp.Tools;


namespace NuSysApp
{
    public class ElementCollectionViewModel: ElementViewModel, ToolStartable
    {
        public event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        public event EventHandler<string> Disposed;
        public event EventHandler<ToolViewModel> FilterTypeAllMetadataChanged;

        public ObservableCollection<ElementViewModel> Elements { get; set; } = new ObservableCollection<ElementViewModel>();
        public ObservableCollection<LinkViewModel> Links { get; set; } = new ObservableCollection<LinkViewModel>();

        public ObservableCollection<PresentationLinkViewModel> Trails { get; set; } = new ObservableCollection<PresentationLinkViewModel>();
        /// <summary>
        /// The unique ID used in the tool startable dictionary
        /// </summary>
        private string _toolStartableId;

        public ObservableCollection<FrameworkElement> AtomViewList { get; set; } 
        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
        protected FreeFormElementViewModelFactory _elementVmFactory = new FreeFormElementViewModelFactory();

        public Vector2 CameraTranslation { get; set; } = new Vector2(-Constants.MaxCanvasSize / 2f, -Constants.MaxCanvasSize / 2f);
        public Vector2 CameraCenter { get; set; } = new Vector2(Constants.MaxCanvasSize / 2f, Constants.MaxCanvasSize / 2f);
        public float CameraScale { get; set; } = 1f;

        public bool IsFinite { get; set; }
        public bool IsShaped { get; set; }

        public ElementCollectionViewModel(ElementCollectionController controller): base(controller)
        {
            controller.ChildAdded += OnChildAdded;
            controller.ChildRemoved += OnChildRemoved;
            controller.CameraPositionChanged += ControllerOnCameraPositionChanged;
            controller.CameraCenterChanged += ControllerOnCameraCenterChanged;

            var model = (CollectionLibraryElementModel) controller.LibraryElementModel;
            IsFinite = model.IsFinite;
            IsShaped = model.ShapePoints != null && model.ShapePoints.Count > 5;
            
            //(libraryElementController.LibraryElementModel as CollectionLibraryElementModel).OnLinkAdded += OnOnLinkAdded;
            //(libraryElementController.LibraryElementModel as CollectionLibraryElementModel).OnLinkRemoved += ElementCollectionViewModel_OnLinkRemoved;

            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            AtomViewList = new ObservableCollection<FrameworkElement>();
            _toolStartableId = SessionController.Instance.GenerateId();
            ToolController.ToolControllers.Add(_toolStartableId, this);
        }

        private void ControllerOnCameraCenterChanged(float f, float f1)
        {
         //   Debug.WriteLine("center chagnes");
            CameraCenter = new Vector2(f, f1);
        }

        private void ControllerOnCameraPositionChanged(float f, float f1)
        {
            CameraTranslation = new Vector2(f, f1);

        }

        public async Task CreateChildren()
        {
            var model = (CollectionLibraryElementModel) Controller.LibraryElementModel;
            foreach (var id in model.Children )
            {
                var childController = SessionController.Instance.IdToControllers[id];
                await CreateChild(childController);
            }
        }

        public override void Dispose()
        {
            var controller = (ElementCollectionController) Controller;
            controller.ChildAdded -= OnChildAdded;
            controller.ChildRemoved -= OnChildRemoved;
            base.Dispose();
            Disposed?.Invoke(this, _toolStartableId);
            ToolController.ToolControllers.Remove(_toolStartableId);

        }

        private async void OnChildAdded(object source, ElementController elementController)
        {
            await CreateChild(elementController);
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());
        }

        private async Task CreateChild(ElementController controller)
        {

            if (controller is ElementCollectionController && this is AreaNodeViewModel) {
                Debug.WriteLine($"Depth of Recursion {(controller as ElementCollectionController).Depth}");
                if ((controller as ElementCollectionController).Depth >= Constants.GroupViewRecursionDepth)
                {
                    return;
                }
            (controller as ElementCollectionController).Depth++;
            }
            var view = await _nodeViewFactory.CreateFromSendable(controller);

            var vm = await _elementVmFactory.CreateFromSendable(controller);
            Elements.Add(vm);
            if (controller is LinkController)
            {
                return;
            }

            controller.Deleted += OnChildDeleted;
        }
        
        private void OnChildDeleted(object source)
        {
            var c = (ElementCollectionController) Controller;
            c.RemoveChild((ElementController)source);
            var model = (CollectionElementModel) Model;
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());

        }

        private void OnChildRemoved(object source, ElementController elementController)
        {
            //FuckYouSahilRemoveAllVisualLinks(elementController);

            var soughtChildren = Elements.Where(a => a.Id == elementController.Model.Id);
            if (soughtChildren.Any())
            {
              //  AtomViewList.Remove( soughtChildren.First());
                Elements.Remove(soughtChildren.First());
            }
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());
        }

        /// <summary>
        /// Returns list of elements within the collection as the output library ids
        /// </summary>
        public HashSet<string> GetOutputLibraryIds()
        {
            var libraryElementIds = new HashSet<string>();
            var collectionLibraryElementModel =
                SessionController.Instance.ContentController.GetLibraryElementModel(Model.LibraryId) as
                    CollectionLibraryElementModel;
            foreach (var node in collectionLibraryElementModel.Children)
            {
                if (SessionController.Instance.IdToControllers.ContainsKey(node))
                {
                    libraryElementIds.Add(
                        SessionController.Instance.IdToControllers[node]?
                            .LibraryElementModel?.LibraryElementId);
                }
            }
            return libraryElementIds;
        }


        public string GetID()
        {
            return _toolStartableId;
        }

        /// <summary>
        /// Returns an empty hashset because a collection has no parents
        /// </summary>
        public HashSet<string> GetParentIds()
        {
            return new HashSet<string>();
        }
    }
}