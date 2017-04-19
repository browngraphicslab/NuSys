using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;
using System.Numerics;

namespace NuSysApp
{
    /// <summary>
    /// This is the class for displaying pages of data as a list
    /// </summary>
    /// <typeparam name="T">
    /// The type of data to be associated with the values in the list.
    /// </typeparam>
    class PaginatedListView<T> : RectangleUIElement
    {
        /// <summary>
        /// The height of the search bar
        /// </summary>
        private const float SEARCHBAR_HEIGHT = 26f;

        private const float SEARCH_BUTTON_WIDTH = 20f;


        /// <summary>
        /// The list ui element to display the data
        /// </summary>
        private ListViewUIElementContainer<T> _list;

        /// <summary>
        /// The source element that is used to provide the data for the next page and the previous page
        /// </summary>
        private NextPageable<T> _source;

        /// <summary>
        /// The search bar used to make searches that will populate the list.
        /// </summary>
        private ScrollableTextboxUIElement _searchBar;

        private ButtonUIElement _searchButton;

        private ButtonUIElement _nextButton;

        private ButtonUIElement _prevButton;




        /// <summary>
        /// Creates a new Paginated list view
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="source">
        ///  The source element that is used to provide the data for the next page and the previous page
        ///  </param>
        public PaginatedListView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, NextPageable<T> source) : base(parent, resourceCreator)
        {
            if(source == null)
            {
                throw new ArgumentNullException("Cannot have a null source when creating a paginated list view");
            }
            _list = new ListViewUIElementContainer<T>(parent, resourceCreator);
            AddChild(_list);
            SetUpSearchBarAndButton();
            SetUpNextPrevButtons();
            _source = source;

        }

        private void SetUpNextPrevButtons()
        {
            _nextButton = new ButtonUIElement(this, Canvas)
            {
                Height = 20,
                ButtonText = "Next",
                Width = 20,
                Background = Colors.Purple
            };
            _nextButton.Tapped += _nextButton_Tapped;
            AddChild(_nextButton);
            _prevButton = new ButtonUIElement(this, Canvas)
            {
                Height = 20,
                ButtonText = "Next",
                Width = 20,
                Background = Colors.Orange
            };
            AddChild(_prevButton);
        }

        private void _nextButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SearchAndPopulate();
        }

        private void SetUpSearchBarAndButton()
        {
            // initialize the search bar
            _searchBar = new ScrollableTextboxUIElement(this, Canvas, false, true)
            {
                Height = SEARCHBAR_HEIGHT,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                TextVerticalAlignment = CanvasVerticalAlignment.Bottom,
                FontSize = 14,
                BorderWidth = 1,
                BorderColor = Constants.MED_BLUE,
                Background = Colors.White,
                FontFamily = UIDefaults.TextFont,
                PlaceHolderText = "Search Values ..."
            };
            AddChild(_searchBar);

            _searchButton = new ButtonUIElement(this, Canvas)
            {
                Height = SEARCHBAR_HEIGHT,
                ButtonText = "Search",
                Width = SEARCH_BUTTON_WIDTH,
                Background = Colors.Red
            };
            _searchButton.Tapped += _searchButton_Tapped; 
            AddChild(_searchButton);
        }

        private void _searchButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SearchAndPopulate();
        }

        private async void SearchAndPopulate()
        {
             _source.MakeSearchRequest(_searchBar.Text);
            PopulateNextPage();
        }

        private async void PopulateNextPage()
        {
            var src = await _source.getNextPage();
            _list.ClearItems();
            _list.AddItems(src);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);

            _list.Width = Width;
            _list.Height = Height - SEARCHBAR_HEIGHT;
            _list.Transform.LocalX = 0;
            _list.Transform.LocalY = SEARCHBAR_HEIGHT;

            _searchBar.Width = Width;
            _searchBar.Height = SEARCHBAR_HEIGHT;
            _searchBar.Transform.LocalY = 0;

            _searchButton.Transform.LocalX = Width - SEARCH_BUTTON_WIDTH;

            _prevButton.Transform.LocalY = Height / 2;
            _prevButton.Transform.LocalX = -SEARCH_BUTTON_WIDTH;

            _nextButton.Transform.LocalX = Width;
            _nextButton.Transform.LocalY = Height / 2;
        }

        /// <summary>
        /// This adds all the columns to _listColumns. If you are adding multiple columns use this instead of the AddColumn method
        /// so that the list only reloads once.
        /// </summary>
        /// <param name="listColumns"></param>
        public void AddColumns(IEnumerable<ListColumn<T>> listColumns)
        {
            _list.AddColumns(listColumns);
        }

        /// <summary>
        /// This should add the listColumn to the _listColumns.
        /// This should also update all the listviewRowUIElements appropriately by repopulating the row with cells with the proper widths.
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(ListColumn<T> listColumn)
        {
            _list.AddColumn(listColumn);
        }

    }
}
