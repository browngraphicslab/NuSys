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
using NetTopologySuite.GeometriesGraph;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

namespace NuSysApp
{
    /// <summary>
    /// Guys You should never be instatiating this class. Use the ListViewUIElementContainer!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListViewUIElement<T> : ScrollableRectangleUIElement
    {
        public delegate void RowTappedEventHandler(T item, String columnName);

        /// <summary>
        /// If the row was selected by a click this will give you the item of the row that was selected and the column 
        /// title that was clicked. If you select a row programatically it will just give you the item. The string columnName will
        /// be null.
        /// </summary>
        public event RowTappedEventHandler RowTapped;

        public delegate void RowDraggedEventHandler(T item, string columnName, CanvasPointer pointer);

        public event RowDraggedEventHandler RowDragged;

        /// <summary>
        /// If this is true, the color will not change when you click on an item, and it will not be added to the selected list.
        /// </summary>
        public bool DisableSelectionByClick { get; set; }

        /// <summary>
        /// This represents the column index that the array is sorted by. If it isn't sorted by any index,
        /// this is -1.
        /// </summary>
        private int _columnIndexSortedBy;
        
        public delegate void RowDragCompletedEventHandler(T item, string columnName, CanvasPointer pointer);

        /// <summary>
        /// This event will fire when you release after dragging a row
        /// </summary>
        public event RowDragCompletedEventHandler RowDragCompleted;

        /// <summary>
        /// This is set to true when dragging to that when a pointer released event is fired, we 
        /// can fire the drag completed event if necessary
        /// </summary>
        private bool _isDragging;


        /// <summary>
        /// The list of items (e.g library element models)
        /// </summary>
        private List<T> _itemsSource;

        /// <summary>
        /// The list of columns. This graphical order of the columns in ListView is the same order as this order
        /// </summary>
        private List<ListColumn<T>> _listColumns;


        /// <summary>
        /// A hashset of the selected rows
        /// </summary>
        private HashSet<T> _selectedElements;

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
                UpdateRowBorder();
            }
        }

        public float Height
        {
            get { return base.Height; }

            set
            {
                if (base.Height != value)
                {
                    base.Height = value;
                    CreateListViewRowUIElements();
                }
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
                if (base.Width != value)
                {
                    base.Width = value;
                    CreateListViewRowUIElements();
                }
            }
        }
        
        public List<ListViewRowUIElement<T>> Rows { set; get; }
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
            BorderWidth = 0;
            _columnIndexSortedBy = -1;
            //RowBorderThickness = 5;
            Rows = new List<ListViewRowUIElement<T>>();
            RowHeight = 40;
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, Width, Height));
            _selectedElements = new HashSet<T>();

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
            //Make RowUIElements
            CreateListViewRowUIElements();
        }

        /// <summary>
        /// Method that creates the ListViewRowUIElements necessary to cover the screen. 
        /// 
        /// This should be called somewhere in the constructor, as well as whenever the number of
        /// rows necessary to cover the screen changes (e.g., height changes)
        /// </summary>
        private void CreateListViewRowUIElements()
        {
            Debug.Assert(_itemsSource != null);

            //Remove handlers of rows
            foreach(var row in Rows)
            {
                RemoveRowHandlers(row);
            }
            //Clear the rows.
            Rows.Clear();


            var position = (ScrollBar == null) ? 0 : ScrollBar.Position; 
            var startIndex = (int)Math.Floor(position * _itemsSource.Count);
            var items = _itemsSource.ToArray();

            //Number of rows needed to cover the screen at all times
            var numberOfRows = (int) Math.Ceiling(Height / RowHeight) + 1;

            if (numberOfRows >= _itemsSource.Count)
            {
                numberOfRows = _itemsSource.Count - 1;
            }
            
            //Creates the row UI elements and adds them to the list.
            var rowList = _itemsSource.GetRange(startIndex, startIndex + numberOfRows);

            foreach (var itemSource in rowList)
            {
                var listViewRowUIElement = new ListViewRowUIElement<T>(this, ResourceCreator, itemSource);
                listViewRowUIElement.Item = itemSource;
                listViewRowUIElement.Background = Colors.White;
                listViewRowUIElement.Bordercolor = Colors.Black;
                listViewRowUIElement.BorderWidth = RowBorderThickness;
                listViewRowUIElement.Width = Width - BorderWidth * 2;
                listViewRowUIElement.Height = RowHeight;
                PopulateListRow(listViewRowUIElement);
                listViewRowUIElement.RowPointerReleased += ListViewRowUIElement_PointerReleased;
                listViewRowUIElement.RowDragged += ListViewRowUIElement_Dragged;
                listViewRowUIElement.PointerWheelChanged += ListViewRowUIElement_PointerWheelChanged;
                Rows.Add(listViewRowUIElement);
            }

        }

        /// <summary>
        /// This handles the PointerWheelChanged event of the rows. The delta passed in is either 1 or -1, so we move the scroll bar
        /// depending on the sign of the delta. 
        /// 
        /// 0.035 is the normalized change in position.
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="cell"></param>
        /// <param name="pointer"></param>
        /// <param name="delta"></param>
        private void ListViewRowUIElement_PointerWheelChanged(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell, CanvasPointer pointer, float delta)
        {
            
            if(delta < 0)
            {
                ScrollBar.ChangePosition(0.035);

            }
            else if(delta> 0)
            {
                ScrollBar.ChangePosition(-0.035);

            }


        }


        /// <summary>
        /// This just changes all the border widths of the rows to be the rowborderthickness variable
        /// </summary>
        private void UpdateRowBorder()
        {
            foreach (var row in Rows)
            {
                Debug.Assert(row != null);
                row.BorderWidth = RowBorderThickness;
            }
        }
        /// <summary>
        /// This method makes sure the appearence of the rows reflect the item they represent.
        /// It sets the row's Item property, then selects/deselects based on if it's selected,
        /// and then updates the content of each row.
        /// </summary>
        private void UpdateListRows()
        {
            if (ScrollBar == null)
            {
                return;
            }
            var startIndex = (int)Math.Floor(ScrollBar.Position * _itemsSource.Count);
            var items = _itemsSource.ToArray();

            foreach (var row in Rows)
            {
                if (row == null)
                {
                    continue;
                }

                var index = startIndex + Rows.IndexOf(row);
                //Accounts for the last, empty row.
                if (index == _itemsSource.Count)
                {
                    continue;
                }

                row.Item = items[index];

                if (_selectedElements.Contains(row.Item))
                {
                    SelectRow(row);
                }
                else
                {
                    DeselectRow(row);
                }

                foreach (var column in _listColumns)
                {
                    row.UpdateContent(column, _listColumns.IndexOf(column));
                }




            }
        }
        /// <summary>
        /// This method simply clears all the cells in each of the rows and repopulates each row using the list of columns
        /// and the column function
        /// </summary>
        private void RepopulateExistingListRows()
        {
            foreach(var row in Rows)
            {
                row.Width = Width - BorderWidth*2;
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

        

        private void ListViewRowUIElement_PointerReleased(ListViewRowUIElement<T> rowUIElement, int colIndex, CanvasPointer pointer, T item)
        {
            if (_isDragging)
            {
                RowDragCompleted?.Invoke(rowUIElement.Item, _listColumns[colIndex].Title, pointer);
                _isDragging = false;
            }else
            {
                var t = Transform.ScreenToLocalMatrix;
                var np = Vector2.Transform(pointer.CurrentPoint, t);
                if (rowUIElement.HitTest(pointer.CurrentPoint) == null)
                {
                    return;
                }

                Debug.Assert(colIndex < _listColumns.Count);
                var colTitle = _listColumns[colIndex].Title;
                if (_selectedElements.Contains(item))
                {
                    if (!DisableSelectionByClick)
                    {
                        DeselectItem(item);

                    }
                }
                else
                {
                    if (!DisableSelectionByClick)
                    {
                        SelectItem(item); 
                    }
                }
                RowTapped?.Invoke(item, colTitle);
                
            }
        }
        
        /// <summary>
        /// event that fires when you drag on the list. 
        /// if the pointer stays within the bounds of the list, this will scroll. 
        /// if not, then the row will fire a dragged event so the user can drag the row out of the listview.
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="cell"></param>
        /// <param name="pointer"></param>
        private void ListViewRowUIElement_Dragged(ListViewRowUIElement<T> rowUIElement, int colIndex, CanvasPointer pointer)
        {
            //calculate bounds of listview
            var minX = this.Transform.Parent.LocalX;
            var maxX = minX + Width;
            var minY = this.Transform.Parent.LocalY;
            var maxY = minY + Height;

            //We need the local point, not the screen point
            var point = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            //check within bounds of listview
            if (point.X < minX || point.X > maxX || point.Y < minY ||
                point.Y > maxY)
            {
                //if out of bounds, invoke row drag out
                RowDragged?.Invoke(rowUIElement.Item,
                     rowUIElement != null ? _listColumns[colIndex].Title : null, pointer);
                _isDragging = true;
            }
            else
            {
                //scroll if in bounds
                var deltaY =  - pointer.DeltaSinceLastUpdate.Y / Height;

                ScrollBar.ChangePosition(deltaY);

                
            }
        }

        /// <summary>
        /// This will sort the list by the column index
        /// </summary>
        /// <param name="columnIndex"></param>
        public void SortByCol(int columnIndex)
        {
            Debug.Assert(columnIndex < _listColumns.Count);
            //If it isn't sorted by this index then just sort it normally
            if (columnIndex != _columnIndexSortedBy)
            {
                _children.Sort(delegate(BaseRenderItem row1, BaseRenderItem row2)
                {
                    var str1 = (row1 as ListViewRowUIElement<T>)?.GetStringValueOfCell(columnIndex);
                    var str2 = (row2 as ListViewRowUIElement<T>)?.GetStringValueOfCell(columnIndex);
                    if (str1 == null || str2 == null)
                    {
                        return 0;
                    }
                    return str1.CompareTo(str2);
                });
                _columnIndexSortedBy = columnIndex;    
            }
            //If it is sorted by this index then sort it with reverse order
            else
            {
                _children.Sort(delegate (BaseRenderItem row1, BaseRenderItem row2)
                {
                    var str1 = (row1 as ListViewRowUIElement<T>)?.GetStringValueOfCell(columnIndex);
                    var str2 = (row2 as ListViewRowUIElement<T>)?.GetStringValueOfCell(columnIndex);
                    if (str1 == null || str2 == null)
                    {
                        return 0;
                    }
                    return str1.CompareTo(str2) * -1;
                });
                _columnIndexSortedBy = -1;
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

            //Do I also need to remove handlers here?
            _selectedElements.RemoveWhere(row => itemsToRemove.Contains(row));

            CreateListViewRowUIElements();
        }


        /// <summary>
        /// Removes all items from the list. Clears item source, selectedElements, and calls createlistviewrowuielements.
        /// </summary>
        public void ClearItems()
        {
            _itemsSource.Clear();
            _selectedElements.Clear();
            CreateListViewRowUIElements();
        }


        /// <summary>
        /// Stops listening to events from the row
        /// </summary>
        /// <param name="rowToRemoveHandlersFrom"></param>
        private void RemoveRowHandlers(ListViewRowUIElement<T> rowToRemoveHandlersFrom)
        {
            //rowToRemoveHandlersFrom.Selected -= ListViewRowUIElement_Selected;
            //rowToRemoveHandlersFrom.Deselected -= ListViewRowUIElement_Deselected;
            rowToRemoveHandlersFrom.RowPointerReleased -= ListViewRowUIElement_PointerReleased;
            rowToRemoveHandlersFrom.PointerWheelChanged -= ListViewRowUIElement_PointerWheelChanged;
            rowToRemoveHandlersFrom.RowDragged -= ListViewRowUIElement_Dragged;
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
        /// This changes the column relative widths for the column at leftHeaderIndex and the column at leftHeaderIndex + 1. This is used for resizing, since
        /// you can only resize 2 cols at the same time.
        /// </summary>
        /// <param name="leftHeaderWidth"></param>
        /// <param name="rightHeaderWidth"></param>
        /// <param name="leftColWidth"></param>
        public void ChangeRelativeColumnWidths(double leftHeaderWidth, double rightHeaderWidth, int leftHeaderIndex)
        {
            var leftCol = _listColumns[leftHeaderIndex];
            var rightCol = _listColumns[leftHeaderIndex + 1];
            float sumRelativeWidths = leftCol.RelativeWidth + rightCol.RelativeWidth;
            leftCol.RelativeWidth = (float)(leftHeaderWidth/(rightHeaderWidth + leftHeaderWidth)*sumRelativeWidths);
            rightCol.RelativeWidth = (float)(rightHeaderWidth / (rightHeaderWidth + leftHeaderWidth) * sumRelativeWidths);
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
            if (MultipleSelections == false)
            {
                _selectedElements.Clear();
            }
            _selectedElements.Add(item);            
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

            rowToSelect.Select();
            
        }

        /// <summary>
        /// This function adds the sizeChange to the width of cell at leftColIndex, and subtracts sizeChange from cell at (leftColIndex + 1) width and adds sizeChanged to the position of the (leftColIndex + 1) cell
        /// </summary>
        /// <param name="leftColIndex"></param>
        /// <param name="rightColIndex"></param>
        /// <param name="distanceToMove"></param>
        public void MoveBorderAfterCell(int leftColIndex, float sizeChange)
        {
            foreach (var row in Rows)
            {
                if (row != null)
                {
                    row.MoveBorderAfterCell(leftColIndex, sizeChange);
                }
            }
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

            if (_selectedElements.Contains(item))
            {
                _selectedElements.Remove(item);

            }
        }

        /// <summary>
        /// Clears the selected elements list
        /// </summary>
        public void DeselectAllItems()
        {
            _selectedElements.Clear();
        }

        /// <summary>
        /// This removes the row from the selected list and calls deselect on the row.
        /// </summary>
        /// <param name="rowToDeselect"></param>
        private void DeselectRow(ListViewRowUIElement<T> rowToDeselect)
        {
            if (rowToDeselect == null)
            {
                Debug.Write("Could not find the row corresponding to the item you with to deselect");
                return;
            }
            rowToDeselect.Deselect();
        }

        /// <summary>
        /// This swaps the places of the two different columns
        /// </summary>
        /// <param name="columnAIndex"></param>
        /// <param name="columnBIndex"></param>
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
            foreach (var row in Rows)
            {
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
            return _selectedElements;
        }
        /// <summary>
        /// Listens to ScrollBar's position, and udates the scrolloffset by denormalized
        /// position.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        public override void ScrollBarPositionChanged(object source, double position)
        {
            _scrollOffset = (float) position * (_heightOfAllRows);
        }
        /// <summary>
        /// Calls update on every row, since rows are not children.
        /// Also updates clipping rect to be based on the width and height.
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(System.Numerics.Matrix3x2 parentLocalToScreenTransform)
        {
            ScrollBar.Range = (double)(Height - BorderWidth * 2) / (_heightOfAllRows);
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, Width, Height));
            UpdateListRows();
            foreach (var row in Rows.ToArray())
            {
                row?.Update(parentLocalToScreenTransform);
            }
            base.Update(parentLocalToScreenTransform);
        }
        /// <summary>
        /// Draw creates a clipping and then draws every single row in Rows
        /// Because the rows are not children of the list view, we need to 
        /// call their draw methods here.
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            // Creates a clipping of the drawing session based on _clippingrect
            using (ds.CreateLayer(1f, _clippingRect))
            {

                var cellVerticalOffset = BorderWidth;
                var headerOffset = Transform.LocalPosition.Y;
                var scrollOffset = _scrollOffset % RowHeight;
                //Draws every row
                foreach (var row in Rows.ToArray())
                {
                    //Position is the position of the bottom of the row
                    var position = cellVerticalOffset - scrollOffset + headerOffset;
                    row.Transform.LocalPosition = new Vector2(BorderWidth, position);
                    row.Draw(ds);

                    cellVerticalOffset += row.Height;
                }

            }
            ds.Transform = orgTransform;
            base.Draw(ds);

        }

        /// <summary>
        /// Hit tests every row in Rows.
        /// This is necessary because the RowUIElements are not children of the list view.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {
            //If scroll bar is hit, return that instead of the row underneath.
            var scrollBarht = ScrollBar.HitTest(screenPoint);
            if(scrollBarht != null)
            {
                return scrollBarht;
            }
            foreach(var row in Rows)
            {
                var ht = row.HitTest(screenPoint);
                if (ht != null)
                {
                    return ht;
                }
            }
            return base.HitTest(screenPoint);
        }
        /// <summary>
        /// Disposes rows and clears the lists we have. The rows are not children so we have to manually call dispose on them.
        /// </summary>
        public override void Dispose()
        {
            foreach (var row in Rows)
            {
                RemoveRowHandlers(row);
                row?.Dispose();
            }
            Rows?.Clear();
            _selectedElements?.Clear();
            _itemsSource?.Clear();
            _listColumns?.Clear();
            Rows = null;
            _selectedElements = null;
            _itemsSource = null;
            _listColumns = null;
            base.Dispose();
        }

    }
}
