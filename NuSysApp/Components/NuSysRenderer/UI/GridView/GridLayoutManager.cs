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
    public class GridLayoutManager : RectangleUIElement
    {
        /// <summary>
        /// The width of the grid, relative widths are calculated based off of this value
        /// </summary>
        public override float Width
        {
            get { return base.Width; }
            set
            {
                base.Width = value;
                RecalculateSize();
            }
        }

        /// <summary>
        /// The height of the grid, relative heights are calculated based off of this value
        /// </summary>
        public override float Height
        {
            get { return base.Height; }
            set
            {
                base.Height = value;
                RecalculateSize();
            }
        }

        /// <summary>
        /// The width of the border of the grid, columns and rows appear within the border
        /// </summary>
        public override float BorderWidth
        {
            get { return base.BorderWidth; }
            set
            {
                base.BorderWidth = value;
                RecalculateSize();
            }
        }

        /// <summary>
        /// The minimum width of the GridView, this is the sum of the minimum widths assigned to each of the columns
        /// </summary>
        public float MinWidth { get; private set; }

        /// <summary>
        /// The minimum height of the GridView, this is the sum of the minimum heights assigned to each of the rows
        /// </summary>
        public float MinHeight { get; private set; }

        /// <summary>
        /// The columns currently in the grid
        /// </summary>
        private List<GridLayoutManagerColumn> Columns { get; }

        /// <summary>
        /// The rows currently in the Grid
        /// </summary>
        public List<GridLayoutManagerRow> Rows { get; }

        /// <summary>
        /// The elements that are currently displayed on the grid
        /// </summary>
        public List<BaseInteractiveUIElement> Elements { get; }

        /// <summary>
        /// Dictionary mapping elements to their locations on the grid
        /// </summary>
        private Dictionary<BaseInteractiveUIElement, GridLocation> _elementsToLocations;

        /// <summary>
        /// Dictionary mapping a row to the elements it contains
        /// </summary>
        private Dictionary<GridLayoutManagerRow, List<BaseInteractiveUIElement>> _rowToElements;

        /// <summary>
        /// Dictionary mapping a column to the elements it contains
        /// </summary>
        private Dictionary<GridLayoutManagerColumn, List<BaseInteractiveUIElement>> _colToElements;

        public GridLayoutManager(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Columns = new List<GridLayoutManagerColumn>();
            Rows = new List<GridLayoutManagerRow>();
            Elements = new List<BaseInteractiveUIElement>();
            _elementsToLocations = new Dictionary<BaseInteractiveUIElement, GridLocation>();
            _rowToElements = new Dictionary<GridLayoutManagerRow, List<BaseInteractiveUIElement>>();
            _colToElements = new Dictionary<GridLayoutManagerColumn, List<BaseInteractiveUIElement>>();
        }

        /// <summary>
        /// Adds a row to the GridView
        /// </summary>
        /// <param name="row"></param>
        public void AddRow(GridLayoutManagerRow row)
        {
            row.MinHeightChanged += RecalculateSize;
            row.RelativeHeightChanged += RecalculateSize;
            _rowToElements[row] = new List<BaseInteractiveUIElement>();
            Rows.Add(row);
            RecalculateSize();
        }

        /// <summary>
        /// Removes a row from the GridView, and all the elements that are in that row
        /// </summary>
        /// <param name="row"></param>
        public void RemoveRow(GridLayoutManagerRow row)
        {
            if (!Rows.Contains(row))
            {
                return;
            }

            // remove the events from the row
            row.MinHeightChanged -= RecalculateSize;
            row.RelativeHeightChanged -= RecalculateSize;

            // remove all the elements from the row
            foreach (var rowElement in _rowToElements[row])
            {
                RemoveElement(rowElement);
            }

            Rows.Remove(row);
            RecalculateSize();
        }

        /// <summary>
        /// Adds a column to the grid view
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(GridLayoutManagerColumn column)
        {
            column.MinWidthChanged += RecalculateSize;
            column.RelativeWidthChanged += RecalculateSize;
            _colToElements[column] = new List<BaseInteractiveUIElement>(); 
            Columns.Add(column);
            RecalculateSize();
        }

        /// <summary>
        /// Removes a column from the grid view
        /// </summary>
        /// <param name="column"></param>
        public void RemoveColumn(GridLayoutManagerColumn column)
        {
            if (!Columns.Contains(column))
            {
                return;
            }

            // remove the column events
            column.MinWidthChanged -= RecalculateSize;
            column.RelativeWidthChanged -= RecalculateSize;

            // remove all the elements from the column
            foreach (var colElement in _colToElements[column])
            {
                RemoveElement(colElement);
            }

            Columns.Remove(column);
            RecalculateSize();
        }

        /// <summary>
        /// Called every time the size of the grid and its columns and rows needs to be recalculated
        /// </summary>
        private void RecalculateSize()
        {
            // return if columns or rows is null, this is because the call can be made before the item is constructed
            // when the default Width and Height are set, this method is called
            if (Columns == null || Rows == null)
            {
                return;
            }

            // assign the proper widths to each column
            var totalRelativeWidth = Columns.Sum(col => col.RelativeWidth);
            var widthPerRelativeUnit = Width/totalRelativeWidth;
            var xOffset = BorderWidth;
            foreach (var column in Columns)
            {
                column.Width = widthPerRelativeUnit*column.RelativeWidth;
                column.Left = xOffset;
                xOffset += column.Width;
            }

            // assign the proper heights to each row
            var totalRelativeHeight = Rows.Sum(row => row.RelativeHeight);
            var heightPerRelativeUnit = Height / totalRelativeHeight;
            var yOffset = BorderWidth;
            foreach (var row in Rows)
            {
                row.Height = heightPerRelativeUnit * row.RelativeHeight;
                row.Top = yOffset;
                yOffset += row.Height;
            }

            // calculate the minWidth
            MinWidth = Columns.Sum(col => col.MinWidth);

            // calculate the minHeight
            MinHeight = Rows.Sum(row => row.MinHeight);

            ArrangeElements();
        }

        /// <summary>
        /// Overload method for the row and column MinWidthChanged, MinHeightChanged, RelativeWidthChanged, RelativeHeightChanged
        /// events
        /// </summary>
        private void RecalculateSize(object sender, float e)
        {
            RecalculateSize();
        }

        /// <summary>
        /// Called whenever the size of the grid changes, sets the positions and sizes of all the elements in the grid
        /// </summary>
        private void ArrangeElements()
        {
            foreach (var element in Elements)
            {
                SetElementPosition(element);
            }
        }

        /// <summary>
        /// Adds an element to the given row and column. Rows and columns are zero indexed
        /// </summary>
        /// <param name="element"></param>
        /// <param name="column"></param>
        /// <param name="row"></param>
        public void AddElement(BaseInteractiveUIElement element, int row, int column, HorizontalAlignment horizontalAlignment = HorizontalAlignment.Center, VerticalAlignment verticalAlignment = VerticalAlignment.Center)
        {
            if (row < 0 || row > Rows.Count - 1)
            {
                Debug.Fail($"Invalid row ({row}). Row must be an integer between 0 and {Rows.Count - 1}. If this looks impossible, you probably don't have any Rows in the grid.");
            }
            if (column < 0 || column > Columns.Count - 1)
            {
                Debug.Fail($"Invalid column ({column}). column must be an integer between 0 and {Columns.Count - 1}. If this looks impossible, you probably don't have any Columns in the grid.");
            }

            Elements.Add(element);
            _elementsToLocations[element] = new GridLocation(Rows[row], Columns[column], horizontalAlignment, verticalAlignment);
            AddChild(element);
            SetElementPosition(element);
        }

        /// <summary>
        /// Removes an element from the grid
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(BaseInteractiveUIElement element)
        {
            // if the elemetn is not in the grid then don't do anything
            if (!Elements.Contains(element))
            {
                return;
            }

            Elements.Remove(element);

            var elementCol = _elementsToLocations[element].Column;
            var elementRow = _elementsToLocations[element].Row;

            _elementsToLocations.Remove(element);
            _rowToElements[elementRow].Remove(element);
            _colToElements[elementCol].Remove(element);

            RemoveChild(element);
        }

        /// <summary>
        /// Sets the position of the element properly
        /// </summary>
        /// <param name="element"></param>
        private void SetElementPosition(BaseInteractiveUIElement element)
        {
            // make sure we have the location of the element
            if (!_elementsToLocations.ContainsKey(element))
            {
                Debug.Fail( "Invalid element, make sure the Element Removal and Add is being performed properly");
            }

            // get the location of the element
            var elementLoc = _elementsToLocations[element];

            // make sure we actually have the column this element should be in
            Debug.Assert(Columns.Contains(elementLoc.Column));
            var elementCol = elementLoc.Column;

            // make sure we actually have the row this element should be in
            Debug.Assert(Rows.Contains(elementLoc.Row));
            var elementRow = elementLoc.Row;

            // set the x position of the element properly
            switch (elementLoc.HorizontalAlignment)
            {
                case HorizontalAlignment.Left:
                    element.Transform.LocalPosition = new Vector2(elementCol.Left, element.Transform.LocalY);
                    break;
                case HorizontalAlignment.Center:
                    element.Transform.LocalPosition = new Vector2(elementCol.Left + elementCol.Width/2 - element.Width/2, element.Transform.LocalY);
                    break;
                case HorizontalAlignment.Right:
                    element.Transform.LocalPosition = new Vector2(elementCol.Right - element.Width, element.Transform.LocalY);
                    break;
                case HorizontalAlignment.Stretch:
                    element.Transform.LocalPosition = new Vector2(elementCol.Left, element.Transform.LocalY);
                    element.Width = elementCol.Width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // set the y position of the element properly
            switch (elementLoc.VerticalAlignment)
            {
                case VerticalAlignment.Top:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementRow.Top);
                    break;
                case VerticalAlignment.Bottom:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementRow.Bottom - element.Height);
                    break;
                case VerticalAlignment.Center:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementRow.Top + elementRow.Height/2 - element.Height/2);
                    break;
                case VerticalAlignment.Stretch:
                    element.Transform.LocalPosition = new Vector2(element.Transform.LocalX, elementRow.Top);
                    element.Height = elementRow.Height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override void Dispose()
        {
            foreach (var row in Rows)
            {
                row.MinHeightChanged -= RecalculateSize;
                row.RelativeHeightChanged -= RecalculateSize;
            }
            foreach (var column in Columns)
            {
                column.MinWidthChanged -= RecalculateSize;
                column.RelativeWidthChanged -= RecalculateSize;
            }
            base.Dispose();
        }

        /// <summary>
        /// Represents a location in a grid
        /// </summary>
        private class GridLocation
        {
            public HorizontalAlignment HorizontalAlignment;

            public VerticalAlignment VerticalAlignment;

            public GridLayoutManagerColumn Column { get; }

            public GridLayoutManagerRow Row { get; }

            public GridLocation(GridLayoutManagerRow row, GridLayoutManagerColumn column, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
            {
                Row = row;
                Column = column;
                HorizontalAlignment = horizontalAlignment;
                VerticalAlignment = verticalAlignment;
            }
        }
    }
}
