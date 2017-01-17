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

        public delegate void ResizeHeaderEventHandler(ListViewHeaderItem<T> header, CanvasPointer pointer, Edge edgeBeingDragged);

        /// <summary>
        /// When left or right borders are dragged, this event will fire
        /// </summary>
        public event ResizeHeaderEventHandler HeaderResizing;

        public delegate void ResizeHeaderCompletedEventHandler(ListViewHeaderItem<T> header, CanvasPointer pointer, Edge edgeBeingDragged);

        /// <summary>
        /// Once the dragging of the border has completed (pointer has been released) this event will fire.
        /// </summary>
        public event ResizeHeaderCompletedEventHandler HeaderResizeCompleted;


        public delegate void DeleteColumnTappedEventHandler(ListViewHeaderItem<T> header, FlyoutPopupGroup group, FlyoutPopup popup, ButtonUIElement flyoutItem, CanvasPointer pointer);
        public event DeleteColumnTappedEventHandler DeleteColumnTapped;

        public delegate void AddColumnTappedEventHandler(ListViewHeaderItem<T> header, FlyoutPopupGroup group, FlyoutPopup popup, ButtonUIElement flyoutItem, CanvasPointer pointer);
        public event AddColumnTappedEventHandler AddColumnTapped;

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

            Tapped += ListViewHeaderItem_Tapped;
            Holding += ListViewHeaderItem_Holding;
        }

        private void ListViewHeaderItem_Holding(InteractiveBaseRenderItem item, Vector2 point)
        {
            AddMainFlyout();
        }


        private void ListViewHeaderItem_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (pointer.IsRightButtonPressed)
            {
                AddMainFlyout();
            }
        }

        private void AddMainFlyout()
        {
            _popupGroup = new FlyoutPopupGroup(this, Canvas);
            var addDeleteColumns = _popupGroup.AddHeadFlyoutPopup();
            addDeleteColumns.AddFlyoutItem("add column", AddColumn, Canvas);
            addDeleteColumns.AddFlyoutItem("delete", DeleteColumn, Canvas);
            _popupGroup.Transform.LocalPosition = new Vector2(0, Height);
            AddChild(_popupGroup);
        }

        private void AddColumn(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            AddColumnTapped?.Invoke(this, _popupGroup, item.Parent as FlyoutPopup, item as ButtonUIElement, pointer);
        }

        private void DeleteColumn(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
           DeleteColumnTapped?.Invoke(this, _popupGroup, item.Parent as FlyoutPopup, item as ButtonUIElement, pointer);
        }

        public override void OnDragStarted(CanvasPointer pointer)
        {
            var startX = Vector2.Transform(pointer.StartPoint, Transform.ScreenToLocalMatrix).X;

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
            base.OnDragStarted(pointer);
        }

        /// <summary>
        /// This overrides the dragged event of the button so that if you are dragging the edge, the headerResizing event is invoked instead of the regular dragging event.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        public override void OnDragged(CanvasPointer pointer)
        {
            if (_borderBeingDragged)
            {
                Debug.Assert(_edgeBeingDragged != null);
                HeaderResizing?.Invoke(this, pointer, _edgeBeingDragged);
            }
            else
            {
                base.OnDragged(pointer);

            }

        }

        /// <summary>
        /// This overrides the released handler of the button. If you were dragging the edge of a button, the headerresizeCompleted is invoked instead. 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        public override void OnReleased(CanvasPointer pointer)
        {
            if (_borderBeingDragged)
            {
                _borderBeingDragged = false;
                Debug.Assert(_edgeBeingDragged != null);
                HeaderResizeCompleted?.Invoke(this, pointer, _edgeBeingDragged);
                return;
            }

            base.OnReleased(pointer);
        }

        /// <summary>
        /// Overrides the pressed handler of the button. iIt does nothing if the edge of the button is pressed, and calls the base handler otherwise.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        public override void OnPressed(CanvasPointer pointer)
        {
            var currentX = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix).X;

            if (currentX > BorderWidth && currentX < Width - BorderWidth)
            {
                base.OnPressed(pointer);
                return;
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            Tapped -= ListViewHeaderItem_Tapped;
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
