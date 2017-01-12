using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    /// <summary>
    /// This is the page for links which appears for all types of elements in the detail view
    /// </summary>
    public class DetailViewLinksPage : RectangleUIElement
    {

        #region ui-items
        // the following items are listed in order of their vertical appearance in the ui

        /// <summary>
        /// Textbox used to input the title of the link to be added, found at the top of the detail view links page
        /// </summary>
        private ScrollableTextboxUIElement _addLinkTitleBox;

        /// <summary>
        /// Textbox used to input the title of the element the new link is to be linked to.
        /// </summary>
        private AutoSuggestTextBox<LibraryElementModel> _addLinkToElementBox;

        /// <summary>
        /// Textbox used to input tags for the new link
        /// </summary>
        private ScrollableTextboxUIElement _addLinkTagsBox;

        /// <summary>
        /// button pressed to indicate that the new link should be created
        /// </summary>
        private ButtonUIElement _createLinkButton;

        /// <summary>
        /// the list view containing all the links attached to this library element
        /// </summary>
        private ListViewUIElementContainer<LinkLibraryElementController> _link_listview;
        #endregion ui-items

        /// <summary>
        /// the library element controller for the item currently being displayed on this page of the detail view
        /// </summary>
        private LibraryElementController _controller;

        public DetailViewLinksPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _addLinkTitleBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                PlaceHolderText = "link title...",
                Background = Colors.Azure,
                BorderWidth = 2,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_addLinkTitleBox);

            _addLinkToElementBox = new AutoSuggestTextBox<LibraryElementModel>(this, Canvas)
            {
                PlaceHolderText = "link to...",
                Background = Colors.Azure,
                BorderWidth = 2,
                Bordercolor = Colors.DarkSlateGray,
                ColumnFunction = elementController => elementController.Title,
                FilterFunction = delegate(string s)
                {
                    return
                        new List<LibraryElementModel>(
                            SessionController.Instance.ContentController.AllLibraryElementModels.Where(
                                lem => lem.Title.Contains(s)));
                },
            };

            _addLinkTagsBox = new ScrollableTextboxUIElement(this, Canvas, false, false)
            {
                Background = Colors.Azure,
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray,
                PlaceHolderText = "tags - spearate with commas"
            };
            AddChild(_addLinkTagsBox);

            _createLinkButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas))
            {
                ButtonText = "Create Link",
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray,
                Background = Colors.Azure,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center
            };
            AddChild(_createLinkButton);

            // always add this as the last child since it has a drop down
            AddChild(_addLinkToElementBox);

        }

        /// <summary>
        /// Called when the create link button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void OnCreateLinkButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // if the auto suggest button has a selection chosen, and there isn't an empty title then create a link
            if (_addLinkToElementBox.HasSelection && !string.IsNullOrEmpty(_addLinkTitleBox.Text))
            {

                // get the tags from the tags box
                var tag_strings = _addLinkTagsBox.Text.Split(new string[] {", ", " ", " ,", ",", " , "},
                    StringSplitOptions.RemoveEmptyEntries);

                var keywords = new HashSet<Keyword>();
                foreach (var tagString in tag_strings)
                {
                    keywords.Add(new Keyword(tagString));
                }

                // try to add a link between the two controllers
                _controller.TryAddLinkTo(SessionController.Instance.ContentController.GetLibraryElementController(_addLinkToElementBox.CurrentSelection.LibraryElementId), _addLinkTitleBox.Text,keywords);
            }
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
            _link_listview.RemoveItems(new List<LinkLibraryElementController> { llecToBeRemoved} );
        }

        /// <summary>
        /// Fired whenever a link is added to the current controller, adds the link to the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLinkAdded(object sender, LinkLibraryElementController e)
        {
            _link_listview.AddItems(new List<LinkLibraryElementController> {e});
        }

        public override void Dispose()
        {
            _controller.LinkAdded -= OnLinkAdded;
            _controller.LinkRemoved -= OnLinkRemoved;
            _createLinkButton.Tapped -= OnCreateLinkButtonTapped;

            base.Dispose();
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

            _link_listview.AddColumns(new List<ListColumn<LinkLibraryElementController>> {listColumn, listColumn2});

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

            // helper variable, the current vertical spacing from the top of the window
            var vertical_spacing = 20;
            var horizontal_spacing = 20;

            // layout all the elments to add a link, (input textbox for title, and input textbox for linked to)
            var addLinkItemHeight = 50;

            var textboxWidth = (Width - 3 * horizontal_spacing) / 2;
            _addLinkTitleBox.Height = addLinkItemHeight;
            _addLinkTitleBox.Width = textboxWidth;
            _addLinkTitleBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _addLinkToElementBox.Height = addLinkItemHeight;
            _addLinkToElementBox.Width = textboxWidth;
            _addLinkToElementBox.Transform.LocalPosition = new Vector2(2 * horizontal_spacing + _addLinkTitleBox.Width, vertical_spacing);

            // layout all the elements for tags
            var tagHeight = 30;
            vertical_spacing += 20 + addLinkItemHeight; // increment the vertical spacing so tags is below add link textboxes

            _addLinkTagsBox.Height = tagHeight;
            _addLinkTagsBox.Width = Width - 2 * horizontal_spacing;
            _addLinkTagsBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            // layout the create link button
            vertical_spacing += 20 + tagHeight;
            var createLinkButtonHeight = 40;

            _createLinkButton.Width = 150;
            _createLinkButton.Height = createLinkButtonHeight;
            _createLinkButton.Transform.LocalPosition = new Vector2(Width/2 -  _createLinkButton.Width/2, vertical_spacing);

            //layout all the elements for the list view
            vertical_spacing += 20 + createLinkButtonHeight;
            _link_listview.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _link_listview.Width = Width - 2 * horizontal_spacing;
            _link_listview.Height = Height - 20 - vertical_spacing;

            base.Update(parentLocalToScreenTransform);
        }
    }
}
