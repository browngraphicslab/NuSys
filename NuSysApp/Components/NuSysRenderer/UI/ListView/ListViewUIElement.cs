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

namespace NuSysApp
{
    public class ListViewUIElement<T> : RectangleUIElement
    {
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
        /// The width of the last column should always fill up the remaining space in the row.
        /// </summary>
        private float _lastColumnWidth;

        /// <summary>
        /// This is here so its easier to debug (i can set breakpoint at setter so i can see when it changes.)
        /// </summary>
        private float LastColumnWidth
        {
            get { return _lastColumnWidth; }
            set
            {
                _lastColumnWidth = value;
            }
        }

        /// <summary>
        /// Whether the list can have multiple or only single selections
        /// </summary>
        public bool MultipleSelections { get; set; }

        /// <summary>
        /// THis is the heigh of each of the rows
        /// </summary>
        public float RowHeight { get; set; }

        /// <summary>
        /// This is the thickness of row ui element border
        /// </summary>
        public float RowBorderThickness { get; set; }
        
        /// <summary>
        /// This overrides the setter for the width property. It calculates the width of the last column so that
        /// all the space is filled up
        /// </summary>
        public override float Width
        {
            get { return base.Width; }
            set
            {
                float diff = value - base.Width - (BorderWidth * 2 + RowBorderThickness * 2);
                if (_lastColumnWidth + diff > 0)
                {
                    LastColumnWidth = LastColumnWidth + diff;
                    base.Width = value;
                }
                else
                {
                    Debug.WriteLine("Your trying to adjust the width of the list view, but doing so would cut the cells off so i'm not letting you adjust it");
                }

            }
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
            MultipleSelections = false;
            RowBorderThickness = 5;
            RowHeight = 40;
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
                foreach (var column in _listColumns)
                {
                    Debug.Assert(column != null);
                    RectangleUIElement cell;
                    if (column == _listColumns.Last())
                    {
                        cell = CreateCell(column, itemSource, listViewRowUIElement, true);
                    }
                    else
                    {
                        cell = CreateCell(column, itemSource, listViewRowUIElement);
                    }
                    Debug.Assert(cell != null);
                    listViewRowUIElement.AddCell(cell);
                }
                //_listViewRowUIElements.Add(listViewRowUIElement);
                listViewRowUIElement.Selected += ListViewRowUIElement_Selected;
                listViewRowUIElement.Deselected += ListViewRowUIElement_Deselected;
                _children.Add(listViewRowUIElement);

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
        private RectangleUIElement CreateCell(ListColumn<T> column, T itemSource, ListViewRowUIElement<T> listViewRowUIElement, bool lastColumn = false)
        {
            if (lastColumn == false)
            {
                return column.GetColumnCellFromItem(itemSource, listViewRowUIElement, ResourceCreator,
                    RowHeight - RowBorderThickness * 2);
            }
            else
            {
                return column.GetColumnCellFromItem(itemSource, listViewRowUIElement, ResourceCreator, RowHeight - RowBorderThickness * 2, _lastColumnWidth);
            }
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
            _children.RemoveAll(child => rowsToRemove.Contains(child));
            _selectedElements.RemoveWhere(row => itemsToRemove.Contains(row.Item));
        }

        /// <summary>
        /// This should add the listColumn to the _listColumns. You should call populate list after this.
        /// This method will not graphically reload the entire list.
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(ListColumn<T> listColumn)
        {
            if (listColumn == null)
            {
                Debug.Write("You are trying to add a null column to the list view");
                return;
            }
            LastColumnWidth = _lastColumnWidth - (_listColumns.Count != 0 ?  _listColumns.Last().Width : 0);
            foreach (var child in _children)
            {
                var row = child as ListViewRowUIElement<T>;
                if (row == null)
                {
                    continue;
                }
                //This part adjust what was previously the last cell to be its normal size, so the new list column will fill up the rest of the space
                if (_listColumns.Any())
                {
                    row.DeleteCell(_listColumns.Count - 1);
                    var oldLastColumn = _listColumns.Last();
                    var oldLastCell = CreateCell(oldLastColumn, row.Item, row);
                    row.AddCell(oldLastCell);
                }

                var cell = CreateCell(listColumn, row.Item, row, true);
                row.AddCell(cell);
            }
            _listColumns.Add(listColumn);

        }

        /// <summary>
        /// This should remove the column with the name from _listColumns.
        /// This should also update all the listviewRowUIElements appropriately by removing the proper cells.
        /// This method will not graphically reload the entire list
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
            foreach (var child in _children)
            {
                var row = child as ListViewRowUIElement<T>;
                if (row == null)
                {
                    continue;
                }
                row.DeleteCell(columnIndex);
            }
            LastColumnWidth = _lastColumnWidth + _listColumns[columnIndex].Width;
            _listColumns.RemoveAt(columnIndex);

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
        private void SelectRow(ListViewRowUIElement<T> rowToSelect)
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
            //if either columns is the last column
            if (columnAIndex == _listColumns.Count - 1)
            {
                //adjust the last column size accordingly
                AIndexIsLast = true;
                LastColumnWidth = _lastColumnWidth + _listColumns[columnBIndex].Width -
                                   _listColumns[columnAIndex].Width;
            }
            else if (columnBIndex == _listColumns.Count - 1)
            {
                BIndexIsLast = true;
                LastColumnWidth = _lastColumnWidth + _listColumns[columnAIndex].Width -
                                   _listColumns[columnBIndex].Width;
            }
            foreach (var child in _children)
            {
                var row = child as ListViewRowUIElement<T>;
                if (row == null)
                {
                    continue;
                }
                //If either of the cell is the last you need to change the sizes of the cells before you swap them
                if (AIndexIsLast)
                {
                    row.SetCellWidth(columnAIndex, _listColumns[columnAIndex].Width);
                    row.SetCellWidth(columnBIndex, _lastColumnWidth);
                }
                else if (BIndexIsLast)
                {
                    row.SetCellWidth(columnBIndex, _listColumns[columnBIndex].Width);
                    row.SetCellWidth(columnAIndex, _lastColumnWidth);
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

        public override void Draw(CanvasDrawingSession ds)
        {
            var cellVerticalOffset = BorderWidth;
            foreach (var child in _children)
            {
                var row = child as ListViewRowUIElement<T>;
                if (row == null)
                {
                    continue;
                }
                row.Transform.LocalPosition = new Vector2(BorderWidth, cellVerticalOffset);
                cellVerticalOffset += row.Height;
            }
            base.Draw(ds);
        }

    }
}
