using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ReadOnlyLinksWindow : ReadOnlyModeWindow
    {
        /// <summary>
        /// the list view containing all the links attached to this library element
        /// </summary>
        private ListViewUIElementContainer<LinkLibraryElementController> _link_listview;

        /// <summary>
        /// the library element controller for the item currently being displayed on this page of the detail view
        /// </summary>
        private LibraryElementController _controller;

        public ReadOnlyLinksWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }

        /// <summary>
        /// Create the list view to display all the links
        /// </summary>
        private void CreateListView()
        {
            _link_listview = new ListViewUIElementContainer<LinkLibraryElementController>(this, ResourceCreator)
            {
                Background = Colors.White,
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_link_listview);

            var listColumn = new ListTextColumn<LinkLibraryElementController>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = llec => llec.Title;

            var listColumn2 = new ListTextColumn<LinkLibraryElementController>();
            listColumn2.Title = "Linked To";
            listColumn2.RelativeWidth = 1;
            listColumn2.ColumnFunction = getOppositeLinkedToTitle;

            _link_listview.AddColumns(new List<ListColumn<LinkLibraryElementController>> { listColumn, listColumn2 });

            _link_listview.AddItems(new List<LinkLibraryElementController>(_controller.GetAllLinks()));
        }


        /// <summary>
        /// Returns the title of the opposite element the passed in link library element controller is attached to
        /// </summary>
        /// <param name="llec"></param>
        /// <returns></returns>
        private string getOppositeLinkedToTitle(LinkLibraryElementController llec)
        {
            // the current library element id for the element this detail view page represents
            var thisLibElemId = _controller.LibraryElementModel.LibraryElementId;

            // if the current is the same as the inAtomId return the title of the OutAtomId
            if (thisLibElemId == llec.LinkLibraryElementModel.InAtomId)
            {
                return SessionController.Instance.ContentController.GetLibraryElementModel(
                    llec.LinkLibraryElementModel.OutAtomId).Title;
            }

            // otherwise return the title of the InAtomId
            return SessionController.Instance.ContentController.GetLibraryElementModel(
                llec.LinkLibraryElementModel.InAtomId).Title;
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_link_listview == null)
            {
                return;
            }
            // helper variable, the current vertical spacing from the top of the window
            var vertical_spacing = 20;
            var horizontal_spacing = 20;
       
            //layout all the elements for the list view
            _link_listview.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _link_listview.Width = Width - 2 * horizontal_spacing;
            _link_listview.Height = Height - 20 - vertical_spacing;

            base.Update(parentLocalToScreenTransform);
        }

        public void UpdateList(LibraryElementController controller)
        {
            if (_controller != null)
            {
                _controller.LinkAdded -= OnLinkAdded;
                _controller.LinkRemoved -= OnLinkRemoved;
            }
            _controller = controller;
            CreateListView();
            _controller.LinkAdded += OnLinkAdded;
            _controller.LinkRemoved += OnLinkRemoved;
        }

        /// <summary>
        /// Fired whenever a link is removed from the current controller, removes the link from the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLinkRemoved(object sender, string e)
        {
            // get the link library element controller of the link that is going to be removed
            var llecToBeRemoved = SessionController.Instance.LinksController.GetLinkLibraryElementControllerFromLibraryElementId(e);
            // remove it from the list
            _link_listview.RemoveItems(new List<LinkLibraryElementController> { llecToBeRemoved });
        }

        /// <summary>
        /// Fired whenever a link is added to the current controller, adds the link to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLinkAdded(object sender, LinkLibraryElementController e)
        {
            _link_listview.AddItems(new List<LinkLibraryElementController> { e });
        }

        public override void Dispose()
        {
            _controller.LinkAdded -= OnLinkAdded;
            _controller.LinkRemoved -= OnLinkRemoved;
            base.Dispose();
        }
    }
}
