﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public class WindowResizerBorderRenderItem : InteractiveBaseRenderItem
    {
        /// <summary>
        ///  The canvas the WindowResizerBorderRenderItem is drawn on
        /// </summary>
        private CanvasAnimatedControl _canvas;

        /// <summary>
        /// The Size of the WindowResizerBorderRenderItem, contains
        /// variables Width and Height
        /// </summary>
        public Size Size;

        /// <summary>
        /// An enum describing the possible ResizerBorderPostions on a window
        /// </summary>
        public enum ResizerBorderPosition { Left, Right, Bottom, BottomRight, BottomLeft}

        /// <summary>
        /// The Width of the Resizer, treat this like a border width
        /// </summary>
        public float ResizerWidth;

        /// <summary>
        /// The position of the border on the the WindowBaseRenderItem
        /// </summary>
        private ResizerBorderPosition _position;

        /// <summary>
        /// The window this border is paired with
        /// </summary>
        private WindowBaseRenderItem _parentWindow;

        /// <summary>
        /// Fired anytime any of the resizers are dragged
        /// </summary>
        public event WindowResizerRenderItem.ResizerDraggedHandler ResizerDragged;

        /// <summary>
        /// The hit box will be at the smallest a _hitBoxErrorMargin x _hitBoxErrorMargin square
        /// If the actual width and height are greater than the _hitBoxErrorMargin the margin is not applied
        /// </summary>
        private float _hitBoxErrorMargin = 25;

        public WindowResizerBorderRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, ResizerBorderPosition position, WindowBaseRenderItem parentWindow, float resizerWidth) : base(parent, resourceCreator)
        {
            // set the canvas equal to the passed in resourceCreator
            _canvas = resourceCreator as CanvasAnimatedControl;
            Debug.Assert(_canvas != null, "The passed in canvas should be an CanvasAnimatedControl if not add support for other types here");

            // set the _position of the border in relation to the parent window
            _position = position;

            // set the parent window
            _parentWindow = parentWindow;

            // set the width correctly
            ResizerWidth = resizerWidth;
        }

        /// <summary>
        /// Fired whenever the parent window's size is changed
        /// </summary>
        /// <param name="size"></param>
        private void _parentWindow_SizeChanged(Size size)
        {
            PlaceResizerBorderRenderItem();
        }

        /// <summary>
        /// Places the resizer in the correct position. the _position variable must be set correcly
        /// </summary>
        public void PlaceResizerBorderRenderItem()
        {
            switch (_position)
            {
                case ResizerBorderPosition.Bottom:
                    Transform.LocalPosition = new Vector2(0, (float)_parentWindow.Size.Height - ResizerWidth);
                    Size.Height = ResizerWidth;
                    Size.Width = _parentWindow.Size.Width;
                    break;
                case ResizerBorderPosition.Left:
                    Transform.LocalPosition = new Vector2(0, 0);
                    Size.Height = _parentWindow.Size.Height;
                    Size.Width = ResizerWidth;
                    break;
                case ResizerBorderPosition.Right:
                    Transform.LocalPosition = new Vector2((float)_parentWindow.Size.Width - ResizerWidth, 0);
                    Size.Height = _parentWindow.Size.Height;
                    Size.Width = ResizerWidth;
                    break;
                case ResizerBorderPosition.BottomRight:
                    Transform.LocalPosition = new Vector2((float)_parentWindow.Size.Width - ResizerWidth, (float)_parentWindow.Size.Height - ResizerWidth);
                    Size.Height = ResizerWidth;
                    Size.Width = ResizerWidth;
                    break;
                case ResizerBorderPosition.BottomLeft:
                    Transform.LocalPosition = new Vector2(0, (float)_parentWindow.Size.Height - ResizerWidth);
                    Size.Height = ResizerWidth;
                    Size.Width = ResizerWidth;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_position), _position, null);
            }
        }

        /// <summary>
        /// Dispose of any event handlers here and take care of clean exit
        /// </summary>
        public override void Dispose()
        {
            // remove event handlers
            _parentWindow.SizeChanged -= _parentWindow_SizeChanged;
            Dragged -= WindowResizerBorderRenderItem_Dragged;

            // call base.Dispose to continue disposing items down the stack
            base.Dispose();
        }

        public override async Task Load()
        {

            // attach an event which is fired whenever the parent window size is changed
            _parentWindow.SizeChanged += _parentWindow_SizeChanged;

            // add the manipulation mode methods
            Dragged += WindowResizerBorderRenderItem_Dragged;

            // Place the border in the correct place
            PlaceResizerBorderRenderItem();

            base.Load();
        }

        /// <summary>
        /// Draws the window onto the screen with the offset of the Local
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.FillRectangle(new Rect(0, 0, Size.Width, Size.Height), Colors.Green);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        /// <summary>
        /// Fired whenever a border is dragged
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void WindowResizerBorderRenderItem_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            Vector2 sizeDelta = new Vector2();
            Vector2 offsetDelta = new Vector2();

            switch (_position)
            {
                // in this case we are changing the size and the offset. Size decreases by drag x amount, offset increases
                // by drag x amount
                case ResizerBorderPosition.Left:
                    sizeDelta.X -= pointer.DeltaSinceLastUpdate.X;
                    offsetDelta.X += pointer.DeltaSinceLastUpdate.X;
                    break;
                // in this case we are changing the size only. Size increases by the drag x amount
                case ResizerBorderPosition.Right:
                    sizeDelta.X += pointer.DeltaSinceLastUpdate.X;
                    break;
                // in this case we are changing the size only. Size increases by the drag y amount
                case ResizerBorderPosition.Bottom:
                    sizeDelta.Y += pointer.DeltaSinceLastUpdate.Y;
                    break;
                case ResizerBorderPosition.BottomRight:
                    sizeDelta.Y += pointer.DeltaSinceLastUpdate.Y;
                    sizeDelta.X += pointer.DeltaSinceLastUpdate.X;
                    break;
                case ResizerBorderPosition.BottomLeft:
                    sizeDelta.X -= pointer.DeltaSinceLastUpdate.X;
                    offsetDelta.X += pointer.DeltaSinceLastUpdate.X;
                    sizeDelta.Y += pointer.DeltaSinceLastUpdate.Y;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // invoke the event so the drag is fired up the chain to the main window
            ResizerDragged?.Invoke(sizeDelta, offsetDelta);
        }

        /// <summary>
        /// Returns the LocalBounds of the base render item, used for hit testing. The bounds are given with the offset
        /// of the local matrix assumed to be zero. If the matrix is offset, then the local bounds must be offset accordingly
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            // get equal sized margins on the left and right. The hitbox is at least as wide and as tall as the square created
            // with sides of size _hitboxErrorMargin
            double horizontalMargin = _hitBoxErrorMargin - Size.Width/2 > 0 ? _hitBoxErrorMargin - Size.Width/2 : 0;
            double verticalMargin = _hitBoxErrorMargin - Size.Height / 2 > 0 ? _hitBoxErrorMargin - Size.Height / 2 : 0;

            return new Rect(-horizontalMargin, -verticalMargin, Size.Width + horizontalMargin * 2, Size.Height + verticalMargin * 2);
        }
    }
}
