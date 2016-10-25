using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
    class DetailViewLinksPage : RectangleUIElement
    {
        private StackLayoutManager _layoutManager;
        private StackLayoutManager _searchBarLayout;

        private RectangleUIElement _linkList;
        private RectangleUIElement _linkTitleSearchBar;
        private RectangleUIElement _linkToSearchBar;
        private RectangleUIElement _tagsSearchBar;
        private ButtonUIElement _createLinkButton;

        public DetailViewLinksPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) 
            : base(parent, resourceCreator)
        {
            // Is this the right content id?
            HashSet<string> links = SessionController.Instance.LinksController.GetLinkedIds(controller.LibraryElementModel.LibraryElementId);

            //extract the link information from the link id to put in the list view


            _linkList = new RectangleUIElement(parent, resourceCreator);
            _linkTitleSearchBar = new RectangleUIElement(parent, resourceCreator);
            _linkToSearchBar = new RectangleUIElement(parent, resourceCreator);
            _tagsSearchBar = new RectangleUIElement(parent, resourceCreator);
            _createLinkButton = new ButtonUIElement(parent, resourceCreator, new RoundedRectangleUIElement(parent, resourceCreator));

            _linkList.Background = Colors.OliveDrab;
            _linkTitleSearchBar.Background = Colors.Aquamarine;
            _linkToSearchBar.Background = Colors.Bisque;
            _tagsSearchBar.Background = Colors.BlueViolet;
            _createLinkButton.Background = Colors.DarkCyan;

            _layoutManager = new StackLayoutManager(StackAlignment.Vertical);
            _searchBarLayout = new StackLayoutManager(StackAlignment.Vertical);

            AddChild(_linkList);
            AddChild(_linkTitleSearchBar);
            AddChild(_linkToSearchBar);
            AddChild(_tagsSearchBar);
            AddChild(_createLinkButton);

            _layoutManager.AddElement(_linkList);

            _searchBarLayout.AddElement(_linkTitleSearchBar);
            _searchBarLayout.AddElement(_linkToSearchBar);
            _searchBarLayout.AddElement(_tagsSearchBar);
            _searchBarLayout.AddElement(_createLinkButton);

        }

        /// <summary>
        /// The update method, manage the layout here, update the transform here, called before draw
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _layoutManager.SetSize(Width, 2*(Height/3));
            _layoutManager.VerticalAlignment = VerticalAlignment.Center;
            _layoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _layoutManager.ItemWidth = Width - 20;
            _layoutManager.ItemHeight = 2*(Height/3);
            _layoutManager.TopMargin = Height/3;
            _layoutManager.ArrangeItems();

            _searchBarLayout.SetSize(Width, Height/3);
            _searchBarLayout.VerticalAlignment = VerticalAlignment.Center;
            _searchBarLayout.HorizontalAlignment = HorizontalAlignment.Center;
            _searchBarLayout.ItemWidth = Width - 20;
            _searchBarLayout.ItemHeight = (Height/3)/4;
            //_searchBarLayout.Spacing = 5;
            _searchBarLayout.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }
    }
}
