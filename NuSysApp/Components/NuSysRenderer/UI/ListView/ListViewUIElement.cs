using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using SharpDX;
using Vector2 = System.Numerics.Vector2;

namespace NuSysApp
{
    public class ListViewUIElement<T> : RectangleUIElement
    {
        private List<T> _itemsSource;
        private List<ListColumn<T>> _listColumns;
        //private List<ListViewRowUIElement<T>> _listViewRowUIElements;
        private HashSet<ListViewRowUIElement<T>> _selectedElements;
        public bool MultipleSelections { get; set; }
        private float _rowHeight;

        /// <summary>
        /// The width of the last column should always fill up the remaining space in the row.
        /// </summary>
        private float _lastColumnWidth;

        public override float Width
        {
            get { return base.Width; }
            set
            {
                float diff = value - base.Width;
                if (_lastColumnWidth + diff > 0)
                {
                    _lastColumnWidth = _lastColumnWidth + diff;
                    base.Width = value;
                }
                else
                {
                    Debug.WriteLine("Your trying to adjust the width of the list view, but doing so would cut the cells off so i'm not letting you adjust it");
                }

            }
        }

        public ListViewUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<T> itemsSource = null, float rowHeight = 30) : base(parent, resourceCreator)
        {
            _itemsSource = new List<T>();
            _listColumns = new List<ListColumn<T>>();
            MultipleSelections = false;
            if (rowHeight <= 0)
            {
                Debug.Write("You tried to give a negative or 0 row height to the list view ui element, idiot. I'm just taking the absolute value of it");
            }
            _rowHeight = Math.Abs(rowHeight);
            //_listViewRowUIElements = new List<ListViewRowUIElement<T>>();
            _selectedElements = new HashSet<ListViewRowUIElement<T>>();
            if (itemsSource != null)
            {
                AddItems(itemsSource);
            }
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
                listViewRowUIElement.BorderWidth = 2;
                listViewRowUIElement.Width = 300;
                listViewRowUIElement.Height = _rowHeight;
                foreach (var column in _listColumns)
                {
                    Debug.Assert(column != null);
                    RectangleUIElement cell;
                    if (column == _listColumns.Last())
                    {
                        cell = column.GetColumnCellFromItem(itemSource, listViewRowUIElement, ResourceCreator, _rowHeight, _lastColumnWidth);


                    }
                    else
                    {
                        cell = column.GetColumnCellFromItem(itemSource, listViewRowUIElement, ResourceCreator, _rowHeight);

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

        private void ListViewRowUIElement_Deselected(ListViewRowUIElement<T> rowUIElement, RectangleUIElement cell)
        {
            DeselectRow(rowUIElement);
        }

        /// <summary>
        /// This just adds the row UI element to the selected list
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
        /// This should add the listColumn to the _listColumns.
        /// This should also update all the ListViewRowUIElements apprpriately by adding the proper cells.
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
            _lastColumnWidth = _lastColumnWidth - (_listColumns.Count != 0 ?  _listColumns.Last().Width : 0);
            _listColumns.Add(listColumn);
            foreach (var child in _children)
            {
                var row = child as ListViewRowUIElement<T>;
                if (row == null)
                {
                    continue;
                }
                var cell = listColumn.GetColumnCellFromItem(row.Item, row, ResourceCreator, _rowHeight);
                row.AddCell(cell);
            }
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
                    _listColumns.RemoveAt(i);
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
            _lastColumnWidth = _lastColumnWidth + _listColumns[columnIndex].Width;
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
