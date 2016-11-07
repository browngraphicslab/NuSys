using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class AudioRegionResizeUIElement : InteractiveBaseRenderItem
    {
        /// <summary>
        /// The top ball used to resize the audio region
        /// </summary>
        private EllipseUIElement _topHandle;

        /// <summary>
        /// The bottom ball used to resize the audio region
        /// </summary>
        private EllipseUIElement _botHandle;

        /// <summary>
        /// The line connecting the botHandle to the top handle
        /// </summary>
        private RectangleUIElement _connectingLine;

        /// <summary>
        /// The height of the resizer
        /// </summary>
        public float Height;

        public AudioRegionResizeUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // initialize all the geometries
            _connectingLine = new RectangleUIElement(this, resourceCreator);
            AddChild(_connectingLine);  // add this child before handles so it is underneath
            InitializeConnectingLineUI(_connectingLine);
            _topHandle = new EllipseUIElement(this, resourceCreator);
            InitializeHandleUI(_topHandle);
            AddChild(_topHandle);
            _botHandle = new EllipseUIElement(this, resourceCreator);
            InitializeHandleUI(_botHandle);
            AddChild(_botHandle);

            // add dragging event handlers
            _topHandle.Dragged += HandleDragged;
            _botHandle.Dragged += HandleDragged;

        }

        /// <summary>
        /// Remove event handlers
        /// </summary>
        public override void Dispose()
        {

            _topHandle.Dragged -= HandleDragged;
            _botHandle.Dragged -= HandleDragged;

            base.Dispose();
        }

        /// <summary>
        /// Whenever a handle is dragged invoke the dragged event for this interactive base render item
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void HandleDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnDragged(pointer);
        }

        /// <summary>
        /// Initialize the connecting line ui
        /// </summary>
        /// <param name="connectingLine"></param>
        private void InitializeConnectingLineUI(RectangleUIElement connectingLine)
        {
            connectingLine.Width = UIDefaults.AudioResizerConnectingLineWidth;
            connectingLine.Background = UIDefaults.AudioResizerHandleColor;

        }

        /// <summary>
        /// Initialize the handle ui
        /// </summary>
        /// <param name="handle"></param>
        private void InitializeHandleUI(EllipseUIElement handle)
        {
            handle.Width = UIDefaults.AudioResizerHandleDiameter;
            handle.Height = UIDefaults.AudioResizerHandleDiameter;
            handle.Background = UIDefaults.AudioResizerHandleColor;
        }

        /// <summary>
        /// Called before draw, sets locations of resizers and dimensions
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // handle radius, for shifting ellipses so they are centered on the element.
            var handleRadius = UIDefaults.AudioResizerHandleDiameter/2;
            _botHandle.Transform.LocalPosition = new Vector2(-handleRadius, Height - handleRadius);
            _topHandle.Transform.LocalPosition = new Vector2(-handleRadius, -handleRadius);
            _connectingLine.Height = Height;
            // move the connecting line by half its width so it is centered on the element
            _connectingLine.Transform.LocalPosition = new Vector2(-UIDefaults.AudioResizerConnectingLineWidth/2, 0);
            base.Update(parentLocalToScreenTransform);
        }
    }
}
