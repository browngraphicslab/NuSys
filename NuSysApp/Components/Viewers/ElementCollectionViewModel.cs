using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using NuSysApp.Tools;


namespace NuSysApp
{
    public class ElementCollectionViewModel: ElementViewModel, ToolStartable
    {
        public string Text { get; set; }
        public event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        public event EventHandler<string> Disposed;
        private string _toolStartableId;

        public ObservableCollection<FrameworkElement> AtomViewList { get; set; } 
        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
       
        public ElementCollectionViewModel(ElementCollectionController controller): base(controller)
        {
            controller.ChildAdded += OnChildAdded;
            controller.ChildRemoved += OnChildRemoved;
            //(controller.LibraryElementModel as CollectionLibraryElementModel).OnLinkAdded += OnOnLinkAdded;
            //(controller.LibraryElementModel as CollectionLibraryElementModel).OnLinkRemoved += ElementCollectionViewModel_OnLinkRemoved;
            Text = controller.LibraryElementModel?.Data;

            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            AtomViewList = new ObservableCollection<FrameworkElement>();
            _toolStartableId = SessionController.Instance.GenerateId();
            ToolController.ToolControllers.Add(_toolStartableId, this);
            
            
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
            var view = await _nodeViewFactory.CreateFromSendable(controller);
            AtomViewList.Add(view);
            if (controller is LinkController)
            {
                return;
            }
            foreach (var regions in controller?.LibraryElementModel?.Regions ?? new HashSet<Region>()) 
            {
                RegionController regionController;

                if (SessionController.Instance.RegionsController.GetRegionController(regions.Id) == null)
                {
                    regionController = SessionController.Instance.RegionsController.AddRegion(regions, controller.LibraryElementModel.LibraryElementId);
                }
                else
                {
                    regionController = SessionController.Instance.RegionsController.GetRegionController(regions.Id);
                }
                var cLinks = SessionController.Instance.LinksController.GetLinkedIds(regionController.ContentId);
                foreach (var linkId in cLinks)
                {
                    var link = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                    //AddVisualLinks(regioncontroller, controller, link.LibraryElementId);
                }
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
            var soughtChildren = AtomViewList.Where(a => a.DataContext is ElementViewModel && ((ElementViewModel) a.DataContext).Id == elementController.Model.Id);
            if (soughtChildren.Any())
            {
                AtomViewList.Remove( soughtChildren.First());
            }
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());
        }
        public HashSet<string> GetOutputLibraryIds()
        {
            var libraryElementIds = new HashSet<string>();
            var collectionLibraryElementModel =
                SessionController.Instance.ContentController.GetContent(Model.LibraryId) as
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

        public HashSet<string> GetParentIds()
        {
            return new HashSet<string>();
        }
    }
}