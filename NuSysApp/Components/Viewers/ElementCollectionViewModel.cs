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


namespace NuSysApp
{
    public class ElementCollectionViewModel: ElementViewModel
    {
        public string Text { get; set; }


        public ObservableCollection<FrameworkElement> AtomViewList { get; set; } 
        protected INodeViewFactory _nodeViewFactory = new FreeFormNodeViewFactory();
       
        public ElementCollectionViewModel(ElementCollectionController controller): base(controller)
        {
            controller.ChildAdded += OnChildAdded;
            controller.ChildRemoved += OnChildRemoved;
            (controller.LibraryElementModel as CollectionLibraryElementModel).OnLinkAdded += OnOnLinkAdded;
            (controller.LibraryElementModel as CollectionLibraryElementModel).OnLinkRemoved += ElementCollectionViewModel_OnLinkRemoved;
            Text = controller.LibraryElementModel?.Data;

            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            AtomViewList = new ObservableCollection<FrameworkElement>();
        }

        private void ElementCollectionViewModel_OnLinkRemoved(string id)
        {
            RemoveVisualLink(id);
        }

        private void OnOnLinkAdded(string id)
        {
            var atoms = new HashSet<FrameworkElement>();
            foreach (var atom in AtomViewList.Where(e=>!(e.DataContext is LinkViewModel)))
            {
                atoms.Add(atom);
            }
            foreach (var atom in atoms)
            {
                var controller = (atom.DataContext as ElementViewModel).Controller;
                var contentLinks =
                    SessionController.Instance.LinkController.GetLinkedIds(
                        new LinkId(controller.LibraryElementModel.LibraryElementId));
                foreach (var linkId in contentLinks)
                {
                    
                    var link =
                        SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                    AddVisualLinks(controller, link.LibraryElementId);
                }
            }
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
        }

        private async void OnChildAdded(object source, ElementController elementController)
        {
            await CreateChild(elementController);
        }

        private async Task CreateChild(ElementController controller)
        {
            var view = await _nodeViewFactory.CreateFromSendable(controller);
            AtomViewList.Add(view);
            var contentLinks = SessionController.Instance.LinkController.GetLinkedIds(new LinkId(controller.LibraryElementModel.LibraryElementId));
            foreach (var linkId in contentLinks)
            {
                var link = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                AddVisualLinks(controller,link.LibraryElementId);
            }
            foreach (var regions in controller.LibraryElementModel.Regions)
            {
                var regioncontroller = SessionController.Instance.RegionsController.GetRegionController(regions.Id);
                 var cLinks = SessionController.Instance.LinkController.GetLinkedIds(regioncontroller.Id);
                foreach (var linkId in cLinks)
                {
                    var link = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                    AddVisualLinks(regioncontroller, controller, link.LibraryElementId);
                }
            }
            controller.Deleted += OnChildDeleted;
        }

