using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace NuSysApp
{
    public abstract class BaseInteractiveUIElement : InteractiveBaseRenderItem
    {
        protected BaseInteractiveUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Canvas = resourceCreator as CanvasAnimatedControl;
            Debug.Assert(Canvas != null, $"The passed in resourceCreator is a {nameof(resourceCreator)} not a CanvasAnimatedControl.");
        }

        /// <summary>
        /// The AnimatedCanvasControl the UI element is placed onto.
        /// </summary>
        protected CanvasAnimatedControl Canvas;

        /// <summary>
        /// The Width of the UI Element
        /// </summary>
        public abstract float Width { get; set; }

        /// <summary>
        /// The Height of the UI Element
        /// </summary>
        public abstract float Height { get; set; }

        /// <summary>
        /// The Background Color of the UI Element
        /// </summary>
        public abstract Color Background { get; set; }

        /// <summary>
        /// The width of the border of the UI Element
        /// </summary>
        public abstract float BorderWidth { get; set; }

        /// <summary>
        /// The color of the Border of the UI Element;
        /// </summary>
        public abstract Color Bordercolor { get; set; }

        /// <summary>
        /// Draws the Border around the UI Element
        /// </summary>
        /// <param name="ds"></param>
        protected abstract void DrawBorder(CanvasDrawingSession ds);

        /// <summary>
        /// The InitialOffset of the UIElement from the parent's upper left corner.
        /// Offsets from the top left of the screen if the parent is null.
        /// </summary>
        public abstract Vector2 InitialOffset { get; set; }

        public override async Task Load()
        {
            Transform.LocalPosition += InitialOffset;
            base.Load();
        }

    }
}
