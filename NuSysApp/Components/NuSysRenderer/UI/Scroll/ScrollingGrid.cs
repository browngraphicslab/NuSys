using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ScrollingGrid<T> : ScrollingCanvas
    {
        /// <summary>
        /// The maximum number of columns. If scroll direction is horizontal then this property is ignored
        /// </summary>
        public int MaxColumns { get; set; }

        /// <summary>
        /// The maximum number of rows. If scroll direction is Vertical then this property is ignored
        /// </summary>
        public int MaxRows { get; set; }

        /// <summary>
        /// the minimum pixel width of any column
        /// </summary>
        public float MinColumnWidth { get; set; }

        /// <summary>
        /// the minimum pixel height of any row
        /// </summary>
        public float MinRowHeight { get; set; }

        /// <summary>
        /// The horizontal alignment of all the items within their cells
        /// </summary>
        public HorizontalAlignment ItemHorizontalAlignment { get; set; }

        /// <summary>
        /// the vertical alignment of all the items within their cells
        /// </summary>
        public VerticalAlignment ItemVerticalAlignment { get; set; }

        /// <summary>
        /// The relative width of the items relative to the size of a grid cell, must be a float between 0 and 1
        /// </summary>
        public float ItemRelativeWidth { get; set; }

        /// <summary>
        /// The relative height of the items relative to the size of a grid cell, must be a float between 0 and 1
        /// </summary>
        public float ItemRelativeHeight { get; set; }

        /// <summary>
        /// This function takes in a generic item, and returns the base interactive ui element to display on the 
        /// grid
        /// </summary>
        public virtual Func<T, BaseInteractiveUIElement> ItemFunction { get; set; }

        /// <summary>
        /// mapping of items the user adds to the scrolling grid, to BaseInteractiveUIElements
        /// that will actually be displayed on the grid
        /// </summary>
        private Dictionary<T, BaseInteractiveUIElement> _itemsToDisplayElements;

        /// <summary>
        /// list of the elements that are going to be displayed on the grid
        /// </summary>
        private List<BaseInteractiveUIElement> _displayElements;

        /// <summary>
        /// Dictionary mapping elements to their locations on the grid
        /// </summary>
        private Dictionary<BaseInteractiveUIElement, GridLocation> _elementsToLocations;

        /// <summary>
        /// the number of columns currently being displayed in the grid
        /// </summary>
        private int _numColumns;

        /// <summary>
        /// the number of rows currently being displayed in the grid
        /// </summary>
        private int _numRows;

        /// <summary>
        /// The width of the columns
        /// </summary>
        private float _columnWidth;

        /// <summary>
        /// The height of the rows
        /// </summary>
        private float _rowHeight;

        /// <summary>
        /// The width of elements
        /// </summary>
        private float _elementWidth;

        /// <summary>
        /// the height of elements
        /// </summary>
        private float _elementHeight;

        /// <summary>
        /// The scroll direction can be horizontal or vertical
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="scrollDirection">The scroll direction must be horizontal or vertical and not both</param>
        public ScrollingGrid(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ScrollOrientation scrollDirection = ScrollOrientation.Vertical) : base(parent, resourceCreator, scrollDirection)
        {
            if (scrollDirection != ScrollOrientation.Horizontal && scrollDirection != ScrollOrientation.Vertical)
            {
                Debug.Fail("The scroll direction must be horizontal or vertical for the dynamic layout to work");
            }

            _itemsToDisplayElements = new Dictionary<T, BaseInteractiveUIElement>();
            _displayElements = new List<BaseInteractiveUIElement>();
            _elementsToLocations = new Dictionary<BaseInteractiveUIElement, GridLocation>();
        }

        /// <summary>
        /// Adds an item to the end of the grid
        /// </summary>
        /// <param name="item"></param>
        public void AddItem(T item)
        {
            Debug.Assert(ItemFunction != null);
            var newElement = ItemFunction(item);
            _itemsToDisplayElements.Add(item, newElement);
            _displayElements.Add(newElement);
            CreateElementGridLocation(newElement);
            AddElement(newElement, new Vector2());
            SetElementLocation(newElement);
        }

        /// <summary>
        /// removes an item from the grid
        /// </summary>
        /// <param name="item"></param>
        public void RemoveItem(T item)
        {
            if (_itemsToDisplayElements.ContainsKey(item))
            {
                var displayElement = _itemsToDisplayElements[item];
                _itemsToDisplayElements.Remove(item);
                if (_displayElements.Contains(displayElement))
                {
                    _displayElements.Remove(displayElement);
                    if (_elementsToLocations.ContainsKey(displayElement))
                    {
                        _elementsToLocations.Remove(displayElement);
                    }
                }
            }
        }

        /// <summary>
        /// Creates the location of an element based on its index in the grid
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private GridLocation CreateElementGridLocation(int index)
        {
            int columnIndex;
            int rowIndex;
            switch (ScrollDirection)
            {
                case ScrollOrientation.Vertical:
                    columnIndex = index%_numColumns;
                    rowIndex = index/_numRows;
                    break;
                case ScrollOrientation.Horizontal:
                    rowIndex = index % _numRows;
                    columnIndex = index / _numColumns;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var newLocation = new GridLocation(rowIndex, columnIndex);
            _elementsToLocations.Add(_displayElements[index], newLocation);
            return newLocation;
        }

        /// <summary>
        /// Gets the location of an element
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private GridLocation CreateElementGridLocation(BaseInteractiveUIElement element)
        {
            var index = _displayElements.IndexOf(element);
            return CreateElementGridLocation(index);
        }

        /// <summary>
        /// Called whenever the size of the scrolling grid is changed
        /// recalculates the heights, widths, number of rows, and number of columns,
        /// new item sizes, and sets the new element locations
        /// </summary>
        protected override void OnSizeChanged()
        {
            CalculateRowsAndColumns();
            CalculateItemSize();
            SetElementSizes();
            SetElementLocations();

            base.OnSizeChanged();
        }

        /// <summary>
        /// Sets the sizes of all the elements on the grid
        /// </summary>
        private void SetElementSizes()
        {
            if (_displayElements == null)
            {
                return;
            }

            foreach (var element in _displayElements)
            {
                element.Width = _elementWidth;
                element.Height = _elementHeight;
            }
        }

        /// <summary>
        /// Sets the locations of all the elements
        /// </summary>
        private void SetElementLocations()
        {
            if (_displayElements == null)
            {
                return;
            }

            foreach (var element in _displayElements)
            {
                SetElementLocation(element);
            }
        }

        /// <summary>
        /// recalculate the size of the items
        /// </summary>
        private void CalculateItemSize()
        {
            _elementWidth = ItemRelativeWidth*_columnWidth;
            _elementHeight = ItemRelativeHeight*_rowHeight;
        }

        /// <summary>
        /// Sets the location of an element properly
        /// </summary>
        /// <param name="element"></param>
        private void SetElementLocation(BaseInteractiveUIElement element)
        {
            var elementLoc = _elementsToLocations[element];

            // set the x position of the element properly
            switch (ItemHorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    element.Transform.LocalPosition = new Vector2(elementLoc.Column * _columnWidth, element.Transform.LocalY);
                    break;
                case HorizontalAlignment.Center:
                    element.Transform.LocalPosition = new Vector2(elementLoc.Column * _columnWidth + _columnWidth / 2 - element.Width / 2, element.Transform.LocalY);
                    break;
                case HorizontalAlignment.Right:
                    element.Transform.LocalPosition = new Vector2(elementLoc.Column * _columnWidth + _columnWidth - element.Width, element.Transform.LocalY);
                    break;
                case HorizontalAlignment.Stretch:
                    element.Transform.LocalPosition = new Vector2(elementLoc.Column * _columnWidth, element.Transform.LocalY);
                    element.Width = _columnWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // set the y position of the element properly
            switch (ItemVerticalAlignment)
            {
                case VerticalAlignment.Top:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementLoc.Row * _rowHeight);
                    break;
                case VerticalAlignment.Bottom:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementLoc.Row * _rowHeight + _rowHeight - element.Height);
                    break;
                case VerticalAlignment.Center:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementLoc.Row * _rowHeight + _rowHeight / 2 - element.Height / 2);
                    break;
                case VerticalAlignment.Stretch:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementLoc.Row * _rowHeight);
                    element.Height = _rowHeight;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Calculates the dimensions of the rows and columns of the grid
        /// </summary>
        private void CalculateRowsAndColumns()
        {

            switch (ScrollDirection)
            {
                case ScrollOrientation.Vertical:
                    _numColumns = (int) (Width/Math.Max(MinColumnWidth, Width/MaxColumns));
                    _columnWidth = Width/_numColumns;
                    _rowHeight = MinRowHeight;
                    _numRows = ((_displayElements?.Count ?? 0) + _numColumns - 1)/_numColumns;
                    break;
                case ScrollOrientation.Horizontal:
                    _numRows = (int)(Height / Math.Max(MinRowHeight, Height / MaxRows));
                    _rowHeight = Height / _numRows;
                    _columnWidth = MinColumnWidth;
                    _numColumns = ((_displayElements?.Count ?? 0) + _numRows - 1) / _numRows;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Represents a location in a grid
        /// </summary>
        private class GridLocation
        {
            public int Column { get; }

            public int Row { get; }

            public GridLocation(int row, int column)
            {
                Row = row;
                Column = column;
            }
        }
    }
}
