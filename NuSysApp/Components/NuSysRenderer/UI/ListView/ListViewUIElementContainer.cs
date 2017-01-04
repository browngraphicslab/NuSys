﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// wrapper to contain a listview and its header
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ListViewUIElementContainer<T> : RectangleUIElement
    {
        /// <summary>
        /// the listview that will be contained by this container
        /// </summary>
        private ListViewUIElement<T> _listview;

        public delegate void RowTappedEventHandler(T item, String columnName, CanvasPointer pointer, bool isSelected);
        /// <summary>
        /// If the row was selected by a click this will give you the item of the row that was selected and the column 
        /// title that was clicked. If you select a row programatically it will just give you the item. The string columnName will
        /// be null.
        /// </summary>
        public event RowTappedEventHandler RowTapped;

        public delegate void RowDraggedEventHandler(T item, string columnName, CanvasPointer pointer);

        /// <summary>
        /// If a row was dragged outisde the list this event will fire.
        /// </summary>
        public event RowDraggedEventHandler RowDragged;

        public delegate void RowDragCompletedEventHandler(T item, string columnName, CanvasPointer pointer);

        /// <summary>
        /// If a row has been double tapped this event will fire.
        /// </summary>
        public event RowDoubleTappedEventHandler RowDoubleTapped;

        public delegate void RowDoubleTappedEventHandler(T item, string columnName, CanvasPointer pointer);


        /// <summary>
        /// If a row was dragged outisde the list this event will fire.
        /// </summary>
        public event RowDragCompletedEventHandler RowDragCompleted;

        /// <summary>
        /// instance variable for resourcecreator so it can make UI elements
        /// </summary>
        private ICanvasResourceCreatorWithDpi _resourceCreator;

        private bool _showHeader;

        public bool ShowHeader
        {
            get { return _showHeader; }
            set
            {
                _header.IsVisible = value;
                _listYPos = value ? _header.Height : 0;
                _showHeader = value;
            }
        }
        

        public bool DisableSelectionByClick
        {
            get { return _listview.DisableSelectionByClick; }
            set { _listview.DisableSelectionByClick = value; }
        }



        /// <summary>
        /// where listview will draw itself
        /// </summary>
        private float _listYPos;

        /// <summary>
        /// This is the header ui element
        /// </summary>
        private ListViewHeader<T> _header;


        /// <summary>
        /// setter and getter for listview
        /// adds the listview to the container's children so it can draw it relative to the container
        /// </summary>
        private ListViewUIElement<T> ListView
        {
            get
            {
                return _listview;
            }
            set
            {
                if (value != null)
                {
                    _listview = value;
                    AddChild(_listview);
                }
            }
        }

        /// <summary>
        /// This sets the background of the list view
        /// </summary>
        public override Color Background
        {
            get { return _listview.Background; }
            set
            {
                if (ListView != null)
                {
                    ListView.Background = value;
                }
            }
        }

        /// <summary>
        /// Sets the width of the list
        /// </summary>
        public override float Width
        {
            get { return base.Width; }
            set
            {
                if (base.Width != value)
                {
                    if (ListView != null)
                    {
                        ListView.Width = value;
                        if (_header != null)
                        {
                            _header.RefreshTitles(ListView.ListColumns, ListView.Width, ListView.SumOfColRelWidths, _resourceCreator);
                        }
                    }
                    base.Width = value;
                }
            }
        }

        /// <summary>
        /// Sets the Height of the width
        /// </summary>
        public override float Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                if (base.Height != value)
                {
                    if (ListView != null)
                    {
                        if (ShowHeader)
                        {
                            ListView.Height = value - _header.Height;
                        }
                        else
                        {
                            ListView.Height = value;
                        }
                    }
                    base.Height = value;
                }
            }
        }

        /// <summary>
        /// The bool on whether or not more than 1 item can be selected at a time
        /// </summary>
        public bool MultipleSelections
        {
            get { return ListView.MultipleSelections; }
            set { ListView.MultipleSelections = value; }
        }

        /// <summary>
        /// The height of each row
        /// </summary>
        public float RowHeight
        {
            get { return ListView.RowHeight; }
            set { ListView.RowHeight = value; }
        }

        /// <summary>
        /// The thickness of the border for each row.
        /// </summary>
        public float RowBorderThickness
        {
            get { return ListView.RowBorderThickness; }
            set { ListView.RowBorderThickness = value; }
        }
        

        public ListViewUIElementContainer(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _resourceCreator = resourceCreator;
            _listYPos = 0;
            ListView = new ListViewUIElement<T>(this, resourceCreator);

            ListView.RowTapped += ListViewRowTapped;
            ListView.RowDragged += ListView_RowDragged;
            ListView.RowDragCompleted += ListView_RowDragCompleted;
            ListView.RowDoubleTapped += ListView_RowDoubleTapped;

            _header = new ListViewHeader<T>(this, resourceCreator);
            _header.HeaderDragged += Header_HeaderDragged;
            _header.HeaderDragCompleted += Header_HeaderDragCompleted;
            _header.HeaderTapped += Header_HeaderTapped;
            _header.HeaderResizing += Header_HeaderResizing;
            _header.HeaderResizeCompleted += Header_HeaderResizeCompleted; ;
            AddChild(_header);
            ShowHeader = true;
        }



        /// <summary>
        /// This gets called when the user completes dragging of the a header's edge to resize a column. It changes the relative column widths
        /// of the columns who's size was changed.
        /// </summary>
        /// <param name="leftHeaderWidth"></param>
        /// <param name="rightHeaderWidth"></param>
        /// <param name="leftHeaderIndex"></param>
        private void Header_HeaderResizeCompleted(double leftHeaderWidth, double rightHeaderWidth, int leftHeaderIndex)
        {
            _listview.ChangeRelativeColumnWidths(leftHeaderWidth, rightHeaderWidth, leftHeaderIndex);
        }

        /// <summary>
        /// This is called when the user is still currently draggint the headers edge to resize a column. This function calls the moveborderaftercell function
        /// which adjust the size of the cells in a col at colIndex and colIndex +/- 1 depending on which edge is being dragged. (e.g. dragging left edge of button resizes the button being dragged and the one 
        /// that comes before it.) 
        /// </summary>
        /// <param name="colIndex"></param>
        /// <param name="pointer"></param>
        /// <param name="edgeBeingDragged"></param>
        private void Header_HeaderResizing(int colIndex, CanvasPointer pointer, ListViewHeaderItem<T>.Edge edgeBeingDragged)
        {
            if (colIndex < 0 || colIndex > _listview.ListColumns.Count - 1)
            {
                return;
            }
            if (edgeBeingDragged == ListViewHeaderItem<T>.Edge.Right)
            {
                _listview.MoveBorderAfterCell(colIndex, pointer.DeltaSinceLastUpdate.X);
            }
            else
            {
                if (colIndex - 1 < 0)
                {
                    return;  
                }
                _listview.MoveBorderAfterCell(colIndex - 1, pointer.DeltaSinceLastUpdate.X);
            }
        }

        /// <summary>
        /// Removes all listeners
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            ListView.RowTapped -= ListViewRowTapped;
            ListView.RowDragged -= ListView_RowDragged;
            ListView.RowDragCompleted -= ListView_RowDragCompleted;
            ListView.RowDoubleTapped -= ListView_RowDoubleTapped;

            _header.HeaderDragged -= Header_HeaderDragged;
            _header.HeaderDragCompleted -= Header_HeaderDragCompleted;
            _header.HeaderTapped -= Header_HeaderTapped;
            _header.HeaderResizing -= Header_HeaderResizing;
            _header.HeaderResizeCompleted -= Header_HeaderResizeCompleted;
        }

        /// <summary>
        /// This fires when the header has just finished being dragged. It will refresh the positions of the titles so it looks nice since you may not have left the header
        /// in the correct spot when dragging.
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="colIndex"></param>
        /// <param name="pointer"></param>
        private void Header_HeaderDragCompleted(ButtonUIElement header, int colIndex, CanvasPointer pointer)
        {
            _header.RefreshTitles();
        }

        /// <summary>
        /// Is called when header is currently being dragged. Checks if you should swap the columns depending on the position of the header that is being dragged,
        ///  and if you should then it swaps both the columns and the headers
        /// 
        /// </summary>
        /// <param name="header"></param>
        /// <param name="colIndex"></param>
        /// <param name="pointer"></param>
        private void Header_HeaderDragged(ButtonUIElement header, int colIndex, CanvasPointer pointer)
        {
            var newX = header.Transform.LocalX + pointer.DeltaSinceLastUpdate.X;

            if (newX > 0 && newX + header.Width < Width)
            {
                header.Transform.LocalX = newX;
                var pointerX = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix).X;

                float centerOfNextHeader = _header.GetColumnHeaderCenter(colIndex + 1);
                float centerOfPreviousHeader = _header.GetColumnHeaderCenter(colIndex - 1);

                if (pointerX > centerOfNextHeader)
                {
                    _header.SwapHeaders(colIndex, colIndex + 1);
                    _listview.SwapColumns(colIndex, colIndex + 1);
                }
                else if (pointerX < centerOfPreviousHeader)
                {
                    _header.SwapHeaders(colIndex, colIndex -1);
                    _listview.SwapColumns(colIndex, colIndex - 1);
                }
            }

        }

        /// <summary>
        /// Whenever a header is tapped just sort the list by that column
        /// </summary>
        /// <param name="columnIndex"></param>
        private void Header_HeaderTapped(int columnIndex)
        {
            _listview.SortByCol(columnIndex);
        }

        public override float BorderWidth
        {
            get { return _listview?.BorderWidth ?? 0; }
            set
            {
                if(_listview != null)
                {
                    _listview.BorderWidth = value;
                }
            }
        }


        #region RouterFunctions

        /// <summary>
        /// Returns the item source of the list
        /// </summary>
        /// <returns></returns>
        public List<T> GetItems()
        {
            return _listview.GetItems();
        }

        /// <summary>
        /// Add new items/rows to the list
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void AddItems(List<T> itemsToAdd)
        {
            ListView.AddItems(itemsToAdd);
        }

        /// <summary>
        /// Removes all items from list view
        /// </summary>
        public void ClearItems()
        {
            ListView.ClearItems();
        }

        /// <summary>
        /// Removes things from the _itemsSource list. Removes the Row from the ListViewRowUIElements list.
        /// </summary>
        /// <param name="itemsToAdd"></param>
        public void RemoveItems(List<T> itemsToRemove)
        {
            ListView.RemoveItems(itemsToRemove);
        }

        /// <summary>
        /// This adds all the columns to _listColumns. If you are adding multiple columns use this instead of the AddColumn method
        /// so that the list only reloads once.
        /// </summary>
        /// <param name="listColumns"></param>
        public void AddColumns(IEnumerable<ListColumn<T>> listColumns)
        {
            ListView.AddColumns(listColumns);
            if (ShowHeader)
            {
                GenerateHeader();
            }
        }

        /// <summary>
        /// This should add the listColumn to the _listColumns.
        /// This should also update all the listviewRowUIElements appropriately by repopulating the row with cells with the proper widths.
        /// </summary>
        /// <param name="column"></param>
        public void AddColumn(ListColumn<T> listColumn)
        {
            ListView.AddColumn(listColumn);
            if (ShowHeader)
            {
                GenerateHeader();
            }
        }

        /// <summary>
        /// This should remove the column with the name from _listColumns.
        /// </summary>
        /// <param name="listColumn"></param>
        public void RemoveColumn(string columnTitle)
        {
            ListView.RemoveColumn(columnTitle);
            if (ShowHeader)
            {
                GenerateHeader();
            }
        }

        /// <summary>
        /// Scrolls down to the item
        /// </summary>
        /// <param name="item"></param>
        public void ScrollTo(T item)
        {
            ListView.ScrollTo(item);
        }

        /// <summary>
        /// This method will select the row corresponding to the item passed in. 
        /// </summary>
        public void SelectItem(T item)
        {
            ListView.SelectItem(item);
        }

        /// <summary>
        /// This method will deselect the row corresponding to the item. This is what users will call when they 
        /// want to deselect a row corresponding to an item
        /// </summary>
        /// <param name="item"></param>
        public void DeselectItem(T item)
        {
            ListView.DeselectItem(item);
        }

        /// <summary>
        /// This swaps the places of the two different columns
        /// </summary>
        /// <param name="columnAIndex"></param>
        /// <param name="columnBIndex"></param>
        public void SwapColumns(int columnAIndex, int columnBIndex)
        {
            ListView.SwapColumns(columnAIndex, columnBIndex);
        }

        /// <summary>
        /// Returns the items (not the row element) selected.
        /// </summary>
        public IEnumerable<T> GetSelectedItems()
        {
            return ListView.GetSelectedItems();
        }

        /// <summary>
        /// Sort by the column at the specified index.
        /// </summary>
        /// <param name="colIndex"></param>
        public void SortByCol(int colIndex)
        {
            _listview.SortByCol(colIndex);
        }
        /// <summary>
        /// When the listviewuielement fires its rowdoubletapped event, the container will fire its rowdoubletapped
        /// which the user should be listening to.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void ListView_RowDoubleTapped(T item, string columnName, CanvasPointer pointer)
        {
            RowDoubleTapped?.Invoke(item, columnName, pointer);
        }
        /// <summary>
        /// Deselects all the items currently selected
        /// </summary>
        public void DeselectAllItems()
        {
            _listview.DeselectAllItems();
        }

        /// <summary>
        /// When the listview  ui element fires its row dragged event, the container will fires it's row dragged
        /// which the user should be listening to
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void ListView_RowDragged(T item, string columnName, CanvasPointer pointer)
        {
            RowDragged?.Invoke(item, columnName, pointer);
        }

        /// <summary>
        /// This fires when the dragging of a row has been completed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        /// <param name="pointer"></param>
        private void ListView_RowDragCompleted(T item, string columnName, CanvasPointer pointer)
        {
            RowDragCompleted?.Invoke(item, columnName, pointer);
        }

        /// <summary>
        /// When the listview ui element firest its row selected event, the container will fires it's row selected event which the user should be 
        /// listening to.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="columnName"></param>
        private void ListViewRowTapped(T item, string columnName, CanvasPointer pointer, bool isSelected)
        {
            RowTapped?.Invoke(item, columnName, pointer, isSelected);
        }



        #endregion RouterFunctions




        /// <summary>
        /// makes a header if you want a header
        /// </summary>
        public void GenerateHeader()
        {
            if (_listview != null)
            {
                _header.Transform.LocalPosition = new Vector2(0,0);
                _header.BorderWidth = 0;
                _header.Bordercolor = Colors.Black;
                _header.Background = Colors.Black;
                _header.Width = this.Width;
                _header.Height = _listview.RowHeight + 10;
                _listYPos = _header.Height;
                _header.RefreshTitles(_listview.ListColumns, ListView.Width, _listview.SumOfColRelWidths, _resourceCreator);
            }
        }

        /// <summary>
        /// draw the list container and its inner children (the listview and the header)
        /// those in turn draw their children
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            //draw the listview below the header
            _listview.Transform.LocalPosition = new Vector2(0, _listYPos);
            base.Draw(ds);
        }
    }
}
