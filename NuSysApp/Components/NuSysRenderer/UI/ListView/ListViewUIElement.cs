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
using Windows.Devices.Input;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.Geometry;
using NetTopologySuite.GeometriesGraph;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;
using Color = Windows.UI.Color;
using Matrix3x2 = System.Numerics.Matrix3x2;

namespace NuSysApp
{
    /// <summary>
    /// Guys You should never be instatiating this class. Use the ListViewUIElementContainer!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListViewUIElement<T> : ScrollableRectangleUIElement
    {
        #region public
        /// <summary>
        /// If the row was selected by a click this will give you the item of the row that was selected and the column 
        /// title that was clicked. If you select a row programatically it will just give you the item. The string columnName will
        /// be null.
        /// </summary>
        public delegate void RowTappedEventHandler(T item, String columnName, CanvasPointer pointer, bool isSelected);
        public event RowTappedEventHandler RowTapped;

        public delegate void RowDraggedEventHandler(T item, string columnName, CanvasPointer pointer);
        public event RowDraggedEventHandler RowDragged;

        public delegate void RowDoubleTappedEventHandler(T item, string columnName, CanvasPointer pointer);
        public event RowDoubleTappedEventHandler RowDoubleTapped;

        /// <summary>
        /// This is the thickness of row ui element border
        /// </summary>
        public float RowBorderThickness
        {
            get { return _rowBorderThickness; }
            set
            {
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

        public List<T> ItemsSource
        {
            get
            {
                return _itemsSource;
            }
            
        }
        public List<ListViewRowUIElement<T>> Rows { set; get; }
        public List<ListColumn<T>> ListColumns
        {
            get { return _listColumns; }
        }

        public HashSet<ListColumn<T>> ColumnOptions
        {
            get { return _columnOptions; }
        }

        /// <summary>
        /// If this is true, the color will not change when you click on an item, and it will not be added to the selected list.
        /// </summary>
        public bool DisableSelectionByClick { get; set; }


        
        public delegate void RowDragCompletedEventHandler(T item, string columnName, CanvasPointer pointer);

        /// <summary>
        /// This event will fire when you release after dragging a row
        /// </summary>
        public event RowDragCompletedEventHandler RowDragCompleted;


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
        public float RowHeight { get; set; } = 40; //TODO drop this hard-code

        /// <summary>
        /// This is a value that is used as a threshold for whether determining an action was a scroll or a drag.
        /// The higher it is, the more often a drag is detected.
        /// It should be roughly near 0.25
        /// </summary>
        public float DragThreshold { set; get; }

        /// <summary>
        /// This is a value that, like DragThreshold, is used for determining whether an action was a scroll or a drag.
        /// The higher it is, the more a drag is detected.
        /// 
        /// It should be a value from 0 to PI/2. it should be roughly near 0.8
        /// </summary>
        public float DragAngleThreshold { set; get; }
        #endregion public

        #region private
        /// <summary>
        /// This represents the column that the list is sorted by. If it isn't sorted by any index,
        /// this is null
        /// </summary>
        private ListColumn<T> _columnSortedBy;

        private Vector2 _startingDragPoint;
        /// <summary>
        /// X of point last given by Dragged
        /// </summary>
        private float _x;
        /// <summary>
        /// Y of point last given by Dragged
        /// </summary>
        private float _y;

        /// <summary>
        /// This is set to true when dragging to that when a pointer released event is fired, we 
        /// can fire the drag completed event if necessary
        /// </summary>
        private bool _isDragging;

        private bool _isScrolling;

        private T _draggedItem;
        /// <summary>
        /// The list of items (e.g library element models)
        /// </summary>
        private List<T> _itemsSource;


        private List<T> _filteredItems;
         
        /// <summary>
        /// The list of columns. This graphical order of the columns in ListView is the same order as this order
        /// </summary>
        private List<ListColumn<T>> _listColumns;

        private HashSet<ListColumn<T>> _columnOptions;

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
        /// The total height of all the rows in the list view
        /// </summary>
        public float HeightOfAllRows { get { return _filteredItems.Count * RowHeight; } }

        /// <summary>
        /// Current filter function that takes in a T (eg, a LibraryElementModel) and returns
        /// a bool (ie, true if that item should be filtered in and false otherwise)
        /// </summary>
        private Func<T, bool> _currentFilter;
        /// <summary>
        /// sum of column relative widths
        /// </summary>
        private float _sumOfColumnRelativeWidths;

        private float _rowBorderThickness;

        private float _scrollVelocity;


        private bool _needsScroll;
        private T _scrollItem;
        private bool _needsCreateRows;
        #endregion private

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
            _filteredItems = new List<T>();
            _listColumns = new List<ListColumn<T>>();
            _columnOptions = new HashSet<ListColumn<T>>();

            DragThreshold = 0.2f;
            DragAngleThreshold = 0.8f;
            _scrollOffset = 0;
            MultipleSelections = false;
            BorderWidth = 0;
            Rows = new List<ListViewRowUIElement<T>>();
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, Width, Height));
            _selectedElements = new HashSet<T>();
 
        }

