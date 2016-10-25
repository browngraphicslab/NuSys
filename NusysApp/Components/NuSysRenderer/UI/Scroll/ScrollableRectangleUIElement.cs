using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using System.Numerics;

namespace NuSysApp
{
    /// <summary>
    /// RectangleUIElement with a scroll bar on the right-hand side
    /// </summary>
    public class ScrollableRectangleUIElement : RectangleUIElement
    {
        private BaseRenderItem _parent;
        private ICanvasResourceCreatorWithDpi _resourceCreator;
        private ScrollBarUIElement _scrollBar;

        public ScrollBarUIElement ScrollBar
        {
            get { return _scrollBar; }
        }


        public ScrollableRectangleUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _parent = parent;
            _resourceCreator = resourceCreator;

            PointerWheelChanged += ScrollableRectanglePointerWheelChanged;
            
        }

        public virtual void ScrollableRectanglePointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {

        }

        /// <summary>
        /// Creates the scrollbar and adds it as a child of the RectangleUIElement
        /// </summary>
        public void MakeScrollBar()
        {
            var scrollbarwidth = 10;

            _scrollBar = new ScrollBarUIElement(_parent, _resourceCreator, ScrollBarUIElement.Orientation.Vertical)
            {
                Background = Colors.LightGray,
                Height = this.Height,
                Width = scrollbarwidth,
                ScrollBarColor = Colors.Gray,
            };

            _scrollBar.ScrollBarPositionChanged += ScrollBarPositionChanged;
            AddChild(_scrollBar);
        }

        public virtual void ScrollBarPositionChanged(object source, double position)
        {

        }

        public override Task Load()
        {
            MakeScrollBar();
            return base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            _scrollBar.Transform.LocalPosition = new Vector2((float)Width, 0);

            base.Draw(ds);
        }

        public override void Dispose()
        {
            _scrollBar.ScrollBarPositionChanged -= ScrollBarPositionChanged;
            PointerWheelChanged -= ScrollableRectanglePointerWheelChanged;
            base.Dispose();
        }

    }
}