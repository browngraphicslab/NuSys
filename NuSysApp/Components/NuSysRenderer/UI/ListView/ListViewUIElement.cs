﻿using System;
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
        private List<ListViewRowUIElement<T>> _listViewRowUIElements;
        private HashSet<ListViewRowUIElement<T>> _selectedElements;
        private bool _multipleSelections;
        public ListViewUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<T> itemsSource = null) : base(parent, resourceCreator)
        {
            _itemsSource = new List<T>();
            _listColumns = new List<ListColumn<T>>();
            _listViewRowUIElements = new List<ListViewRowUIElement<T>>();
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
            _listViewRowUIElements.Clear();
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
                listViewRowUIElement.Background = Colors.Purple;
                listViewRowUIElement.Bordercolor = Colors.White;
                listViewRowUIElement.BorderWidth = 2;
                listViewRowUIElement.Width = 400;
                listViewRowUIElement.Height = 100;
                foreach (var column in _listColumns)
                {
                    Debug.Assert(column != null);
                    var cell = column.ColumnFunction(itemSource, listViewRowUIElement, ResourceCreator);
                    Debug.Assert(cell != null);
                    listViewRowUIElement.AddCell(cell);
                }
                _listViewRowUIElements.Add(listViewRowUIElement);
                _children.Add(listViewRowUIElement);

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
            _listViewRowUIElements.RemoveAll(row => itemsToRemove.Contains(row.Item));
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
            _listColumns.Add(listColumn);
            foreach (var row in _listViewRowUIElements)
            {
                var cell = listColumn.ColumnFunction(row.Item, row, ResourceCreator);
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
            foreach (var row in _listViewRowUIElements)
            {
                row.DeleteCell(columnIndex);
            }
        }

        /// <summary>
        /// Scrolls down to the item
        /// </summary>
        /// <param name="item"></param>
        public void ScrollTo(T item)
        {
            
        }

        /// <summary>
        /// This method will select the row corresponding to the item passed in
        /// </summary>
        public void SelectItem(T item)
        {
            if (item == null)
            {
                Debug.Write("Trying to select a null item idiot");
                return;
            }
            var rowToSelect = _listViewRowUIElements.First(row => row.Item.Equals(item));
            if (rowToSelect == null)
            {
                Debug.Write("Could not find the row corresponding to the item you with to select");
                return;
            }
            _selectedElements.Add(rowToSelect);
            rowToSelect.Select();
        }

        /// <summary>
        /// This method will deselect the row corresponding 
        /// </summary>
        /// <param name="item"></param>
        public void DeselectItem(T item)
        {
            if (item == null)
            {
                Debug.Write("Trying to deselect a null item idiot");
                return;
            }
            var rowToSelect = _selectedElements.First(row => row.Item.Equals(item));
            if (rowToSelect == null)
            {
                Debug.Write("Could not find the row corresponding to the item you with to deselect");
                return;
            }
            _selectedElements.Remove(rowToSelect);
            rowToSelect.Deselect();
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
            foreach (var row in _listViewRowUIElements)
            {
                row.Transform.LocalPosition = new Vector2(cellVerticalOffset, BorderWidth);
                cellVerticalOffset += row.Height;
            }
            base.Draw(ds);

        }

    }
}
