﻿using Microsoft.Graphics.Canvas;
using NuSysApp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using NuSysApp.Components.NuSysRenderer.UI.BaseUIElements;

namespace NuSysApp
{
    public class DetailViewLinksPage : RectangleUIElement
    {
        // All the layout managers to format the links page
        private StackLayoutManager _layoutManager;
        private StackLayoutManager _searchBarLayout;
        private StackLayoutManager _topSearchBars;

        // The rectangles that will be turned into textboxes and buttons later
        private RectangleUIElement _linkTitleSearchBar;
        private RectangleUIElement _linkToSearchBar;
        private RectangleUIElement _tagsSearchBar;
        private ButtonUIElement _createLinkButton;

        // List of link information
        private ListViewUIElementContainer<string> _listView;

        /// <summary>
        /// The controller for the links page
        /// </summary>
        private LibraryElementController _controller;

        public DetailViewLinksPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) 
            : base(parent, resourceCreator)
        {
            // set up the local controller variable
            _controller = controller;

            // Create all necessary rectangles
            _linkTitleSearchBar = new RectangleUIElement(parent, resourceCreator);
            _linkToSearchBar = new RectangleUIElement(parent, resourceCreator);
            _tagsSearchBar = new RectangleUIElement(parent, resourceCreator);
            _createLinkButton = new RoundedRectButtonUIElement(parent, resourceCreator);

            // Set Background colors
            _linkTitleSearchBar.Background = Colors.Aquamarine;
            _linkToSearchBar.Background = Colors.SlateBlue;
            _tagsSearchBar.Background = Colors.BlueViolet;
            _createLinkButton.Background = Colors.DarkCyan;

            _layoutManager = new StackLayoutManager(StackAlignment.Vertical);
            _searchBarLayout = new StackLayoutManager(StackAlignment.Vertical);
            _topSearchBars = new StackLayoutManager();

            // Add rectangles as children of the class
            AddChild(_linkTitleSearchBar);
            AddChild(_linkToSearchBar);
            AddChild(_tagsSearchBar);
            AddChild(_createLinkButton);

            // Add elements to layout managers
            _topSearchBars.AddElement(_linkTitleSearchBar);
            _topSearchBars.AddElement(_linkToSearchBar);
            _searchBarLayout.AddElement(_tagsSearchBar);
            _searchBarLayout.AddElement(_createLinkButton);

            SetUpList(parent, resourceCreator, controller);

            // make the list live updating
            _controller.LinkAdded += OnLinkAdded;
            _controller.LinkRemoved += OnLinkRemoved;
        }

        private void OnLinkRemoved(object sender, string e)
        {
            _listView.RemoveItems(new List<string> {e});
        }

        private void OnLinkAdded(object sender, LinkLibraryElementController libraryElementController)
        {
            _listView.AddItems(new List<string>{ libraryElementController.LibraryElementModel.LibraryElementId });
        }

        /// <summary>
        /// Set up the list holding the link information
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="controller"></param>
        private void SetUpList(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller)
        {
            // Returns the list of LinkLibraryElement ID's for links attached to the LibraryElementModel of the passed in id
            HashSet<string> linkLibraryElementIds = SessionController.Instance.LinksController.GetLinkedIds(controller.LibraryElementModel.LibraryElementId);

            _listView = new ListViewUIElementContainer<string>(parent, resourceCreator);
            _listView.ShowHeader = false;

            // Create the title column
            ListTextColumn<string> title = new ListTextColumn<string>();
            title.Title = "TITLE:";
            title.RelativeWidth = 1;
            title.ColumnFunction = delegate (string linkLibraryElementId)
            {
                var linkController = SessionController.Instance.LinksController.GetLinkLibraryElementControllerFromLibraryElementId(linkLibraryElementId);
                Debug.Assert(linkController != null);
                return linkController.LinkLibraryElementModel.Title;
            };

            // Create the column holding the linked to information
            ListTextColumn<string> linkedTo = new ListTextColumn<string>();
            linkedTo.Title = "LINKED TO:";
            linkedTo.RelativeWidth = 1;
            linkedTo.ColumnFunction = delegate (string linkLibraryElementId)
            {
                var linkController = SessionController.Instance.LinksController.GetLinkLibraryElementControllerFromLibraryElementId(linkLibraryElementId);
                Debug.Assert(linkController != null);
                var opposite = SessionController.Instance.LinksController.GetOppositeLibraryElementModel(controller.LibraryElementModel.LibraryElementId, linkController);
                Debug.Assert(opposite != null);
                return opposite.Title;
            };

            List<ListColumn<string>> cols = new List<ListColumn<string>>();
            cols.Add(title);
            cols.Add(linkedTo);
            _listView.AddColumns(cols);

            _listView.Transform.LocalPosition = new Vector2(0, 0);

            _listView.AddItems(linkLibraryElementIds.ToList<string>());


            // Add it as a child of the links page and to a layout manager to
            // format its size and location
            AddChild(_listView);
            _layoutManager.AddElement(_listView);
        }

        /// <summary>
        /// The update method, manage the layout here, update the transform here, called before draw
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // Update all layout managers to stay consistent with the container size
            _layoutManager.SetSize(Width, 2 * (Height / 3) - 5);
            _layoutManager.VerticalAlignment = VerticalAlignment.Center;
            _layoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _layoutManager.ItemWidth = Width - 20;
            _layoutManager.ItemHeight = 2 * (Height / 3) - 5;
            _layoutManager.ArrangeItems(new Vector2(0, Height / 3));

            _searchBarLayout.SetSize(Width, 2*((Height/3)/3));
            _searchBarLayout.VerticalAlignment = VerticalAlignment.Center;
            _searchBarLayout.HorizontalAlignment = HorizontalAlignment.Center;
            _searchBarLayout.ItemWidth = Width - 20;
            _searchBarLayout.ItemHeight = (Height/3)/3-5;
            _searchBarLayout.Spacing = 5;
            _searchBarLayout.ArrangeItems(new Vector2(0, (Height/3)/3));

            _topSearchBars.SetSize(Width, (Height / 3)/3);
            _topSearchBars.VerticalAlignment = VerticalAlignment.Top;
            _topSearchBars.HorizontalAlignment = HorizontalAlignment.Center;
            _topSearchBars.ItemWidth = (Width - 20)/2;
            _topSearchBars.ItemHeight = (Height / 3) / 3 - 5;
            _topSearchBars.Spacing = 5;
            _topSearchBars.ArrangeItems();

            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            _controller.LinkAdded -= OnLinkAdded;
            _controller.LinkRemoved -= OnLinkRemoved;
            base.Dispose();
        }
    }
}