        private void AddVisualLinks(ElementController controller,string id)
        {
            if (controller is LinkElementController)
            {
                return;
            }
            var link = SessionController.Instance.LinkController.GetLinkLibraryElementController(id);
            
            foreach (var atom in new HashSet<FrameworkElement>(AtomViewList))
            {
                var dc = (atom.DataContext as ElementViewModel);
                foreach (var region in (atom.DataContext as ElementViewModel).Controller.LibraryElementModel.Regions)
                {
                    //  if (dc.Controller == controller) continue;
                    var regioncontroller =
    SessionController.Instance.RegionsController.GetRegionController(region.Id);
                    if (regioncontroller.Id ==
                        link.LinkLibraryElementModel.InAtomId ||
                        regioncontroller.Id ==
                        link.LinkLibraryElementModel.OutAtomId)
                    {
                        var isAlreadyMade = false;
                        foreach (var linkvms in AtomViewList.Where(r => r.DataContext is LinkViewModel).Select(e => e.DataContext as LinkViewModel))
                        {

                            if ((linkvms.Controller.Model as LinkModel).InAtomId == controller.LibraryElementController.Id && (linkvms.Controller.Model as LinkModel).OutAtomId == regioncontroller.Id ||
                                (linkvms.Controller.Model as LinkModel).OutAtomId == controller.LibraryElementController.Id && (linkvms.Controller.Model as LinkModel).InAtomId == regioncontroller.Id)
                            {
                                isAlreadyMade = true;
                            }
                        }
                        if (isAlreadyMade) continue;
                        var lm = new LinkModel(SessionController.Instance.GenerateId());
                        lm.InAtomId = new LinkId(controller.Model.Id);
                        lm.OutAtomId = new LinkId(dc.Model.Id,region.Id);
                        lm.LibraryId = id;
                        var lc = new LinkElementController(lm);
                        var view = new BezierLinkView(new LinkViewModel(lc));
                        AtomViewList.Add(view);

                    }
                }
                Debug.Assert(dc != null);
              //  if (dc.Controller == controller) continue;
                if (dc.Controller.LibraryElementController.Id ==
                    link.LinkLibraryElementModel.InAtomId ||
                    dc.Controller.LibraryElementController.Id ==
                    link.LinkLibraryElementModel.OutAtomId)
                {
                     var isAlreadyMade = false;
                     foreach (var linkvms in AtomViewList.Where(r => r.DataContext is LinkViewModel).Select(e=>e.DataContext as LinkViewModel))
                     {
                         if ((linkvms.Controller.Model as LinkModel).InAtomId == controller.LibraryElementController.Id && (linkvms.Controller.Model as LinkModel).OutAtomId == dc.Controller.LibraryElementController.Id || 
                             (linkvms.Controller.Model as LinkModel).OutAtomId == controller.LibraryElementController.Id && (linkvms.Controller.Model as LinkModel).InAtomId == dc.Controller.LibraryElementController.Id)
                         {
                             isAlreadyMade = true;
                         }
                     }
                     if (isAlreadyMade) continue;
                    var lm = new LinkModel(SessionController.Instance.GenerateId());
                    lm.InAtomId = new LinkId(controller.Model.Id);
                    lm.OutAtomId = new LinkId(dc.Controller.Model.Id);
                    lm.LibraryId = id;
                    var lc = new LinkElementController(lm);
                    var view = new BezierLinkView(new LinkViewModel(lc));
                    AtomViewList.Add(view);

                }
            }
        }
        private void AddVisualLinks(RegionController controller, ElementController elementController, string id)
        {
            if (elementController is LinkElementController)
            {
                return;
            }
            var link = SessionController.Instance.LinkController.GetLinkLibraryElementController(id);

            foreach (var atom in new HashSet<FrameworkElement>(AtomViewList))
            {
                var dc = (atom.DataContext as ElementViewModel);
                foreach (var region in (atom.DataContext as ElementViewModel).Controller.LibraryElementModel.Regions)
                {
                    //  if (dc.Controller == controller) continue;
                    var regioncontroller =
    SessionController.Instance.RegionsController.GetRegionController(region.Id);
                    if (regioncontroller.Id == controller.Id)
                    {
                      //  continue;
                    }
                    if (regioncontroller.Id ==
                        link.LinkLibraryElementModel.InAtomId ||
                        regioncontroller.Id ==
                        link.LinkLibraryElementModel.OutAtomId)
                    {

                        var lm = new LinkModel(SessionController.Instance.GenerateId());
                        lm.InAtomId = new LinkId(elementController.Model.Id, controller.Model.Id);
                        lm.OutAtomId = new LinkId(dc.Model.Id, region.Id);
                        lm.LibraryId = id;
                        var lc = new LinkElementController(lm);
                        var view = new BezierLinkView(new LinkViewModel(lc));
                        AtomViewList.Add(view);

                    }
                }
                Debug.Assert(dc != null);
                //  if (dc.Controller == controller) continue;
                if (dc.Controller.LibraryElementController.Id ==
                    link.LinkLibraryElementModel.InAtomId ||
                    dc.Controller.LibraryElementController.Id ==
                    link.LinkLibraryElementModel.OutAtomId)
                {
                    var lm = new LinkModel(SessionController.Instance.GenerateId());
                    lm.InAtomId = new LinkId(elementController.Model.Id,controller.Model.Id);
                    lm.OutAtomId = new LinkId(dc.Controller.Model.Id);
                    lm.LibraryId = id;
                    var lc = new LinkElementController(lm);
                    var view = new BezierLinkView(new LinkViewModel(lc));
                    AtomViewList.Add(view);

                }
            }
        }
        private void RemoveAllVisualLinks(ElementController controller)
        {
            if (controller is LinkElementController)
            {
                return;
            }
            var contentLinks = SessionController.Instance.LinkController.GetLinkedIds(new LinkId(controller.LibraryElementModel.LibraryElementId));
            var toLinkIds = new HashSet<LinkId>();
            foreach (var linkId in contentLinks)
            {
                var link = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                toLinkIds.Add(link.InAtomId.LibraryElementId == controller.LibraryElementModel.LibraryElementId ? link.OutAtomId : link.InAtomId);
            }
            var toRemoveAtoms = new HashSet<FrameworkElement>();
            foreach (var atom in AtomViewList.Where(r => (r.DataContext is LinkViewModel) && r.DataContext != null))
            {
                if (contentLinks.Contains((atom.DataContext as LinkViewModel).LinkModel.LibraryId))
                {
                    toRemoveAtoms.Add(atom);
                }
            }

            foreach (var toRemove in toRemoveAtoms)
            {
                AtomViewList.Remove(toRemove);
            }
            
        }

        public void RemoveVisualLink(string id)
        {
            var link = SessionController.Instance.LinkController.GetLinkLibraryElementController(id);
            foreach (var atom in new HashSet<FrameworkElement>(AtomViewList.Where(e=> e.DataContext is LinkViewModel)))
            {
                var vm = atom.DataContext as LinkViewModel;
                var linkModel = (vm?.Controller?.Model as LinkModel);
                Debug.Assert(linkModel != null);
                if (linkModel.LibraryId == link.LibraryElementModel.LibraryElementId)
                {
                    AtomViewList.Remove(atom);
                }
            }
        }
        private void OnChildDeleted(object source)
        {
            var c = (ElementCollectionController) Controller;
            c.RemoveChild((ElementController)source);
            var model = (CollectionElementModel) Model;
        }

        private void OnChildRemoved(object source, ElementController elementController)
        {
            RemoveAllVisualLinks(elementController);
            var soughtChildren = AtomViewList.Where(a => ((ElementViewModel) a.DataContext).Id == elementController.Model.Id);
            if (soughtChildren.Any())
            {
                AtomViewList.Remove( soughtChildren.First());
            }
        }
    }
}