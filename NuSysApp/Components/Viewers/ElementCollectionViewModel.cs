using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
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
            Text = controller.LibraryElementModel?.Data;

            Color = new SolidColorBrush(Windows.UI.Color.FromArgb(175, 156, 227, 143));
            AtomViewList = new ObservableCollection<FrameworkElement>();
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
            AddVisualLinks(controller);
            var view = await _nodeViewFactory.CreateFromSendable(controller);
            AtomViewList.Add(view);
            controller.Deleted += OnChildDeleted;
        }

        private void AddVisualLinks(ElementController controller)
        {
            var contentLinks = SessionController.Instance.LinkController.GetLinkedIds(controller.LibraryElementModel.LibraryElementId);
            var toLinkIds = new HashSet<string>();
            foreach (var linkId in contentLinks)
            {
                var link = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                toLinkIds.Add(link.InAtomId == controller.LibraryElementModel.LibraryElementId ? link.OutAtomId : link.InAtomId);
            }
            foreach ( var atom in AtomViewList.Select(e => e.DataContext as ElementViewModel))
            {
                if (toLinkIds.Contains(atom.ContentId))
                {
                    var lm = new LinkModel(SessionController.Instance.GenerateId());
                    lm.InAtomId = controller.Model.Id;
                    lm.OutAtomId = atom.Controller.Model.Id;
                    var lc = new LinkElementController(lm);
                    var view = new BezierLinkView(new LinkViewModel(lc));
                    AtomViewList.Add(view);
                }
            }
        }
        private void RemoveVisualLinks(ElementController controller)
        {
            var contentLinks = SessionController.Instance.LinkController.GetLinkedIds(controller.LibraryElementModel.LibraryElementId);
            var toLinkIds = new HashSet<string>();
            foreach (var linkId in contentLinks)
            {
                var link = SessionController.Instance.ContentController.GetContent(linkId) as LinkLibraryElementModel;
                toLinkIds.Add(link.InAtomId == controller.LibraryElementModel.LibraryElementId ? link.OutAtomId : link.InAtomId);
            }
            foreach (var atom in AtomViewList)
            {
                if (toLinkIds.Contains((atom.DataContext as ElementViewModel).ContentId))
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