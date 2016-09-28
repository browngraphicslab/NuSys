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
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    class WindowTopBarRenderItem : InteractiveBaseRenderItem
    {
        /// <summary>
        /// The size of the top bar contains variables width and height
        /// </summary>
        public Size Size;
        
        /// <summary>
        /// Delegate used whenever the WinodwTopBarRenderItem is dragged
        /// </summary>
        /// <param name="offsetDelta">The amount the WindowBaseRenderItem local transform should be offset</param>
        /// <param name="pointer">The pointer which is performing the drag</param>
        public delegate void WindowTopBarDraggedHandler(Vector2 offsetDelta, CanvasPointer pointer);

        /// <summary>
        /// Invoked every time the WindowTopBarRenderItem is dragged
        /// </summary>
        public event WindowTopBarDraggedHandler TopBarDragged;

        /// <summary>
        /// Delegate used when a pointer is released from the WindowTopBarRenderItem
        /// </summary>
        /// <param name="pointer">The pointer that was released from the WindowTopBarRenderItem</param>
        public delegate void WindowTopBarReleasedHandler(CanvasPointer pointer);

        /// <summary>
        /// Invoked every time the WindowTopbarRenderItem is clicked on then released
        /// </summary>
        public event WindowTopBarReleasedHandler TopBarReleased;

        /// <summary>
        ///  The canvas the WindowTopBarRenderItem is drawn on
        /// </summary>
        private CanvasAnimatedControl _canvas;

        /// <summary>
        /// The window this border is paired with
        /// </summary>
        private WindowBaseRenderItem _parentWindow;

        /// <summary>
        /// The default height of the TopBar
        /// </summary>
        public float TopBarHeight = 100; //todo add code so this can be dynamically changed

        public WindowTopBarRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the canvas equal to the passed in resourceCreator
            _canvas = resourceCreator as CanvasAnimatedControl;
            Debug.Assert(_canvas != null, "The passed in canvas should be an CanvasAnimatedControl if not add support for other types here");

            _parentWindow = parent as WindowBaseRenderItem;
            Debug.Assert(_parentWindow != null, "If the parent calling this is not the WindowBaseRenderItem then make sure to pass the" +
                                                "WindowBaseRenderItem as a variable down the stack");
        }

        /// <summary>
        /// An initializer method
        /// </summary>
        public override async Task Load() //todo find out exactly when this is called
        {
            // Add the manipulation events
            Dragged += WindowTopBarRenderItem_Dragged;
            Released += WindowTopBarRenderItem_Released;

            // add event listeners
            _parentWindow.SizeChanged += parentWindow_SizeChanged;

            // set the size
            Size.Height = TopBarHeight; // height is generic
            Size.Width = _parentWindow.Size.Width; // width is same as base window

            base.Load();
        }

        /// <summary>
        /// Fired whenever the pointer is release on the WindowTopBarRenderItem
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void WindowTopBarRenderItem_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            TopBarReleased?.Invoke(pointer);
        }

        /// <summary>
        /// Dispose of any event handlers here and take care of clean exit
        /// </summary>
        public override void Dispose()
        {
            // remove event handlers
            Dragged -= WindowTopBarRenderItem_Dragged;
            _parentWindow.SizeChanged -= parentWindow_SizeChanged;

            // call base.Dispose to continue disposing items down the stack
            base.Dispose();
        }

        /// <summary>
        /// Called whenever the WindowBaseRenderItem this top bar is paired with changes size
        /// </summary>
        /// <param name="size"></param>
        private void parentWindow_SizeChanged(Size size)
        {
            Size.Width = size.Width;
        }

        /// <summary>
        /// Fired whenever the WindowTopBarRenderItem is dragged
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void WindowTopBarRenderItem_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // Inform the objects up the stack that the WindowTopBarRenderItem was dragged
            TopBarDragged?.Invoke(pointer.DeltaSinceLastUpdate, pointer);
        }


        /// <summary>
        /// Draws the window onto the screen with the offset of the Local transform
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
                return;

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            ds.FillRectangle(new Rect(0, 0, Size.Width, Size.Height), Colors.Blue);

            ds.Transform = orgTransform;

            base.Draw(ds);
        }

        /// <summary>
        /// Returns the LocalBounds of the base render item, used for hit testing. The bounds are given with the offset
        /// of the local matrix assumed to be zero. If the matrix is offset, then the local bounds must be offset accordingly
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return new Rect(0, 0, Size.Width, Size.Height);
        }
    }
}
