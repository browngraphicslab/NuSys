using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    public class ReadOnlyLinksWindow : ReadOnlyModeWindow
    {
        /// <summary>
        /// the list view containing all the links attached to this library element
        /// </summary>
        private ListViewUIElementContainer<LinkLibraryElementController> _link_listview;

        private TextboxUIElement _label;

        /// <summary>
        /// the library element controller for the item currently being displayed on this window.
        /// </summary>
        private LibraryElementController _controller;

        public ReadOnlyLinksWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _label = new TextboxUIElement(this, ResourceCreator);
            _label.Text = "links";
            _label.Width = Width;
            _label.Height = 38;
            _label.FontSize = 32;
            _label.TextColor = Constants.DARK_BLUE;
            _label.Background = Constants.LIGHT_BLUE;
            _label.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _label.IsHitTestVisible = false;

            AddChild(_label);
        }

        public override Task Load()
        {
            CreateListView();

            return base.Load();
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
                BorderColor = Constants.DARK_BLUE
            };
            AddChild(_link_listview);
            _link_listview.Load();

            var listColumn = new ListTextColumn<LinkLibraryElementController>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = llec => llec.Title;

            var listColumn2 = new ListTextColumn<LinkLibraryElementController>();
            listColumn2.Title = "Linked To";
            listColumn2.RelativeWidth = 1;
            listColumn2.ColumnFunction = getOppositeLinkedToTitle;

            _link_listview.AddColumns(new List<ListColumn<LinkLibraryElementController>> { listColumn, listColumn2 });
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
            if (_link_listview == null || _controller == null)
            {
                return;
            }

            var horizontalMargin = 10;
            var verticalMargin = 5;

            _label.Width = Width;
            _label.Transform.LocalPosition = new Vector2( 0, verticalMargin);

            //layout all the elements for the list view
            _link_listview.Transform.LocalPosition = new Vector2( horizontalMargin, _label.Height + verticalMargin);
            _link_listview.Width = Width -  ( 2 * horizontalMargin);
            _link_listview.Height = Height - (_label.Height + verticalMargin*2);

            if (_controller.LibraryElementModel.Type == NusysConstants.ElementType.Collection)
            {
                IsVisible = false;
            }

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
            _link_listview?.ClearItems();
            _link_listview?.ClearFilter();
            _link_listview?.AddItems(new List<LinkLibraryElementController>(_controller.GetAllLinks()));
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
            _link_listview?.RemoveItems(new List<LinkLibraryElementController> { llecToBeRemoved });
        }

        /// <summary>
        /// Fired whenever a link is added to the current controller, adds the link to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLinkAdded(object sender, LinkLibraryElementController e)
        {
            _link_listview?.AddItems(new List<LinkLibraryElementController> { e });
        }

        public override void Dispose()
        {
            _controller.LinkAdded -= OnLinkAdded;
            _controller.LinkRemoved -= OnLinkRemoved;
            base.Dispose();
        }
    }
}
