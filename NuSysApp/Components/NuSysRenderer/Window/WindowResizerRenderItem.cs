using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    class WindowResizerRenderItem : BaseRenderItem
    {

        /// <summary>
        ///  The canvas the WindowTopBarRenderItem is drawn on
        /// </summary>
        private CanvasAnimatedControl _canvas;

        /// <summary>
        /// The resizer at the bottom of the window
        /// </summary>
        private WindowResizerBorderRenderItem _bottomResizer;

        /// <summary>
        /// The resizer at the left of the window
        /// </summary>
        private WindowResizerBorderRenderItem _leftResizer;

        /// <summary>
        /// The resizer at the top of the window
        /// </summary>
        private WindowResizerBorderRenderItem _rightResizer;

        /// <summary>
        /// The window this resizer is paired with
        /// </summary>
        private WindowBaseRenderItem _parentWindow;

        /// <summary>
        /// Delegate that is passed the change in size, and the change in offset given
        /// that a resizer was dragged. To use add sizeDelta to the size of the window
        /// and add offsetDelta to the local transform of the window
        /// </summary>
        /// <param name="size"></param>
        public delegate void ResizerDraggedHandler(Vector2 sizeDelta, Vector2 offsetDelta);

        /// <summary>
        /// Fired anytime any of the resizers are dragged
        /// </summary>
        public event ResizerDraggedHandler ResizerDragged;

        public WindowResizerRenderItem(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            // set the canvas equal to the passed in resourceCreator
            _canvas = resourceCreator as CanvasAnimatedControl;
            Debug.Assert(_canvas != null, "The passed in canvas should be an CanvasAnimatedControl if not add support for other types here");

            // set the _parentWindow to the passed in parent
            _parentWindow = parent as WindowBaseRenderItem;
            Debug.Assert(_parentWindow != null, "The passed in parent should be an WindowBaseRenderItem if not add support for other types here");
        }

        /// <summary>
        /// An initializer method
        /// </summary>
        public override async Task Load() //todo find out exactly when this is called
        {
            // create the _leftResizer and add it as a child
            _leftResizer = new WindowResizerBorderRenderItem(this, _canvas, WindowResizerBorderRenderItem.ResizerBorderPosition.Left, _parentWindow);
            _leftResizer.ResizerDragged += WindowResizerBorderRenderItem_OnResizerDragged;
            AddChild(_leftResizer);

            // create the _rightResizer and add it as a child
            _rightResizer = new WindowResizerBorderRenderItem(this, _canvas, WindowResizerBorderRenderItem.ResizerBorderPosition.Right, _parentWindow);
            _rightResizer.ResizerDragged += WindowResizerBorderRenderItem_OnResizerDragged;
            AddChild(_rightResizer);

            // create the _bottomResizer and add it as a child
            _bottomResizer = new WindowResizerBorderRenderItem(this, _canvas, WindowResizerBorderRenderItem.ResizerBorderPosition.Bottom, _parentWindow);
            _bottomResizer.ResizerDragged += WindowResizerBorderRenderItem_OnResizerDragged;
            AddChild(_bottomResizer);

            base.Load();
        }

        /// <summary>
        /// Dispose of any event handlers here and take care of clean exit
        /// </summary>
        public override void Dispose()
        {
            // remove event handlers
            _leftResizer.ResizerDragged -= WindowResizerBorderRenderItem_OnResizerDragged;
            _rightResizer.ResizerDragged -= WindowResizerBorderRenderItem_OnResizerDragged;
            _bottomResizer.ResizerDragged -= WindowResizerBorderRenderItem_OnResizerDragged;

            // call base.Dispose to continue disposing items down the stack
            base.Dispose();
        }

        /// <summary>
        /// Called whenever one of the borders is dragged
        /// </summary>
        /// <param name="sizedelta"></param>
        /// <param name="offsetdelta"></param>
        private void WindowResizerBorderRenderItem_OnResizerDragged(Vector2 sizedelta, Vector2 offsetdelta)
        {
            ResizerDragged?.Invoke(sizedelta, offsetdelta);
        }
    }
}
