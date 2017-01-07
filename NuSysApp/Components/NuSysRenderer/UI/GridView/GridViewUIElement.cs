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
    public class GridViewUIElement : RectangleUIElement
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
        private List<GridViewColumn> Columns { get; }

        /// <summary>
        /// The rows currently in the Grid
        /// </summary>
        public List<GridViewRow> Rows { get; }

        /// <summary>
        /// The elements that are currently displayed on the grid
        /// </summary>
        public List<BaseRenderItem> Elements { get; }

        /// <summary>
        /// Dictionary mapping elements to their locations on the grid
        /// </summary>
        private Dictionary<BaseRenderItem, GridLocation> _elementsToLocations;

        /// <summary>
        /// Dictionary mapping a row to the elements it contains
        /// </summary>
        private Dictionary<GridViewRow, List<BaseRenderItem>> _rowToElements;

        /// <summary>
        /// Dictionary mapping a column to the elements it contains
        /// </summary>
        private Dictionary<GridViewColumn, List<BaseRenderItem>> _colToElements;

        public GridViewUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Columns = new List<GridViewColumn>();
            Rows = new List<GridViewRow>();
            Elements = new List<BaseRenderItem>();
            _elementsToLocations = new Dictionary<BaseRenderItem, GridLocation>();
            _rowToElements = new Dictionary<GridViewRow, List<BaseRenderItem>>();
            _colToElements = new Dictionary<GridViewColumn, List<BaseRenderItem>>();
        }

        /// <summary>
        /// Adds a row to the GridView
        /// </summary>
        /// <param name="row"></param>
        public void AddRow(GridViewRow row)
        {
            row.MinHeightChanged += RecalculateSize;
            row.RelativeHeightChanged += RecalculateSize;
            _rowToElements[row] = new List<BaseRenderItem>();
            Rows.Add(row);
            RecalculateSize();
        }

        /// <summary>
        /// Removes a row from the GridView, and all the elements that are in that row
        /// </summary>
        /// <param name="row"></param>
        public void RemoveRow(GridViewRow row)
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
        public void AddColumn(GridViewColumn column)
        {
            column.MinWidthChanged += RecalculateSize;
            column.RelativeWidthChanged += RecalculateSize;
            _colToElements[column] = new List<BaseRenderItem>(); 
            Columns.Add(column);
            RecalculateSize();
        }

        /// <summary>
        /// Removes a column from the grid view
        /// </summary>
        /// <param name="column"></param>
        public void RemoveColumn(GridViewColumn column)
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
        public void AddElement(BaseRenderItem element, int row, int column)
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
            _elementsToLocations[element] = new GridLocation(Rows[row], Columns[column]);
            AddChild(element);
            SetElementPosition(element);
        }

        /// <summary>
        /// Removes an element from the grid
        /// </summary>
        /// <param name="element"></param>
        public void RemoveElement(BaseRenderItem element)
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
        private void SetElementPosition(BaseRenderItem element)
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

            // set the position of the element properly
            //todo add more refined positioning
            element.Transform.LocalPosition = new Vector2(elementCol.Left, elementRow.Top);

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
            public GridViewColumn Column { get; }

            public GridViewRow Row { get; }

            public GridLocation(GridViewRow row, GridViewColumn column)
            {
                Row = row;
                Column = column;
            }
        }
    }


}
