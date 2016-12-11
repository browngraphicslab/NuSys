using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the header of a list view
    /// </summary>
    public class ListViewHeader<T> : RectangleUIElement
    {
        public delegate void HeaderTappedEventHandler(int colIndex);

        /// <summary>
        /// Fires when header has been tapped
        /// </summary>
        public event HeaderTappedEventHandler HeaderTapped;

        
        public delegate void HeaderDraggedEventHandler(ButtonUIElement header, int colIndex, CanvasPointer pointer);

        /// <summary>
        /// Fires when header a header is being dragged
        /// </summary>
        public event HeaderDraggedEventHandler HeaderDragged;

        public delegate void HeaderDragCompletedEventHandler(ButtonUIElement header, int colIndex, CanvasPointer pointer);

        /// <summary>
        /// Fires when header is done being dragged
        /// </summary>
        public event HeaderDragCompletedEventHandler HeaderDragCompleted;

        private bool _headerBeingDragged;

        public delegate void ResizeHeaderEventHandler(int colIndex, CanvasPointer pointer, ListViewHeaderItem<T>.Edge edgeBeingDragged);

        /// <summary>
        /// When left or right borders are dragged, this event will fire
        /// </summary>
        public event ResizeHeaderEventHandler HeaderResizing;

        public delegate void ResizeHeaderCompletedEventHandler(double leftHeaderWidth, double rightHeaderWidth, int leftHeaderIndex);

        /// <summary>
        /// Once the dragging of the border has completed (pointer has been released) this event will fire.
        /// </summary>
        public event ResizeHeaderCompletedEventHandler HeaderResizeCompleted;

        /// <summary>
        /// store resource creator in instance variable so we can pass it to new textboxUIElement later
        /// </summary>
        private ICanvasResourceCreatorWithDpi _resourceCreator;

        public ListViewHeader(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _headerBeingDragged = false;
        }

        /// <summary>
        /// make the titles for all the columns of the header based off of a listview parameter
        /// </summary>
        /// <param name="listview"></param>
        public void RefreshTitles(List<ListColumn<T>> listColumns, float width, float sumOfColRelWidths, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            var indexPointer = 0f;
            foreach (var child in _children)
            {
                var header = child as ListViewHeaderItem<T>;
                Debug.Assert(header != null);
                RemoveHeaderHandlers(header);
            }
            ClearChildren();
            foreach (ListColumn<T> c in listColumns)
            {
                var headerItem = new ListViewHeaderItem<T>(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
                headerItem.BorderWidth = 2;
                headerItem.Background = Colors.LightGray;
                headerItem.ButtonTextColor = Colors.Black;
                headerItem.ButtonText = c.Title;
                //headerItem.ButtonFontSize = 15;
                headerItem.Width = c.RelativeWidth / sumOfColRelWidths * width;
                headerItem.Height = Height;
                headerItem.Transform.LocalPosition = new Vector2(indexPointer, 0);
                AddHeaderHandlers(headerItem);
                this.AddChild(headerItem);
                indexPointer += headerItem.Width;
            }
        }

        /// <summary>
        /// This just repositions all the titles.
        /// </summary>
        public void RefreshTitles()
        {
            var indexPointer = 0f;
            foreach (var child in _children)
            {
                var headerItem = child as ButtonUIElement;
                headerItem.Transform.LocalPosition = new Vector2(indexPointer, 0);
                indexPointer += headerItem.Width;
            }
        }


        /// <summary>
        /// When the header is released, fire the dragcompleted event 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Header_DragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var header = item as ButtonUIElement;
            if (header != null)
            {
                var index = _children.IndexOf(header);
                Debug.Assert(index >= 0);
                HeaderDragCompleted?.Invoke(header, index, pointer);
            }
        }

        /// <summary>
        /// Returns the local position of the center of the specified column header.
        /// Used for dynamically rearranging column. Returns -1 if the index is less than 0 and 
        /// returns positive inifinity if column index is greater than the number of columns
        /// </summary>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        public float GetColumnHeaderCenter(int columnIndex)
        {
            if (columnIndex < 0)
            {
                return -1;
            }
            else if (columnIndex >= _children.Count)
            {
                return float.PositiveInfinity;
            }
            var column = _children[columnIndex] as ButtonUIElement;
            Debug.Assert(column != null);
            return column.Transform.LocalX + column.Width/2;
        }

        /// <summary>
        /// Swap Headers will move the header at headerToMoveIndex to where 
        /// the header at dragged index starts.
        /// </summary>
        /// <param name="headingBeingDraggedIndex"></param>
        /// <param name="headerToMove"></param>
        public void SwapHeaders(int draggedIndex, int headerToMoveIndex)
        {
            if (draggedIndex == headerToMoveIndex || draggedIndex < 0 || headerToMoveIndex < 0 ||
                draggedIndex >= _children.Count || headerToMoveIndex >= _children.Count)
            {
                Debug.WriteLine("Pass in proper indices for swapping column headers");
                return;
            }
            var headerToMove = _children[headerToMoveIndex] as ButtonUIElement;
            var headerBeingDragged = _children[draggedIndex] as ButtonUIElement;
            Debug.Assert(headerToMove != null && headerBeingDragged != null);

            //Swaps headers in children
            _children[headerToMoveIndex] = _children[draggedIndex];
            _children[draggedIndex] = headerToMove;

            //Moves x position of the header to move
            if (draggedIndex < headerToMoveIndex)
            {
                headerToMove.Transform.LocalX -= headerBeingDragged.Width;
            }
            else if (draggedIndex > headerToMoveIndex)
            {
                headerToMove.Transform.LocalX += headerBeingDragged.Width;
            }
        }

        /// <summary>
        /// When the header is being dragged, fire the dragged event
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Header_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //_headerBeingDragged = true;
            var header = item as ButtonUIElement;
            if (header != null)
            {
                var index = _children.IndexOf(header);
                Debug.Assert(index >= 0);
                if (index == 2)
                {
                    var x = 4;
                }
                HeaderDragged?.Invoke(header, index, pointer);
            }
        }

        /// <summary>
        /// Removes all the handlers that were previously attached to the header passed in.
        /// </summary>
        /// <param name="header"></param>
        public void RemoveHeaderHandlers(ListViewHeaderItem<T> header)
        {
            header.Tapped -= Header_Tapped;
            header.Dragged -= Header_Dragged;
            header.DragCompleted -= Header_DragCompleted;
            header.HeaderResizing -= HeaderItemResizing;
            header.HeaderResizeCompleted -= HeaderItemResizeCompleted;
        }
        
        /// <summary>
        /// Adds the necessary handlers to the header passed in
        /// </summary>
        /// <param name="header"></param>
        public void AddHeaderHandlers(ListViewHeaderItem<T> header)
        {
            header.Tapped += Header_Tapped;
            header.Dragged += Header_Dragged;
            header.DragCompleted += Header_DragCompleted;
            header.HeaderResizing += HeaderItemResizing;
            header.HeaderResizeCompleted += HeaderItemResizeCompleted;
        }

        /// <summary>
        /// This function is called when the headerItem edge has stopped being dragged. This function figures out which of the two headers for the 
        /// is the left header and fires the header resize completed event which the list view ui element container will be listening to
        /// </summary>
        /// <param name="header"></param>
        /// <param name="pointer"></param>
        /// <param name="edgeBeingDragged"></param>
        private void HeaderItemResizeCompleted(ListViewHeaderItem<T> header, CanvasPointer pointer, ListViewHeaderItem<T>.Edge edgeBeingDragged)
        {
            var col1Index = _children.IndexOf(header);
            Debug.Assert(col1Index != -1);
            if ((col1Index == 0 && edgeBeingDragged == ListViewHeaderItem<T>.Edge.Left) ||
                (col1Index == _children.Count - 1 && edgeBeingDragged == ListViewHeaderItem<T>.Edge.Right))
            {
                return;
            }

            int col2Index;
            if (edgeBeingDragged == ListViewHeaderItem<T>.Edge.Right)
            {
                col2Index = col1Index + 1;
                var col1Header = _children[col1Index] as ListViewHeaderItem<T>;
                Debug.Assert(col1Header != null);
                var col2Header = _children[col2Index] as ListViewHeaderItem<T>;
                Debug.Assert(col2Header != null);
                HeaderResizeCompleted?.Invoke(col1Header.Width, col2Header.Width, col1Index);
            }
            else
            {
                col2Index = col1Index - 1;
                var col1Header = _children[col1Index] as ListViewHeaderItem<T>;
                Debug.Assert(col1Header != null);
                var col2Header = _children[col2Index] as ListViewHeaderItem<T>;
                Debug.Assert(col2Header != null);
                HeaderResizeCompleted?.Invoke(col2Header.Width, col1Header.Width, col2Index);
            }


        }

        /// <summary>
        /// This function is called when the header item edge is currently being dragged. This adjusts the size of
        ///  the headers accordingly and fires the headerresizing event which the listViewUIElementContainer will be listening
        /// to so that it can adjust the size of the cells in the columns.
        /// </summary>
        /// <param name="header"></param>
        /// <param name="pointer"></param>
        /// <param name="edgeBeingDragged"></param>
        private void HeaderItemResizing(ListViewHeaderItem<T> header, CanvasPointer pointer, ListViewHeaderItem<T>.Edge edgeBeingDragged)
        {
            if (header == null)
            {
                return;
            }
            var index = _children.IndexOf(header);
            if (index < 0 || index > _children.Count - 1)
            {
                return;
            }
            if ((index == 0 && edgeBeingDragged == ListViewHeaderItem<T>.Edge.Left) ||
                (index == _children.Count - 1 && edgeBeingDragged == ListViewHeaderItem<T>.Edge.Right))
            {
                return;
            }
            var deltaX = pointer.DeltaSinceLastUpdate.X;
            Debug.Assert(index != -1);
            ListViewHeaderItem<T> left;
            ListViewHeaderItem<T> right;

            if (edgeBeingDragged == ListViewHeaderItem<T>.Edge.Right)
            {
                if (index + 1 < 0 || index + 1 > _children.Count - 1)
                {
                    return;
                }
                left = _children[index] as ListViewHeaderItem<T>;
                right = _children[index + 1] as ListViewHeaderItem<T>;
                Debug.Assert(left != null && right != null);
            }
            else
            {
                if (index - 1 < 0 || index - 1 > _children.Count - 1)
                {
                    return;
                }
                left = _children[index - 1] as ListViewHeaderItem<T>;
                right = _children[index] as ListViewHeaderItem<T>;
                Debug.Assert(left != null && right != null);
            }
            if (left.Width + deltaX < 10)
            {
                return;
            }
            if (right.Width - deltaX < 10)
            {
                return;
            }
            left.Width += deltaX;
            right.Width -= deltaX;
            right.Transform.LocalX += deltaX;
            HeaderResizing?.Invoke(index, pointer, edgeBeingDragged);


        }


        /// <summary>
        /// When a header is tapped this class will fire the header tapped event which the listview ui element container should be listening to
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Header_Tapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            HeaderTapped?.Invoke(_children.IndexOf(interactiveBaseRenderItem));   
        }
    }
}