        #region Adding and Removing
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
            //If it has a filter, then add the items to the filteredItems if the filter function returns true
            if (HasFilter())
            {
                _filteredItems.AddRange(itemsToAdd.Where(_currentFilter));
            }
            else
            {
                //If it has no filter, simply add all elements to filteredItems
                _filteredItems.AddRange(itemsToAdd);
            }

            //Make RowUIElements
            CreateListViewRowUIElements();
        }

        /// <summary>
        /// Removes things from the _filteredItems list and the _itemsSource list. Removes the Row from the ListViewRowUIElements list.
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
            _filteredItems.RemoveAll(item => itemsToRemove.Contains(item));
            //Do I also need to remove handlers here?
            _selectedElements.RemoveWhere(row => itemsToRemove.Contains(row));

            if (_filteredItems.Count <= Rows.Count)
            {
                CreateListViewRowUIElements();
            }
        }


        /// <summary>
        /// Removes all items from the list. Clears item source, selectedElements, and calls createlistviewrowuielements.
        /// </summary>
        public void ClearItems()
        {
            _itemsSource.Clear();
            _filteredItems.Clear();
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
            rowToRemoveHandlersFrom.RowTapped -= ListViewRowUIElementOnRowTapped;
            rowToRemoveHandlersFrom.RowDoubleTapped -= ListViewRowUIElementOnRowDoubleTapped;
            rowToRemoveHandlersFrom.RowDragged -= ListViewRowUIElement_Dragged;
            rowToRemoveHandlersFrom.RowDragStarted -= ListViewRowUIElement_RowDragStarted;
            rowToRemoveHandlersFrom.PointerWheelChanged -= ListViewRowUIElement_PointerWheelChanged;
            rowToRemoveHandlersFrom.RowHolding -= ListViewRowUIElement_RowHolding;
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
            
            _columnOptions.UnionWith(listColumns);
            _listColumns.AddRange(listColumns);
            foreach (var col in listColumns)
            {
                _sumOfColumnRelativeWidths += col.RelativeWidth;
            }
            RepopulateExistingListRows();
        }

        public void AddColumnOptions(IEnumerable<ListColumn<T>> listColumns)
        {
            _columnOptions.UnionWith(listColumns);
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
        public void RemoveColumn(ListColumn<T> column)
        {
            int columnIndex = _listColumns.IndexOf(column);

            if (columnIndex == -1)
            {
                Debug.Write("You tried to remove a column from a list view that doesnt exist");
                return;
            }
            _sumOfColumnRelativeWidths -= _listColumns[columnIndex].RelativeWidth;
            _listColumns.RemoveAt(columnIndex);
            column?.Dispose();
            RepopulateExistingListRows();
        }
        #endregion Adding and Removing

        #region Filtering

        /// <summary>
        /// HasFilter returns true if there is curretnly a filter
        /// </summary>
        /// <returns></returns>
        public bool HasFilter()
        {
            if (_currentFilter != null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// ClearFilter is called when the filter is empty (ie, all the items in the itemsSource
        /// should be displayed).
        /// </summary>
        public void ClearFilter()
        {
            _currentFilter = null;

            _filteredItems.Clear();
            _filteredItems.AddRange(_itemsSource);
            //Only recreate the rows if the number of rows needed does not equal current number of rows
            var numberOfRowsNeeded = Math.Min(_filteredItems.Count, (int)Math.Ceiling(Height / RowHeight) + 1);
            if (numberOfRowsNeeded != Rows.Count)
            {
                CreateListViewRowUIElements();
            }
        }
        /// <summary>
        /// This method filters the items in the list using the function passed in.
        /// The filter function takes in a T (eg, a LibraryElementModel) and outputs a
        /// bool (eg, true if its title contains some string)
        /// 
        /// </summary>
        /// <param name="filter"></param>
        public void FilterBy(Func<T, bool> filter)
        {
            _filteredItems.Clear();
            _currentFilter = filter;

            //Set filteredItems to the items in the itemsSource list for which the filter function returns true
            _filteredItems.AddRange(_itemsSource.ToArray().Where(filter));

            //Only recreate the rows if the number of rows needed does not equal current number of rows
            var numberOfRowsNeeded = Math.Min(_filteredItems.Count, (int)Math.Ceiling(Height / RowHeight) + 1);
            if (numberOfRowsNeeded != Rows.Count)
            {
                CreateListViewRowUIElements();
            }


            //Update position and range of scroll
            SetPosition(ScrollBar.Position); //This line is necessary, in case the position of the list does not match the position of the scrollbar
            ScrollBar.Range = (Height - BorderWidth * 2f) / (HeightOfAllRows);

        }
        #endregion Filtering

        #region Scrolling
        /// <summary>
        /// Listens to ScrollBar's position, and udates the scrolloffset by denormalized
        /// position.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        public override void ScrollBarPositionChanged(object source, float position)
        {
            SetPosition(position);
            _scrollVelocity = 0;

        }

        /// <summary>
        /// Sets flags to scroll down to the item.
        /// 
        /// This can be called anywhere
        /// </summary>
        /// <param name="item"></param>
        public void ScrollTo(T item)
        {
            _scrollItem = item;
            _needsScroll = true;

        }
        /// <summary>
        /// Scrolls down to the item.
        /// This should only be called in the update method
        /// </summary>
        private void ScrollToOnUpdate()
        {
            
            var i = _filteredItems.IndexOf(_scrollItem);
            if (i < 0)
            {
                return;
            }
            float position = (float)i / _filteredItems.Count;
            //Sets the position of the ScrollBar to the position of the item in the list
            SetPosition(position);


        }

        /// <summary>
        /// Returns the position of the ScrollBar if it has been initialized.
        /// Otherwise, returns 0.
        /// </summary>
        /// <returns></returns>
        private float GetPosition()
        {
            return (ScrollBar == null) ? 0 : ScrollBar.Position;
        }
        /// <summary>
        /// Sets the position of the listview to the position passed in.
        /// Also updates the scrollbar.
        /// </summary>
        /// <param name="position"></param>
        private void SetPosition(float position)
        {
            if (position + ScrollBar.Range > 1)
            {
                position = 1 - ScrollBar.Range;
            }

            if (position < 0)
            {
                position = 0;
            }
            _scrollOffset = position * (HeightOfAllRows);
            ScrollBar.Position = position;
        }
        /// <summary>
        /// Changes the position by the float passed in.
        /// If delta is negative, then Position is going down and the bar is being lowered
        /// If delta is positive, then Position is going up and the bar is being raised
        /// </summary>
        /// <param name="delta"></param>
        private void ChangePosition(float delta)
        {
            var currentPosition = GetPosition();

            SetPosition(currentPosition + delta);


        }
        #endregion Scrolling

        #region Creating, Drawing, and Updating
        /// <summary>
        /// Method that sets flags so that ListViewRowUIElements are called at next update call
        /// 
        /// Unlike CreateListViewRowUIElementsOnUpdate, this can be called from anywhere and should be called
        /// when the number of visible rows changes (eg, if the height of the list view changes).
        /// </summary>
        private void CreateListViewRowUIElements()
        {
            _needsCreateRows = true;
        }

        /// <summary>
        /// Method that creates the ListViewRowUIElements necessary to cover the screen. 
        /// 
        /// This should never be called outside the update method.
        /// </summary>
        private void CreateListViewRowUIElementsOnUpdate()
        {

            Debug.Assert(_filteredItems != null);

            //Remove handlers of rows
            foreach (var row in Rows)
            {
                RemoveRowHandlers(row);
            }
            //Clear the rows.
            Rows.Clear();

            //If itemssource is empty, no need to create rows.
            if (_filteredItems.Count == 0)
            {
                return;
            }

            var position = GetPosition();

            //This sets the position of the scroll to 0 if we are scrolled further than possible (the start index + number of rows > itemsource.count)
            if ((int)Math.Floor(position * _filteredItems.Count) + (int)Math.Ceiling(Height / RowHeight) + 1 > _filteredItems.Count)
            {
                if (ScrollBar != null)
                {
                    SetPosition(0);
                }
                position = GetPosition();

            }

            //Start index is the itemsource-index of the first item shown on the listview 
            var startIndex = (int)Math.Floor(position * _filteredItems.Count);

            //Number of rows needed to cover the screen at all times
            //Make sures that the number of rows created does not exceed the number of rows in the source
            var numberOfRows = Math.Min(_filteredItems.Count, (int)Math.Ceiling(Height / RowHeight) + 1);

            if (numberOfRows > _filteredItems.Count)
            {
                numberOfRows = _filteredItems.Count;
            }

            //Creates the row UI elements and adds them to the list.
            var rowList = _itemsSource.GetRange(startIndex, numberOfRows);

            foreach (var itemSource in rowList)
            {
                var listViewRowUIElement = new ListViewRowUIElement<T>(this, ResourceCreator, itemSource);
                listViewRowUIElement.Item = itemSource;
                listViewRowUIElement.Background = Colors.White;
                listViewRowUIElement.BorderColor = Constants.MED_BLUE;
                listViewRowUIElement.BorderWidth = RowBorderThickness;
                listViewRowUIElement.Width = Width - BorderWidth * 2;
                listViewRowUIElement.Height = RowHeight;
                PopulateListRow(listViewRowUIElement);
                listViewRowUIElement.RowPointerReleased += ListViewRowUIElement_PointerReleased;
                listViewRowUIElement.RowTapped += ListViewRowUIElementOnRowTapped;
                listViewRowUIElement.RowDoubleTapped += ListViewRowUIElementOnRowDoubleTapped;
                listViewRowUIElement.RowDragged += ListViewRowUIElement_Dragged;
                listViewRowUIElement.RowDragStarted += ListViewRowUIElement_RowDragStarted;
                listViewRowUIElement.PointerWheelChanged += ListViewRowUIElement_PointerWheelChanged;
                listViewRowUIElement.RowHolding += ListViewRowUIElement_RowHolding;
                Rows.Add(listViewRowUIElement);
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
            var startIndex = (int)Math.Floor(ScrollBar.Position * _filteredItems.Count);
            var items = _filteredItems;

            foreach (var row in Rows)
            {
                if (row == null)
                {
                    continue;
                }

                var index = startIndex + Rows.IndexOf(row);
                //Accounts for the last, empty row.
                if (index >= _filteredItems.Count)
                {
                    continue;
                }

                row.Item = items[index];


                //there MUST be a better way to do selection than this, this is crazy
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
            foreach (var row in Rows)
            {
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
            foreach (var column in _listColumns?.ToArray())
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
            var height = RowHeight;
            return column.GetColumnCellFromItem(itemSource, listViewRowUIElement, ResourceCreator,
                    height - RowBorderThickness * 2, _sumOfColumnRelativeWidths);

        }


        /// <summary>
        /// Calls update on every row, since rows are not children.
        /// Also updates clipping rect to be based on the width and height.
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _clippingRect = CanvasGeometry.CreateRectangle(ResourceCreator, new Rect(0, 0, Width, Height));

            FinishScrolling();

            //Update position and range of scroll
            SetPosition(ScrollBar.Position); //This line is necessary, in case the position of the list does not match the position of the scrollbar
            ScrollBar.Range = (Height - BorderWidth * 2f) / (HeightOfAllRows);

            //If create new rows flag is set to true, create new rows
            if (_needsCreateRows)
            {
                CreateListViewRowUIElementsOnUpdate();
                //Reset flag
                _needsCreateRows = false;
            }

            if (_needsScroll)
            {
                ScrollToOnUpdate();
                //Reset flag and reference to scroll item
                _scrollItem = default(T);
                _needsScroll = false;
            }

            //Update content of rows
            UpdateListRows();

            
            var cellVerticalOffset = BorderWidth;
            var headerOffset = Transform.LocalPosition.Y;
            var scrollOffset = _scrollOffset % (RowHeight);
            foreach (var row in Rows)
            {
                //Position is the position of the bottom of the row
                var position = cellVerticalOffset - scrollOffset + headerOffset;
                row.Transform.LocalPosition = new Vector2(BorderWidth, position);

                row?.Update(parentLocalToScreenTransform);
                cellVerticalOffset += row.Height;

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
                //Draws every row
                foreach (var row in Rows.ToArray())
                {
                    row.Draw(ds);
                }

            }
            ds.Transform = orgTransform;
            base.Draw(ds);

        }
        /// <summary>
        /// Finishes scrolling by the _scrollVelocity.
        /// </summary>
        private void FinishScrolling()
        {
            //If we are already currently scrolling manually, simply return
            if (_isScrolling)
            {
                return;
            }
            //Ignore small values
            if (Math.Abs(_scrollVelocity) < 0.0001)
            {
                _scrollVelocity = 0;
                return;
            }
            //Change position based on scroll velocity
            var deltaY = -_scrollVelocity / HeightOfAllRows;
            ChangePosition(deltaY);
            //Acceleration is relative to RowHeight
            var acceleration = RowHeight/30f; 
            
            //Finally, slow down speed by acceleration
            if (_scrollVelocity < 0)
            {
                _scrollVelocity += acceleration;
                if (_scrollVelocity > 0 || GetPosition() + ScrollBar.Range > 0.9999f) 
                {
                    _scrollVelocity = 0; //If we have reached the end, stop scrolling
                }
            }
            else
            {
                _scrollVelocity -= acceleration;
                if (_scrollVelocity < 0 || GetPosition() < 0.0001f)
                {
                    _scrollVelocity = 0; //If we have reached the start, stop scrolling
                }
          
        }

            
        }



        /// <summary>
        /// Hit tests every row in Rows.
        /// This is necessary because the RowUIElements are not children of the list view.
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <returns></returns>
        public override BaseRenderItem HitTest(Vector2 screenPoint)
        {

            var clippingRect = new Rect(0, 0, Width, Height);
            var localPoint = Vector2.Transform(screenPoint, Transform.ScreenToLocalMatrix);
            //If the point being hittested is not inside the visible part of the ListView (ie, the clipping rect), we return null.
            if (!clippingRect.Contains(localPoint.ToPoint()))
            {
                return null;
            }

            //If scroll bar is hit, return that instead of the row underneath.
            var scrollBarht = ScrollBar.HitTest(screenPoint);
            if (scrollBarht != null)
            {
                return scrollBarht;
            }
            foreach (var row in Rows.ToArray())
            {
                var ht = row.HitTest(screenPoint);
                if (ht != null)
                {
                    return ht;
                }
            }
            return base.HitTest(screenPoint);
        }
        #endregion Creating, Drawing, and Updating

        #region Event handlers

        private void ListViewRowUIElement_PointerReleased(ListViewRowUIElement<T> rowUIElement, int colIndex, CanvasPointer pointer, T item)
        {
            if (_isDragging)
            {
                RowDragCompleted?.Invoke(rowUIElement.Item, _listColumns[colIndex].Title, pointer);
                _isDragging = false;

            }
            //Remove reference to item you were dragging
            _draggedItem = default(T);
            _isScrolling = false;


        }

        private void ListViewRowUIElement_RowHolding(ListViewRowUIElement<T> rowUIElement, int colIndex, Vector2 point, T item)
        {
            SelectItem(item);
        }

        private void ListViewRowUIElement_RowDragStarted(T item, int colIndex, CanvasPointer pointer)
        {
            _draggedItem = item;
            _startingDragPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            _x = _startingDragPoint.X;
            _y = _startingDragPoint.Y;
            _isDragging = false;
            _isScrolling = false;


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
            float minX = 0f, maxX = Width, minY = 0, maxY = Height;

            //Get local point from screen point
            var point = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            float dX = point.X - _x, dY = point.Y - _y;

            _x = point.X;
            _y = point.Y;
            //Vector representing change from start position
            var deltaVector = point - _startingDragPoint;

            if (!_isDragging)
            {
                if (point.X < minX || point.X > maxX || point.Y < minY || point.Y > maxY && !_isScrolling)
                {
                    //find difference in start and end vector
                    var angle = Math.Atan(deltaVector.Y / deltaVector.X);
                    //If drag gesture inferred, set isDragging to true
                    if(Math.Abs(angle) < DragAngleThreshold)
                    {
                        _isDragging = true;
                        //If it's the first time we are leaving the listview, select the item
                        if (!DisableSelectionByClick && !(pointer.DeviceType == PointerDeviceType.Pen || SessionController.Instance.ShiftHeld))
                        {
                            SelectItem(_draggedItem);
                        }
                    }
                    else
                    {
                        _isScrolling = true;
                    }

                }
                //Otherwise, scroll
                else
                {
                    //scroll if in bounds
                    var deltaY = -dY / (HeightOfAllRows);
                    if (Math.Abs(dY) > 0.001)
                    {
                        _scrollVelocity = dY;
                    }
                    ChangePosition(deltaY);
                }
            }else
            {
                RowDragged?.Invoke(_draggedItem, rowUIElement != null ? _listColumns[colIndex].Title : null, pointer);

            }


        }

        private void ListViewRowUIElementOnRowTapped(ListViewRowUIElement<T> rowUiElement, int colIndex, CanvasPointer pointer, T item)
        {
            bool isSelected = false;
            var colTitle = _listColumns[colIndex].Title; //get the title from the columns
            var np = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            if (rowUiElement.HitTest(pointer.CurrentPoint) == null)
            {
                return;
            }

            if (pointer.DeviceType == PointerDeviceType.Pen || SessionController.Instance.ShiftHeld)
            {
                MultipleSelections = true;
                if (_selectedElements.Contains(item))
                {
                    DeselectItem(item);
                }
                else
                {
                    SelectItem(item);
                }
                MultipleSelections = false;
            }
            else
            {
                Debug.Assert(colIndex < _listColumns.Count);

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
                        isSelected = true;
                    }
                }

            }
            RowTapped?.Invoke(item, colTitle, pointer, isSelected);

        }

        private void ListViewRowUIElementOnRowDoubleTapped(ListViewRowUIElement<T> rowUiElement, int colIndex, CanvasPointer pointer, T item)
        {
            var colTitle = _listColumns[colIndex].Title;
            RowDoubleTapped?.Invoke(item, colTitle, pointer);

        }


        /// <summary>
        /// Fires RowDoubleTapped event listened by container
        /// </summary>
        /// <param name="rowUIElement"></param>
        /// <param name="colIndex"></param>
        /// <param name="pointer"></param>
        /// <param name="item"></param>
        private void ListViewRowUIElement_RowDoubleTapped(ListViewRowUIElement<T> rowUIElement, int colIndex, CanvasPointer pointer, T item)
        {
            Debug.Assert(rowUIElement != null);
            RowDoubleTapped?.Invoke(rowUIElement.Item,
                 rowUIElement != null ? _listColumns[colIndex].Title : null, pointer);
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

            if (delta < 0)
            {
                ChangePosition(1f / _filteredItems.Count); //This moves the listview down by the height of one row.

            }
            else if (delta > 0)
            {
                ChangePosition(-1f / _filteredItems.Count); //This moves the listview up by the height of one row.
            }

        }
        #endregion Event handlers

        #region Sorting

        /// <summary>
        /// This will sort the list by the column index
        /// </summary>
        /// <param name="columnIndex"></param>
        public void SortByCol(int columnIndex)
        {
            var column = ListColumns[columnIndex];

            Debug.Assert(column != null);
            Comparer<T> comparer = column.Comparer ?? column.GetDefaultComparer();
            Sort(column, comparer);

        }


        private void Sort(ListColumn<T> column, Comparer<T> comparer)
        {
            if (comparer == null)
            {
                return;
            }

            if (_columnSortedBy == column)
            {
                _itemsSource.Reverse();
                _filteredItems.Reverse();
            }
            else
            {
                _itemsSource.Sort(comparer);
                _filteredItems.Sort(comparer);
            }

            _columnSortedBy = column;
        }


        #endregion Sorting

        #region Selection
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

            if (MultipleSelections == false)
            {
                _selectedElements.Clear();
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
        /// Returns the items (not the row element) selected.
        /// </summary>
        public IEnumerable<T> GetSelectedItems()
        {
            return _selectedElements;
        }

        #endregion Selection

        #region Column manipulation

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
            leftCol.RelativeWidth = (float)(leftHeaderWidth / (rightHeaderWidth + leftHeaderWidth) * sumRelativeWidths);
            rightCol.RelativeWidth = (float)(rightHeaderWidth / (rightHeaderWidth + leftHeaderWidth) * sumRelativeWidths);
            _sumOfColumnRelativeWidths = _listColumns.Sum(col => col.RelativeWidth);
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

        #endregion Column manipulation

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

            foreach (var col in ListColumns)
            {
                col?.Dispose();
            }

            foreach (var col in ColumnOptions)
            {
                col?.Dispose();
            }
            Rows?.Clear();
            _selectedElements?.Clear();
            _itemsSource?.Clear();
            _filteredItems.Clear();
            _listColumns?.Clear();
            base.Dispose();
        }


    }
}
