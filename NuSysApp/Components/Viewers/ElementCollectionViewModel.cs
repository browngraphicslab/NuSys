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
using Windows.UI.Xaml.Media.Animation;
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
        protected FreeFormElementViewModelFactory _elementVmFactory = new FreeFormElementViewModelFactory();

        public Vector2 CameraTranslation { get; set; } = new Vector2(-Constants.MaxCanvasSize / 2f, -Constants.MaxCanvasSize / 2f);
        public Vector2 CameraCenter { get; set; } = new Vector2(Constants.MaxCanvasSize / 2f, Constants.MaxCanvasSize / 2f);
        public float CameraScale { get; set; } = 1f;

        public bool IsFinite { get; set; }
        public bool IsShaped { get; set; }
        public double AspectRatio { get; set; }
        public Color ShapeColor { get; set; } = Colors.Black;
        public String ImageBackground { get; set; }

        public ElementCollectionViewModel(ElementCollectionController controller): base(controller)
        {
            controller.ChildAdded += OnChildAdded;
            controller.ChildRemoved += OnChildRemoved;
            controller.CameraPositionChanged += ControllerOnCameraPositionChanged;
            controller.CameraCenterChanged += ControllerOnCameraCenterChanged;

            var libElemController = (CollectionLibraryElementController) controller.LibraryElementController;
            libElemController.LinkAdded += LibraryElementControllerOnLinkAdded;
            libElemController.OnTrailAdded += LibElemControllerOnOnTrailAdded;
            libElemController.OnTrailRemoved += LibElemControllerOnOnTrailRemoved;

            var model = (CollectionLibraryElementModel) controller.LibraryElementModel;
            IsFinite = model.IsFinite;
            IsShaped = model.ShapePoints != null && model.ShapePoints.Count > 5;
            AspectRatio = model.AspectRatio;

            if (model.ShapeColor != null)
            {
                ShapeColor = model.ShapeColor.ToColor();
            }

            if (model.ImageBackground != null)
            {
                ImageBackground = model.ImageBackground;
            }


            foreach (var childId in model.Children)
            {
                var childController =  SessionController.Instance.IdToControllers[childId];
                Debug.Assert(childController != null);
                CreateChild(childController);
            }

            foreach (var vm in SessionController.Instance.LinksController.GetLinkViewModel(controller.LibraryElementModel.LibraryElementId))
            {
                if (!Links.Contains(vm))
                    Links.Add(vm);   
            }

            foreach (var vm in SessionController.Instance.LinksController.GetTrailViewModel(controller.LibraryElementModel.LibraryElementId))
            {
                if (!Trails.Contains(vm))
                    Trails.Add(vm);
            }


            AtomViewList = new ObservableCollection<FrameworkElement>();
            _toolStartableId = SessionController.Instance.GenerateId();
            ToolController.ToolControllers.Add(_toolStartableId, this);
        }

        private void LibElemControllerOnOnTrailAdded(PresentationLinkViewModel vm)
        {
            Trails.Add(vm);
        }

        private void LibElemControllerOnOnTrailRemoved(PresentationLinkViewModel vm)
        {
            Trails.Remove(vm);
        }

        private void LibraryElementControllerOnLinkAdded(object sender, LinkViewModel linkViewModel)
        {
            Links.Add(linkViewModel);
        }

        private void ControllerOnCameraCenterChanged(float f, float f1)
        {
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

            var libElemController = (CollectionLibraryElementController)controller.LibraryElementController;
            libElemController.LinkAdded -= LibraryElementControllerOnLinkAdded;
            libElemController.OnTrailAdded -= LibElemControllerOnOnTrailAdded;
            libElemController.OnTrailRemoved -= LibElemControllerOnOnTrailRemoved;

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

            if (controller is ElementCollectionController) {
                Debug.WriteLine($"Depth of Recursion {(controller as ElementCollectionController).Depth}");
                if ((controller as ElementCollectionController).Depth >= Constants.GroupViewRecursionDepth)
                {
                    return;
                }
                (controller as ElementCollectionController).Depth++;
            }
           
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
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());

        }

        private void OnChildRemoved(object source, ElementController elementController)
        {
            //FuckYouSahilRemoveAllVisualLinks(elementController);

            var soughtChildren = Elements.Where(a => a.Id == elementController.Model.Id);
            if (soughtChildren.Any())
            {
                //  AtomViewList.Remove( soughtChildren.First());
                var soughtChild = soughtChildren.First();
                Elements.Remove(soughtChild);

                foreach (var linkViewModel in Links.ToList())
                {
                    if (linkViewModel.LinkModel.InAtomId == soughtChild.Id ||
                        linkViewModel.LinkModel.OutAtomId == soughtChild.Id)
                    {
                        Links.Remove(linkViewModel);
                    }
                }

                foreach (var trail in Trails.ToList())
                {
                    if (trail.Model.InElementId == soughtChild.Id ||
                        trail.Model.OutElementId == soughtChild.Id)
                    {
                        trail.DeletePresentationLink();
                        Trails.Remove(trail);
                    }
                }
            }
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());
        }

        protected override void OnSizeChanged(object source, double width, double height)
        {

            if (!IsFinite)
            {
                base.OnSizeChanged(source, width,height);
                return;
            }
            if (height *  AspectRatio < Constants.MinNodeSize)
            {
                return; // If the height becomes smaller than the minimum node size then we don't apply the size changed, applying the height's change causes weird behaviour
            }

            SetSize(height * AspectRatio, height);

        }

        /// <summary>
        /// Returns list of elements within the collection as the output library ids
        /// The bool recursively reload does nothing because the collection can only ever be the
        /// start of a tool chain.
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

        /// <summary>
        /// 
        /// </summary>
        public void RefreshFromTopOfChain()
        {
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());
        }

        /// <summary>
        /// Returns the tool startable id that is used in the dictionary from id to controller.
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// since this collection has no parents to merge, this method jsut returns GetOutputLibraryIds().
        /// </summary>
        /// <param name="recursiveRefresh"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUpdatedDataList()
        {
            return GetOutputLibraryIds();
        }
    }
}