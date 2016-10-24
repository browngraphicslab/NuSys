using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Import;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using SharpDX;
using Vector2 = System.Numerics.Vector2;
using System.Numerics;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class ListViewUIElement<T> : ScrollableRectangleUIElement
    {
        public delegate void RowSelectedEventHandler(T item, String columnName);
        /// <summary>
        /// If the row was selected by a click this will give you the item of the row that was selected and the column 
        /// title that was clicked. If you select a row programatically it will just give you the item. The string columnName will
        /// be null.
        /// </summary>
        public event RowSelectedEventHandler RowSelected;

        public delegate void RowDraggedEventHandler(T item, string columnName, CanvasPointer pointer);

        public event RowDraggedEventHandler RowDragged;


        /// <summary>
        /// The list of items (e.g library element models)
        /// </summary>
        private List<T> _itemsSource;

        /// <summary>
        /// The list of columns. This graphical order of the columns in ListView is the same order as this order
        /// </summary>
        private List<ListColumn<T>> _listColumns;

        //private List<ListViewRowUIElement<T>> _listViewRowUIElements;

        /// <summary>
        /// A hashset of the selected rows
        /// </summary>
        private HashSet<ListViewRowUIElement<T>> _selectedElements;
        /// <summary>
        /// A clipping rectangle the size of the list view
        /// </summary>
        private CanvasGeometry _clippingRect;
        /// <summary>
        /// Denormalized vertical offset -- makes sure the position of the list view
        /// reflects the position of the slider in the scroll bar.
        /// </summary>
        private float _scrollOffset;

        /// <summary>
        /// The combined height of every listviewuielementrow
        /// </summary>
        private float _heightOfAllRows { get { return _itemsSource.Count * RowHeight; } }

        /// <summary>
        /// sum of column relative widths
        /// </summary>
        private float _sumOfColumnRelativeWidths;

        public float SumOfColRelWidths
        {
            get { return _sumOfColumnRelativeWidths; }
        }

        /// <summary>
        /// Whether the list can have multiple or only single selections
        /// </summary>
        public bool MultipleSelections { get; set; }

        /// <summary>
        /// THis is the heigh of each of the rows
        /// </summary>
        public float RowHeight { get; set; }

        private float _rowBorderThickness;

        /// <summary>
        /// This is the thickness of row ui element border
        /// </summary>
        public float RowBorderThickness {
            get { return _rowBorderThickness; }
            set{
                _rowBorderThickness = value;
                
            }
        }


        public float Width
        {
            get
            {
                return base.Width;
                
            }
            set
            {
                base.Width = value;
                RepopulateExistingListRows();
            }
        }
        
        public List<ListColumn<T>> ListColumns
        {
            get { return _listColumns; }   
        }

        /// <summary>
        /// x and y positions necessary to see if canvas pointer is moving within the listview.
        /// these should be set by the listviewcontainer.
        /// </summary>
        private float _x;
        private float _y;
        public float X
        {
            set { _x = value; }
        }
        public float Y
        {
            set { _y = value; }
        }

        /// <summary>
        /// This is the constructor for a ListViewUIElement. You have the option of passing in an item source. 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="itemsSource"></param>
        /// <param name="rowHeight"></param>
        public ListViewUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _itemsSource = new List<T>();
            _listColumns = new List<ListColumn<T>>();
            _scrollOffset = 0;
            MultipleSelections = false;
            //RowBorderThickness = 5;
            RowHeight = 40;
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, Width, Height));
            //_listViewRowUIElements = new List<ListViewRowUIElement<T>>();
            _selectedElements = new HashSet<ListViewRowUIElement<T>>();
        }
        

        /// <summary>
        /// This method will populate the list view using the functions of the columns and the item source.
        /// </summary>
        public void PopulateListView()
        {
            //_listViewRowUIElements.Clear();
            ClearChildren();
            _selectedElements.Clear();
            CreateListViewRowUIElements(_itemsSource);
        }

        /// <summary>
        /// Appends things to the _itemsSource list. Creates new ListViewRowUIElement for each of the items
        /// and also adds those to the list of RowUIElements.
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddItems(List<T> itemsToAdd)
        {
            if (itemsToAdd == null)
            {
                Debug.Write("You are trying to add a null list of items to the ListView");
                return;
            }
            //Add items to the item source
            _itemsSource.AddRange(itemsToAdd);

            CreateListViewRowUIElements(itemsToAdd);
        }

        /// <summary>
        /// This simply creates new list view row ui elements for each of the 
        /// items passed in.
        /// </summary>
        /// <param name="itemsToCreateRow"></param>
        public void CreateListViewRowUIElements(List<T> itemsToCreateRow)
        {
            foreach (var itemSource in _itemsSource)
            {
                if (itemSource == null)
                {
                    continue;
                }
                var listViewRowUIElement = new ListViewRowUIElement<T>(this, ResourceCreator, itemSource);
                listViewRowUIElement.Item = itemSource;
                listViewRowUIElement.Background = Colors.White;
                listViewRowUIElement.Bordercolor = Colors.Blue;
                listViewRowUIElement.BorderWidth = RowBorderThickness;
                listViewRowUIElement.Width = Width - BorderWidth * 2;
                listViewRowUIElement.Height = RowHeight;
                PopulateListRow(listViewRowUIElement);
                //_listViewRowUIElements.Add(listViewRowUIElement);
                listViewRowUIElement.Selected += ListViewRowUIElement_Selected;
                listViewRowUIElement.Deselected += ListViewRowUIElement_Deselected;
                listViewRowUIElement.Dragged += ListViewRowUIElement_Dragged;
                _children.Add(listViewRowUIElement);

            }
        }



        /// <summary>
        /// This method simply clears all the cells in each of the rows and repopulates each row using the list of columns
        /// and the column function
        /// </summary>
        private void RepopulateExistingListRows()
        {
            foreach (var child in _children)
            {
                var row = child as ListViewRowUIElement<T>;
                if (row == null)
                {
                    continue;
                }
                row.Width = Width - BorderWidth * 2;

                row.RemoveAllCells();
                PopulateListRow(row);
            }
        }

        /// <summary>
        /// This populates the passed in row with the cells using the list of columns.
        /// </summary>
        /// <param name="row"></param>
        private void PopulateListRow(ListViewRowUIElement<T> row)
        {
            foreach (var column in _listColumns)
            {
                Debug.Assert(column != null);
                RectangleUIElement cell;
                cell = CreateCell(column, row.Item, row);
                Debug.Assert(cell != null);
                row.AddCell(cell);
            }
        }
             
        /// <summary>
        /// This creates a cell that will fit into the listview row ui element. It uses the function of the column passed in along with the item source passed in 
        /// to create the cell.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="itemSource"></param>
        /// <param name="listViewRowUIElement"></param>
        /// <returns></returns>
        private RectangleUIElement CreateCell(ListColumn<T> column, T itemSource, ListViewRowUIElement<T> listViewRowUIElement)
        {
                return column.GetColumnCellFromItem(itemSource, listViewRowUIElement, ResourceCreator,
                    RowHeight - RowBorderThickness * 2, _sumOfColumnRelativeWidths);

        }

        /// <summary>
        /// This is called when a list view row ui element fires its deselected event. It simply calls the select row method.
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="cell"></param>
        private void ListViewRowUIElement_Deselected(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell)
        {
            DeselectRow(rowUIElement);
        }

        /// <summary>
        /// This is called when a list view row ui element fires its selected event. It simply calls the select row method.
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="cell"></param>
        private void ListViewRowUIElement_Selected(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell)
        {
            SelectRow(rowUIElement);
        }
        
        /// <summary>
        /// event that fires when you drag on the list. 
        /// if the pointer stays within the bounds of the list, this will scroll. 
        /// if not, then the row will fire a dragged event so the user can drag the row out of the listview.
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="cell"></param>
        /// <param name="pointer"></param>
        private void ListViewRowUIElement_Dragged(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell, CanvasPointer pointer)
        {
            //calculate bounds of listview
            var minX = this.Transform.Parent.LocalX;
            var maxX = minX + Width;
            var minY = this.Transform.Parent.LocalY;
            var maxY = minY + Height;

            //check within bounds of listview
            if (pointer.CurrentPoint.X < minX || pointer.CurrentPoint.X > maxX || pointer.CurrentPoint.Y < minY ||
                pointer.CurrentPoint.Y > maxY)
            {
                //if out of bounds, invoke row drag out
                RowDragged?.Invoke(rowUIElement.Item,
                    cell != null && rowUIElement != null ? _listColumns[rowUIElement.GetColumnIndex(cell)].Title : null,
                    pointer);
            }
            else
            {
                //scroll if in bounds
                var position = pointer.DeltaSinceLastUpdate.Y;
                _scrollOffset = (float)position;
            }
            
            
        }

        
        /// <summary>
        /// Removes things from the _itemsSource list. Removes the Row from the ListViewRowUIElements list.
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void RemoveItems(List<T> itemsToRemove)
        {
            if (itemsToRemove == null)
            {
                Debug.Write("You are trying to remove a null list from items to the ListView");
                return;
            }
            _itemsSource.RemoveAll(item => itemsToRemove.Contains(item));
            var rowsToRemove = _children.Where(row => row is ListViewRowUIElement<T> && itemsToRemove.Contains((row as ListViewRowUIElement<T>).Item));
            foreach (ListViewRowUIElement<T> row in rowsToRemove)
            {
                row.Selected -= ListViewRowUIElement_Selected;
                row.Deselected -= ListViewRowUIElement_Deselected;
            }
            _children.RemoveAll(delegate(BaseRenderItem item)
            {
                var cell = item as ListViewRowUIElement<T>;
                if (cell != null && rowsToRemove.Contains(cell))
                {
                    RemoveRowHandlers(cell);
                    return true;
                }
                return false;
            });
            //Do I also need to remove handlers here?
            _selectedElements.RemoveWhere(row => itemsToRemove.Contains(row.Item));
        }

        private void RemoveRowHandlers(ListViewRowUIElement<T> rowToRemoveHandlersFrom)
        {
            rowToRemoveHandlersFrom.Selected -= ListViewRowUIElement_Selected;
            rowToRemoveHandlersFrom.Deselected -= ListViewRowUIElement_Deselected;
            rowToRemoveHandlersFrom.Dragged -= ListViewRowUIElement_Dragged;
        }

        /// <summary>
        /// This adds all the columns to _listColumns. If you are adding multiple columns use this instead of the AddColumn method
        /// so that the list only reloads once.
        /// </summary>
        /// <param name="listColumns"></param>
        public void AddColumns(IEnumerable<ListColumn<T>> listColumns)
        {
            if (listColumns == null)
            {
                Debug.Write("You are trying to add a null list of column to the list view");
                return;
            }
            _listColumns.AddRange(listColumns);
            foreach (var col in listColumns)
            {
                _sumOfColumnRelativeWidths += col.RelativeWidth;
            }
            RepopulateExistingListRows();
        }

        /// <summary>
        /// This should add the listColumn to the _listColumns.
        /// This should also update all the listviewRowUIElements appropriately by repopulating the row with cells with the proper widths.
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(ListColumn<T> listColumn)
        {
            if (listColumn == null)
            {
                Debug.Write("You are trying to add a null column to the list view");
                return;
            }
            _sumOfColumnRelativeWidths += listColumn.RelativeWidth;
            _listColumns.Add(listColumn);
            RepopulateExistingListRows();
        }


        /// <summary>
        /// This should remove the column with the name from _listColumns.
        /// This should also update all the listviewRowUIElements appropriately by repopulating the row with cells with the proper widths.
        /// </summary>
        /// <param name="listColumn"></param>
        public void RemoveColumn(string columnTitle)
        {
            if (columnTitle == null)
            {
                return;
            }
            int columnIndex = -1;
            for (int i = 0; i < _listColumns.Count; i++)
            {
                if (_listColumns[i].Title.Equals(columnTitle))
                {
                    columnIndex = i;
                }
            }
            if (columnIndex == -1)
            {
                Debug.Write("You tried to remove a column from a list view that doesnt exist");
                return;
            }
            _sumOfColumnRelativeWidths -= _listColumns[columnIndex].RelativeWidth;
            _listColumns.RemoveAt(columnIndex);
            RepopulateExistingListRows();
        }

        /// <summary>
        /// Scrolls down to the item
        /// </summary>
        /// <param name="item"></param>
        public void ScrollTo(T item)
        {
            
        }

        /// <summary>
        /// This method will select the row corresponding to the item passed in. This is what users will call when you 
        /// want to select an item in the list.
        /// </summary>
        public void SelectItem(T item)
        {
            if (item == null)
            {
                Debug.Write("Trying to select a null item idiot");
                return;
            }
            var rowToSelect = _children.First(row => row is ListViewRowUIElement<T> && (row as ListViewRowUIElement<T>).Item.Equals(item)) as ListViewRowUIElement<T>;
            SelectRow(rowToSelect);
            
        }

        /// <summary>
        /// This actually moves the row to the selected list and selects the row.
        /// </summary>
        /// <param name="rowToSelect"></param>
        private void SelectRow(ListViewRowUIElement<T> rowToSelect, RectangleUIElement cell = null)
        {
            if (rowToSelect == null)
            {
                Debug.Write("Could not find the row corresponding to the item you with to select");
                return;
            }
            if (MultipleSelections == false)
            {
                foreach (var selectedRow in _selectedElements)
                {
                    selectedRow.Deselect();
                }
                _selectedElements.Clear();
            }
            rowToSelect.Select();
            _selectedElements.Add(rowToSelect);
            RowSelected?.Invoke(rowToSelect.Item,
                cell != null && rowToSelect != null ? _listColumns[rowToSelect.GetColumnIndex(cell)].Title : null);
            
        }

        /// <summary>
        /// This method will deselect the row corresponding to the item. This is what users will call when they 
        /// want to deselect a row corresponding to an item
        /// </summary>
        /// <param name="item"></param>
        public void DeselectItem(T item)
        {
            if (item == null)
            {
                Debug.Write("Trying to deselect a null item idiot");
                return;
            }
            var rowToDeselect = _selectedElements.First(row => row.Item.Equals(item));
            DeselectRow(rowToDeselect);
        }

        /// <summary>
        /// This removes the row from the selected list and calls deselect on the row.
        /// </summary>
        /// <param name="rowToDeselect"></param>
        public void DeselectRow(ListViewRowUIElement<T> rowToDeselect)
        {
            if (rowToDeselect == null)
            {
                Debug.Write("Could not find the row corresponding to the item you with to deselect");
                return;
            }
            rowToDeselect.Deselect();
            _selectedElements.Remove(rowToDeselect);
        }



        public void SwapColumns(int columnAIndex, int columnBIndex)
        {
            if (columnAIndex == columnBIndex || columnAIndex < 0 || columnBIndex < 0 ||
                columnAIndex >= _listColumns.Count || columnBIndex >= _listColumns.Count)
            {
                Debug.WriteLine("Pass in proper indices for swapping columns");
                return;
            }
            bool AIndexIsLast = false;
            bool BIndexIsLast = false;
            foreach (var child in _children)
            {
                var row = child as ListViewRowUIElement<T>;
                if (row == null)
                {
                    continue;
                }
                row.SwapCell(columnAIndex, columnBIndex);
            }

            //swaps the columns in the list of columns
            var tmpCol = _listColumns[columnAIndex];
            _listColumns[columnAIndex] = _listColumns[columnBIndex];
            _listColumns[columnBIndex] = tmpCol;


        }


        /// <summary>
        /// Returns the items (not the row element) selected.
        /// </summary>
        public IEnumerable<T> GetSelectedItems()
        {
            return _selectedElements.Select(row => row.Item);
        }

        public override void ScrollBarPositionChanged(object source, double position)
        {
            _scrollOffset = (float) position * (_heightOfAllRows);
        }
        public override void Update(System.Numerics.Matrix3x2 parentLocalToScreenTransform)
        {
            ScrollBar.Range = (double)(Height - BorderWidth * 2) / (_heightOfAllRows);
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, Width, Height));

            base.Update(parentLocalToScreenTransform);
        }
        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);

            var orgTransform = ds.Transform;
            //Is this necessary?
            //ds.Transform = Transform.LocalToScreenMatrix;

            //Clipping in this way does not work...
            using (ds.CreateLayer(1f, _clippingRect))
            {

                var cellVerticalOffset = BorderWidth;
                foreach (var child in _children)
                {
                    var row = child as ListViewRowUIElement<T>;
                    if (row == null)
                    {
                        continue;
                    }

                    
                    //Position is the position of the bottom of the row
                    var position = cellVerticalOffset - _scrollOffset + RowHeight;

                    //Set visibility based on if the row is at all visible.
                    if (position > 0 && position < Height + RowHeight)
                    {
                        row.IsVisible = true;
                    }
                    else
                    {
                        row.IsVisible = false;
                    }
                    row.Transform.LocalPosition = new Vector2(BorderWidth, cellVerticalOffset - _scrollOffset);
                    cellVerticalOffset += row.Height;
   
                }
            }
            ds.Transform = orgTransform;


        }

    }
}
