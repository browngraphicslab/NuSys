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
                headerItem.ButtonFontSize = 15;
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
        /// Adds the necessary handlers to the header passed in
        /// </summary>
        /// <param name="header"></param>
        public void AddHeaderHandlers(ListViewHeaderItem<T> header)
        {
            header.Tapped += Header_Tapped;
            header.Dragging += Header_Dragged;
            header.DragCompleted += Header_DragCompleted;
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
        }

        /// <summary>
        /// When a header is tapped this class will fire the header tapped event which the listview ui element container should be listening to
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Header_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            HeaderTapped?.Invoke(_children.IndexOf(item));   
        }
    }
}
