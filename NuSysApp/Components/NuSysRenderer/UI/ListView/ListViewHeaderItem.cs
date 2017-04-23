﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// a single element in the header corresponding to a certain attribute. this will be the handle used to sort, add/remove, and drag
    /// </summary>
    public class ListViewHeaderItem<T> : ButtonUIElement
    {
        public enum Edge
        {
            Right,
            Left
        };

        /// <summary>
        /// This is the edge that is being dragged. (either left or right). This is used by the listviewheader to see which other column should be resized. 
        /// (E.g if left edge is being dragged, the header before is also resized)
        /// </summary>
        private Edge _edgeBeingDragged;

        public delegate void ResizeHeaderEventHandler(ListViewHeaderItem<T> header, Vector2 translation, Edge edgeBeingDragged);

        /// <summary>
        /// When left or right borders are dragged, this event will fire
        /// </summary>
        public event ResizeHeaderEventHandler HeaderResizing;

        public delegate void ResizeHeaderCompletedEventHandler(ListViewHeaderItem<T> header, Edge edgeBeingDragged);

        /// <summary>
        /// Once the dragging of the border has completed (pointer has been released) this event will fire.
        /// </summary>
        public event ResizeHeaderCompletedEventHandler HeaderResizeCompleted;


        public delegate void HeaderOptionsActivatedEventHandler(ListViewHeaderItem<T> header);
        public event HeaderOptionsActivatedEventHandler HeaderOptionsActivated;

        /// <summary>
        /// The boolean for the border being dragged so we can fire drag completed events when you release the pointer.
        /// </summary>
        public bool _borderBeingDragged;
        /// <summary>
        /// list of the row elements that it will need to access
        /// </summary>
        private List<ListViewRowUIElement<T>> _rowElements;

        /// <summary>
        /// accessor for rowelement list
        /// </summary>
        public List<ListViewRowUIElement<T>> RowElements
        {
            get { return _rowElements; }
            set {
                if (value != null)
                {
                    _rowElements = value;
                }
            }
        }

        /// <summary>
        /// column that this headeritem corresponds to
        /// </summary>
        private ListColumn<T> _column;

        private FlyoutPopupGroup _popupGroup;

        /// <summary>
        /// accessor for column that headeritem corresponds to
        /// </summary>
        public ListColumn<T> Column
        {
            get { return _column; }
            set
            {
                if (value != null)
                {
                    _column = value;
                }
            }
        }

        /// <summary>
        /// shape element that is passed in should always be a rectangle
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="shapeElement"></param>
        public ListViewHeaderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BaseInteractiveUIElement shapeElement) : base(parent, resourceCreator, shapeElement)
        {
            _borderBeingDragged = false;
            ButtonTextColor = Constants.ALMOST_BLACK;
            ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;

            Holding += ListViewHeaderItem_Holding;

            var tapRecognizer = new TapGestureRecognizer();
            tapRecognizer.OnTapped += TapRecognizer_OnTapped;
            GestureRecognizers.Add(tapRecognizer);

            var dragRecognizer = new DragGestureRecognizer();
            dragRecognizer.OnDragged += DragRecognizer_OnDragged;
            GestureRecognizers.Add(dragRecognizer);
        }

        private void DragRecognizer_OnDragged(DragGestureRecognizer sender, DragEventArgs args)
        {
            if (args.CurrentState == GestureEventArgs.GestureState.Began)
            {
                var startX = Vector2.Transform(args.CurrentPoint, Transform.ScreenToLocalMatrix).X;

                if (_borderBeingDragged == false)
                {
                    if (startX < BorderWidth)
                    {
                        _edgeBeingDragged = Edge.Left;
                        _borderBeingDragged = true;

                    }
                    else if (startX > Width - BorderWidth)
                    {
                        _edgeBeingDragged = Edge.Right;
                        _borderBeingDragged = true;
                    }
                }
            } else if ( args.CurrentState == GestureEventArgs.GestureState.Ended)
            {
                if (_borderBeingDragged)
                {
                    _borderBeingDragged = false;
                    Debug.Assert(_edgeBeingDragged != null);
                    HeaderResizeCompleted?.Invoke(this, _edgeBeingDragged);
                    return;
                }
            }
        }

        private void TapRecognizer_OnTapped(TapGestureRecognizer sender, TapEventArgs args)
        {
            if (args.TapType == TapEventArgs.Tap.RightTap)
            {
                HeaderOptionsActivated?.Invoke(this);
            }
        }

        private void ListViewHeaderItem_Holding(InteractiveBaseRenderItem item, Vector2 point)
        {
            HeaderOptionsActivated?.Invoke(this);
        }

        public override void Dispose()
        {
            base.Dispose();

            Holding -= ListViewHeaderItem_Holding;
        }

        protected override CanvasTextFormat GetCanvasTextFormat()
        {
            // create a text format object
            var textFormat = new CanvasTextFormat
            {
                HorizontalAlignment = ButtonTextHorizontalAlignment,
                VerticalAlignment = ButtonTextVerticalAlignment,
                WordWrapping = CanvasWordWrapping.NoWrap,
                TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
                TrimmingSign = CanvasTrimmingSign.Ellipsis,
                FontSize = 18,
                FontFamily = UIDefaults.TextFont
            };

            return textFormat;
        }
    }
}
