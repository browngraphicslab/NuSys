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
                if ((atom.DataContext as ElementViewModel).Controller == controller) continue;
                if ((atom.DataContext as ElementViewModel).ContentId ==
                    link.LinkLibraryElementModel.InAtomId.LibraryElementId ||
                    (atom.DataContext as ElementViewModel).ContentId ==
                    link.LinkLibraryElementModel.OutAtomId.LibraryElementId)
                {
                    var lm = new LinkModel(SessionController.Instance.GenerateId());
                    lm.InAtomId = controller.Model.Id;
                    lm.OutAtomId = (atom.DataContext as ElementViewModel).Controller.Model.Id;
                    lm.ContentId = id;
                    var lc = new LinkElementController(lm);
                    var view = new BezierLinkView(new LinkViewModel(lc));
                    AtomViewList.Add(view);

                }
            }
            /*   var contentLinks = SessionController.Instance.LinkController.GetLinkedIds(new LinkId(controller.LibraryElementModel.LibraryElementId));
            var toLinkIds = new HashSet<LinkId>();
            foreach (var linkId in contentLinks)
            {
                var link = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                if (link != null)
                {
                    toLinkIds.Add(link.InAtomId.LibraryElementId == controller.LibraryElementModel.LibraryElementId ? link.OutAtomId : link.InAtomId);
                }
            }
            var toAddAtoms = new HashSet<FrameworkElement>();
            foreach ( var atom in AtomViewList.Where(r => !(r.DataContext is LinkViewModel) && r.DataContext != null).Select(e => e.DataContext as ElementViewModel))
            {
                if (atom == null)
                {
                    continue;
                }
                if (toLinkIds.Select(Id=>Id.LibraryElementId).Contains(atom.ContentId))

                {
                    var isAlreadyMade = false;
                    foreach (var link in AtomViewList.Where(r => r.DataContext is LinkViewModel).Select(e => e.DataContext as LinkViewModel))
                    {
                        if ((link.Controller.Model as LinkModel).InAtomId == controller.Model.Id &&
                            (link.Controller.Model as LinkModel).OutAtomId == atom.Model.Id ||
                            (link.Controller.Model as LinkModel).OutAtomId == controller.Model.Id &&
                            (link.Controller.Model as LinkModel).InAtomId == atom.Model.Id)
                        {
                            isAlreadyMade = true;
                        }
                    }
                    if (isAlreadyMade) continue;
                    var lm = new LinkModel(SessionController.Instance.GenerateId());
                    lm.InAtomId = controller.Model.Id;
                    lm.OutAtomId = atom.Controller.Model.Id;
                    lm.ContentId = ;
                    var lc = new LinkElementController(lm);
                    var view = new BezierLinkView(new LinkViewModel(lc));
                    toAddAtoms.Add(view);
                }
            }
            foreach (var toAdd in toAddAtoms)
            {
                AtomViewList.Add(toAdd);
            }*/
        }
        private void RemoveVisualLinks(ElementController controller)
        {
            foreach (var atom in new HashSet<FrameworkElement>(AtomViewList))
            {
                Debug.Assert(atom.DataContext is ElementViewModel);
                var vm = (atom.DataContext as ElementViewModel);
                if (vm?.Controller == controller)
                {
                    AtomViewList.Remove(atom);
                }
                else if(vm?.ElementType == ElementType.Link)
                {
                    Debug.Assert(vm is LinkViewModel);
                    var linkVm = vm as LinkViewModel;
                    var linkModel = linkVm.LinkModel;
                    if (linkModel.InAtomId == controller.Model.Id || linkModel.OutAtomId == controller.Model.Id)
                    {
                        AtomViewList.Remove(atom);
                    }
                }
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
            RemoveVisualLinks(elementController);
            var soughtChildren = AtomViewList.Where(a => ((ElementViewModel) a.DataContext).Id == elementController.Model.Id);
            if (soughtChildren.Any())
            {
                AtomViewList.Remove( soughtChildren.First());
            }
        }
    }
}