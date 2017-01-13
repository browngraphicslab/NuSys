using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class WindowUIElement : RectangleUIElement
    {
        /// <summary>
        /// The height of the TopBar.
        /// </summary>
        private float _topBarHeight;

        /// <summary>
        /// The height of the TopBar. Must be a value greater than or equal to zero.
        /// </summary>
        public float TopBarHeight
        {
            get { return _topBarHeight; }
            set
            {
                Debug.Assert(value >= 0);
                _topBarHeight = value >= 0 ? value : 0;
                if (_topBar != null)
                {
                    _topBar.Height = _topBarHeight;
                }
            }
        }

        /// <summary>
        /// Helper variable for the TopBarColor property
        /// </summary>
        private Color _topBarColor;

        /// <summary>
        /// The Color of the TopBar
        /// </summary>
        public Color TopBarColor
        {
            get { return _topBarColor; }
            set
            {
                _topBarColor = value;
                if (_topBar != null)
                {
                    _topBar.Background = value;
                }
            }
        }

        /// <summary>
        /// The top bar rectangle
        /// </summary>
        private RectangleUIElement _topBar;

        /// <summary>
        /// private helper variable for public property error margin
        /// </summary>
        private float _errorMargin { get; set; }

        /// <summary>
        /// A margin of error extending beyond the width and height of the window
        /// to provide a bufferzone for touch events. Any touch in the ErrorMargin
        /// pixels extending beyond the height and width of the UI Element will
        /// trigger a touch event. The error margin is 0 when the window doesn't
        /// have focus
        /// </summary>
        public float ErrorMargin
        {
            get { return HasFocus || ChildHasFocus ? _errorMargin : 0; }
            set { _errorMargin = value; }
        }

        /// <summary>
        /// The Buttons on the right side of the top bar
        /// </summary>
        private List<ButtonUIElement> _topBarRightButtons;

        private StackLayoutManager _rightButtonLayoutManager;

        /// <summary>
        /// The width of buttons on the top bar, added using AddButton
        /// </summary>
        public float TopBarButtonWidth => TopBarHeight;

        /// <summary>
        /// The height of buttons on the top bar, added using AddButton
        /// </summary>
        public float TopBarButtonHeight => TopBarHeight;


        private float _buttonSpacing = 1;

        /// <summary>
        /// Event fired whenever the top bar is dragged
        /// </summary>
        public event PointerHandler TopBarDragged;

        /// <summary>
        /// Event fired when the top bar drag is started
        /// </summary>
        public event PointerHandler TopBarDragStarted;

        /// <summary>
        /// Event fired when the top bar drag is completed
        /// </summary>
        public event PointerHandler TopBarDragCompleted;

        public enum TopBarPosition
        {
            Right
        }

        public WindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            TopBarColor = UIDefaults.TopBarColor;
            TopBarHeight = UIDefaults.TopBarHeight;
            ErrorMargin = UIDefaults.ErrorMargin;
            BorderWidth = UIDefaults.WindowBorderWidth;
            
            // initialize the top bar rectangle
            _topBar = new RectangleUIElement(this, Canvas)
            {
                Background = TopBarColor,
                Height = TopBarHeight
            };
            _topBar.BorderColor = BorderColor;
            _topBar.BorderWidth = BorderWidth;
            AddChild(_topBar);

            _topBar.Dragged += InvokeTopBarDragged;
            _topBar.DragStarted += InvokeTopBarDragStarted;
            _topBar.DragCompleted += InvokeTopBarDragCompleted;

            _topBarRightButtons = new List<ButtonUIElement>();
            _rightButtonLayoutManager = new StackLayoutManager
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Spacing = _buttonSpacing
            };
        }



        public override void Dispose()
        {
            _topBar.DragStarted -= InvokeTopBarDragStarted;
            _topBar.Dragged -= InvokeTopBarDragged;
            _topBar.DragCompleted -= InvokeTopBarDragCompleted;


            base.Dispose();
        }

        private void InvokeTopBarDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            TopBarDragCompleted?.Invoke(item, pointer);
        }

        private void InvokeTopBarDragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            TopBarDragStarted?.Invoke(item, pointer);
        }

        /// <summary>
        /// Invokes the on top bar dragged event
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void InvokeTopBarDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            TopBarDragged?.Invoke(item, pointer);
        }

        /// <summary>
        /// Adds the passed in button to the top bar at the passed in button position, also adds the button as a child so do not
        /// add the button as a child elsewhere. The button's size is automatically set
        /// </summary>
        /// <param name="button"></param>
        /// <param name="position"></param>
        public void AddButton(ButtonUIElement button, TopBarPosition position)
        {
            switch (position)
            {
                case TopBarPosition.Right:
                    _topBarRightButtons.Add(button);
                    _rightButtonLayoutManager.AddElement(button);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            _topBar.AddChild(button);
        }

        /// <summary>
        /// Removes the passed in button from the top bar, throws an exception if given the wrong position for the button,
        /// does not handle disposing of the button
        /// </summary>
        /// <param name="button"></param>
        /// <param name="position"></param>
        public void RemoveButton(ButtonUIElement button, TopBarPosition position)
        {
            switch (position)
            {
                case TopBarPosition.Right:
                    _topBarRightButtons.Remove(button);
                    _rightButtonLayoutManager.Remove(button);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(position), position, null);
            }

            _topBar.RemoveChild(button);
        }

        /// <summary>
        /// Draws the window onto the canvas drawing session
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            base.Draw(ds);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _topBar.Width = Width;

            _rightButtonLayoutManager.SetSize(_topBar.Width, _topBar.Height);
            _rightButtonLayoutManager.ItemWidth = TopBarButtonWidth;
            _rightButtonLayoutManager.ItemHeight = TopBarButtonHeight;
            _rightButtonLayoutManager.ArrangeItems();

            _topBar.BorderColor = BorderColor;
            _topBar.BorderWidth = BorderWidth;

            base.Update(parentLocalToScreenTransform);
        }
    }
}
