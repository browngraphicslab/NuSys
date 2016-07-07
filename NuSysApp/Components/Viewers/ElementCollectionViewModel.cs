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
                Debug.Assert(dc != null);
                if (dc.Controller == controller) continue;
                if (dc.ContentId ==
                    link.LinkLibraryElementModel.InAtomId.LibraryElementId ||
                    dc.ContentId ==
                    link.LinkLibraryElementModel.OutAtomId.LibraryElementId)
                {
                    var lm = new LinkModel(SessionController.Instance.GenerateId());
                    lm.InAtomId = controller.Model.Id;
                    lm.OutAtomId = dc.Controller.Model.Id;
                    lm.ContentId = id;
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
                toRemoveAtoms.Add(atom);
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
                if (((atom.DataContext as LinkViewModel).Model as LinkModel).ContentId == id)
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