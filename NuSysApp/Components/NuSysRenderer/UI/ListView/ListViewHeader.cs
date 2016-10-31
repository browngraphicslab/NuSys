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
        /// Adds the necessary handlers to the header passed in
        /// </summary>
        /// <param name="header"></param>
        public void AddHeaderHandlers(ListViewHeaderItem<T> header)
        {
            header.Tapped += Header_Tapped;
            header.Dragged += Header_Dragged;
            header.Released += Header_Released;
        }

        /// <summary>
        /// When the header is released, fire the dragcompleted event 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Header_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_headerBeingDragged)
            {
                var header = item as ButtonUIElement;
                if (header != null)
                {
                    var index = _children.IndexOf(header);
                    Debug.Assert(index >= 0);
                    HeaderDragCompleted?.Invoke(header, index, pointer);
                }
            }
        }

        /// <summary>
        /// When the header is being dragged, fire the dragged event
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Header_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _headerBeingDragged = true;
            var header = item as ButtonUIElement;
            if (header != null)
            {
                var index = _children.IndexOf(header);
                Debug.Assert(index >= 0);
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
