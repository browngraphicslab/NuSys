using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using NuSysApp.Tools;


namespace NuSysApp
{
    public class ElementCollectionViewModel: ElementViewModel, ToolStartable
    {
        public List<ISelectable> Selections { get; private set; } = new List<ISelectable>();

        public static Dictionary<ElementViewModel, IRandomAccessStream> Mems = new Dictionary<ElementViewModel, IRandomAccessStream>(); 

        public string Text { get; set; }
        public event EventHandler<HashSet<string>> OutputLibraryIdsChanged;
        public event EventHandler<string> Disposed;

        public delegate void SelectionChangedHandler(object source);
        public event SelectionChangedHandler SelectionChanged;
        /// <summary>
        /// The unique ID used in the tool startable dictionary
        /// </summary>
        private string _toolStartableId;

        public ObservableCollection<FrameworkElement> AtomViewList { get; set; } = new ObservableCollection<FrameworkElement>();
        public ObservableCollection<ElementViewModel> Elements { get; set; } = new ObservableCollection<ElementViewModel>();
        public ObservableCollection<LinkViewModel> Links { get; set; } = new ObservableCollection<LinkViewModel>();

        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
        protected FreeFormElementViewModelFactory _elementVmFactory = new FreeFormElementViewModelFactory();
       
        public ElementCollectionViewModel(ElementCollectionController controller): base(controller)
        {
            controller.ChildAdded += OnChildAdded;
            controller.ChildRemoved += OnChildRemoved;
            //(controller.LibraryElementModel as CollectionLibraryElementModel).OnLinkAdded += OnOnLinkAdded;
            //(controller.LibraryElementModel as CollectionLibraryElementModel).OnLinkRemoved += ElementCollectionViewModel_OnLinkRemoved;
            Text = controller.LibraryElementModel?.Data;

            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
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

        public void DeselectAll()
        {
            ClearSelection();
        }


        /// <summary>
        /// Sets the passed in Atom as selected. If there atlready is a selected Atom, the old \
        /// selection and the new selection are linked.
        /// </summary>
        /// <param name="selected"></param>
        public void AddSelection(ISelectable selected)
        {
            if (!Selections.Contains(selected))
            {
                selected.IsSelected = true;
                Selections.Add(selected);
            }
            else
            {
                selected.IsSelected = false;
                Selections.Remove(selected);
            }
            SelectionChanged?.Invoke(this);
        }

        public void RemoveSelection(ISelectable selected)
        {
            selected.IsSelected = false;
            Selections.Remove(selected);
            SelectionChanged?.Invoke(this);
        }

        /// <summary>
        /// Unselects the currently selected node.
        /// </summary> 
        public void ClearSelection()
        {
            foreach (var selectable in Selections)
            {
                selectable.IsSelected = false;
            }
            Selections.Clear();
            SelectionChanged?.Invoke(this);
        }

        private async void OnChildAdded(object source, ElementController elementController)
        {
            await CreateChild(elementController);
            OutputLibraryIdsChanged?.Invoke(this, GetOutputLibraryIds());
        }

        private async Task CreateChild(ElementController controller)
        {

            var vm = await _elementVmFactory.CreateFromSendable(controller);
            Elements.Add(vm);

            /*
          var view = await _nodeViewFactory.CreateFromSendable(controller);
          AtomViewList.Add(view);
            if (view is TextNodeView)
            {
                var tview = (TextNodeView) view;
                InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream();
                await tview.XWebView.CapturePreviewToStreamAsync(ms);

                // Create a small thumbnail.
                int longlength = 180, width = 0, height = 0;
                double srcwidth = tview.XWebView.ActualWidth, srcheight = tview.XWebView.ActualHeight;
                double factor = srcwidth / srcheight;
                if (factor < 1)
                {
                    height = longlength;
                    width = (int)(longlength * factor);
                }
                else
                {
                    width = longlength;
                    height = (int)(longlength / factor);
                }

                Mems.Add(vm, ms.CloneStream());
            }

        



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

    */

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

        /// <summary>
        /// Returns list of elements within the collection as the output library ids
        /// </summary>
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

        /// <summary>
        /// Returns an empty hashset because a collection has no parents
        /// </summary>
        public HashSet<string> GetParentIds()
        {
            return new HashSet<string>();
        }
    }
}