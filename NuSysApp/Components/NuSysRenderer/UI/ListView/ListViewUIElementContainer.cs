﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
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
        private float _headerHeight;

        /// <summary>
        /// This is the header ui element
        /// </summary>
        private ListViewHeader<T> _header;

        private FlyoutPopupGroup _popupGroup;

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
        /// Get the height of all the rows in the list view
        /// </summary>
        public float HeightOfAllRows => ListView.HeightOfAllRows;


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
        /// Sets the Height of the width, //todo find out who did this and give them a medal
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
            ListView = new ListViewUIElement<T>(this, resourceCreator);

            ListView.RowTapped += ListViewRowTapped;
            ListView.RowDragged += ListView_RowDragged;
            ListView.RowDragCompleted += ListView_RowDragCompleted;
            ListView.RowDoubleTapped += ListView_RowDoubleTapped;


            _header = new ListViewHeader<T>(this, resourceCreator);
            AddHeaderHandlers();
            AddChild(_header);
            ShowHeader = true;
        }

        private void AddHeaderHandlers()
        {
            _header.HeaderDragged += Header_HeaderDragged;
            _header.HeaderDragCompleted += Header_HeaderDragCompleted;
            _header.HeaderTapped += Header_HeaderTapped;
            _header.HeaderOptionsActivated += Header_HeaderOptionsActivated;
            _header.HeaderResizing += Header_HeaderResizing;
            _header.HeaderResizeCompleted += Header_HeaderResizeCompleted;
        }



        private void RemoveHeaderHandlers()
        {
            _header.HeaderDragged -= Header_HeaderDragged;
            _header.HeaderDragCompleted -= Header_HeaderDragCompleted;
            _header.HeaderTapped -= Header_HeaderTapped;
            _header.HeaderOptionsActivated -= Header_HeaderOptionsActivated;
            _header.HeaderResizing-= Header_HeaderResizing;
            _header.HeaderResizeCompleted -= Header_HeaderResizeCompleted;
        }




        /// <summary>
        /// Adds the column options as ButtonUIElements to the popup passed in
        /// </summary>
        /// <param name="popup"></param>
        /// <param name="columns"></param>
        private void AddColumnOptionsToPopup(FlyoutPopup popup, IEnumerable<ListColumn<T>> columns)
        {
            foreach (var column in columns)
            {
                popup.AddFlyoutItem(column.Title, OnColumnOptionTapped, ResourceCreator);
            }
            //finally, add the custom option
            var customMetadataPopup = new RectangleButtonUIElement(this, ResourceCreator);
            customMetadataPopup.Background = Colors.White;
            customMetadataPopup.BorderColor = Constants.MED_BLUE;
            customMetadataPopup.ButtonTextColor = Constants.ALMOST_BLACK;
            customMetadataPopup.BorderWidth = 1;
            //customMetadataPopup.ButtonText = "fuck";
            customMetadataPopup.ButtonTextSize = 12;
            customMetadataPopup.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            customMetadataPopup.SelectedBackground = Constants.LIGHT_BLUE;

            customMetadataPopup.IsHitTestVisible = false;
            customMetadataPopup.IsChildrenHitTestVisible = true;

            popup.AddDummyFlyoutItem(customMetadataPopup, OnCustomOptionTapped);

            var textBox = new ScrollableTextboxUIElement(this, Canvas, false, true)
            {
                Width = customMetadataPopup.Width * 0.66f,
                Height = customMetadataPopup.Height,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Left,
                TextVerticalAlignment = CanvasVerticalAlignment.Bottom,
                FontSize = 14,
                BorderWidth = 1,
                BorderColor = Constants.MED_BLUE,
                Background = Colors.White,
                FontFamily = UIDefaults.TextFont
            };
            customMetadataPopup.AddChild(textBox);

            var button = new RectangleButtonUIElement(this, ResourceCreator)
            {
                Width = customMetadataPopup.Width * 0.34f,
                Height = customMetadataPopup.Height,
                ButtonText = "Add",
                Background = Constants.MED_BLUE
            };
            customMetadataPopup.AddChild(button);
            button.Transform.LocalX += textBox.Width; 

        }

        private void OnCustomOptionTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {

        }

        private void OnColumnOptionTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var button = item as ButtonUIElement;
            var columns = ListView.ColumnOptions.Where(a => a.Title == button.ButtonText);
            var column = columns.First();

            if (column == null)
            {
                Debug.Assert(false, "Column should not be null");
                return;
            }
            AddColumn(column);
            _popupGroup.DismissAllPopups();

        }


        /// <summary>
        /// Calls ListViewUIElement's clearfilter method
        /// </summary>
        public void ClearFilter()
        {
            _listview.ClearFilter();
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

        private void Header_HeaderOptionsActivated(ListViewHeaderItem<T> header)
        {
            _popupGroup = new FlyoutPopupGroup(this, Canvas, header);
            var addDeleteColumns = _popupGroup.AddHeadFlyoutPopup();
            //Only add the Add Column option if there are any column options not already in the list
            if (ListView.ColumnOptions.Where(col => !ListView.ListColumns.Contains(col)).Count() > 0)
            {
                addDeleteColumns.AddFlyoutItem("add column", AddColumnTapped, Canvas);
            }
            //Only add the Delete Column option if there are more than one columns
            if (ListView.ListColumns.Count > 1)
            {
                addDeleteColumns.AddFlyoutItem("delete", DeleteColumnTapped, Canvas);
            }
            _popupGroup.Transform.LocalPosition = new Vector2(header.Transform.LocalX, header.Height);
            AddChild(_popupGroup);
        }
        /// <summary>
        /// Called when you press the "add column" flyout item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddColumnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var options = ListView.ColumnOptions.Where(col => !ListView.ListColumns.Contains(col));
            var button = item as ButtonUIElement;
            var newpopup = _popupGroup.AddFlyoutPopup(button);
            AddColumnOptionsToPopup(newpopup, options);
        }
        /// <summary>
        /// Called when you press the "delete" flyout item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void DeleteColumnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var header = _popupGroup.Source as ListViewHeaderItem<T>;
            if (header == null)
            {
                return;
            }
            RemoveColumn(header.Column);
            _popupGroup.DismissAllPopups();
        }


        /// <summary>
        /// Whenever a header is tapped just sort the list by that column
        /// </summary>
        /// <param name="columnIndex"></param>
        private void Header_HeaderTapped(int columnIndex, CanvasPointer pointer)
        {
            SortByCol(columnIndex);
        
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
            return _listview.ItemsSource;
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

        public void AddColumnOptions(IEnumerable<ListColumn<T>> listColumns)
        {
            ListView.AddColumnOptions(listColumns);
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
        public void RemoveColumn(ListColumn<T> column)
        {
            //Don't remove any column if there's only one left.
            if (ListView.ListColumns.Count == 1)
            {
                return;
            }

            ListView.RemoveColumn(column);
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
        /// Multiselects rows corresponding to the items passed in.
        /// </summary>
        /// <param name="items"></param>
        public void SelectItems(IEnumerable<T> items)
        {
            var originalSetting = ListView.MultipleSelections;
            ListView.MultipleSelections = true;
            foreach(var item in items)
            {
                ListView.SelectItem(item);
            }
            ListView.MultipleSelections = originalSetting;
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
        /// <summary>a
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
                _header.Width = this.Width;
                _header.Height = UIDefaults.ListHeaderHeight;
                _header.RefreshTitles(_listview.ListColumns, ListView.Width, _listview.SumOfColRelWidths, _resourceCreator);
            }
        }
        /// <summary>
        /// Calls ListViewUIElement's FilterBy methodc
        /// </summary>
        /// <param name="filter"></param>
        public void FilterBy(Func<T, bool> filter)
        {
            _listview.FilterBy(filter);
        }

        /// <summary>
        /// draw the list container and its inner children (the listview and the header)
        /// those in turn draw their children
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            //draw the listview below the header
            float offset = 0;
            if (ShowHeader)
            {
                offset = _header.Height; //Offset should be header's height if there is a header
            }
            //Otherwise, vertical offset should stay 0
            _listview.Transform.LocalPosition = new Vector2(0, offset);

            base.Draw(ds);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _header.Width = Width;

            base.Update(parentLocalToScreenTransform);
        }
    }
}
