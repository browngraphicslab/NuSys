using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;

namespace NuSysApp
{
    public class ScrollBarUIElement : RectangleUIElement
    {
        #region private members
        /// <summary>
        /// Position of the (start of the) slider bar
        /// </summary>
        private double _position;
        /// <summary>
        /// Normalized length of the slider. The end - start.
        /// </summary>
        private double _range;
        /// <summary>
        /// Orientation can be either vertical or horizontal
        /// </summary>
        private Orientation _orientation;
        /// <summary>
        /// Velocity with which the list view scrolls to the place.
        /// </summary>
        private static double _scrollVelocity;
        /// <summary>
        /// Bool for whether you are currently dragging the slider bar.
        /// </summary>
        private bool _isdragging;

        #endregion private members
        #region public members
        /// <summary>
        /// TODO: Support horizontal scroll bars.
        /// </summary>
        public enum Orientation { Horizontal, Vertical }

        /// <summary>
        /// Color of the scroll bar. 
        /// </summary>
        public Color ScrollBarColor = Colors.Gray;

        /// <summary>
        /// Invoked when the user slides the scroll bar to a new position. Position is
        /// double from 0 to 1. It repreesents the "start" of the scroll bar.
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        public delegate void ScrollBarPositionChangedHandler(object source, double position);
        public event ScrollBarPositionChangedHandler ScrollBarPositionChanged;

        /// <summary>
        /// Range is the normalized length of the slider. The end position - start position.
        /// </summary>
        public double Range
        {
            set
            {
                _range = Math.Min(1, value);
                if (_range == 1)
                {
                    IsVisible = false;
                }
            }
            get
            {
                return _range;
            }
        }
        /// <summary>
        /// Position of the (start of the) slider bar
        /// </summary>
        public double Position
        {
            set
            {
                _position = value;
                ScrollBarPositionChanged?.Invoke(this, Position);
            }
            get
            {
                return _position;
            }

        }

        #endregion public members
        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="orientation"></param>
        public ScrollBarUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, Orientation orientation) : base(parent, resourceCreator)
        {
            _orientation = orientation;
            Position = 0;
            Range = 0;
            _isdragging = false;
            _scrollVelocity = 0.08;
            BorderWidth = 0;
        }

        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            base.Draw(ds);

            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;
            // draw the slide bar onto the rectangle
            ds.FillRectangle(new Rect(0, Position * Height, Width, Height * Range), ScrollBarColor);
            ds.Transform = orgTransform;


        }

        public override Task Load()
        {
            Dragged += ScrollBarUIElement_Dragged;
            Pressed += ScrollBarUIElement_Pressed;
            Released += ScrollBarUIElement_Released;
            PointerWheelChanged += ScrollBarUIElement_PointerWheelChanged;
            
            return base.Load();
        }

        public void ChangePosition(double delta)
        {

            if (delta < 0)
            {
                //If you're going up (position going down), set position + delta, with 0 as min.
                Position = Math.Max(0, Position + delta);
            }

            if (delta > 0)
            {
                //If you're going down (position going up), set position + delta, with 1-range being maximum.
                Position = (Position + delta + Range > 1) ? 1 - Range : Position + delta;

            }
        }
        private void ScrollBarUIElement_PointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta)
        {
            
        }

        public override void Dispose()
        {
            Dragged -= ScrollBarUIElement_Dragged;
            Pressed -= ScrollBarUIElement_Pressed;
            Released -= ScrollBarUIElement_Released;
            PointerWheelChanged -= ScrollBarUIElement_PointerWheelChanged;

            base.Dispose();
        }

        private void ScrollBarUIElement_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _isdragging = false;
        }

        private void ScrollBarUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // transform the pointers current point into the local coordinate system
            var currentPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);

            //If the point is outside bounds of the ScrolLBarUIElement, simply return
            if (currentPoint.X < 0 || currentPoint.X > Width || currentPoint.Y < 0 || currentPoint.Y > Height)
            {
                return;
            }

            //If the point is on the slide bar itself, return
            if (SlideBarHit(currentPoint))
            {
                return;
            }

            //TODO: If point is not on the slide bar, go to that point with the scroll velocity


        }

        /// <summary>
        /// Helper method checks whether you actually hit the slider of the scroll bar.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool SlideBarHit(Vector2 point)
        {
            if (point.Y / Height >= Position && point.Y / Height < Position + Range)
            {
                return true;
            }
            return false;
        }

        
        private void ScrollBarUIElement_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //Checks to make sure it is the actual bar that is being dragged

            var currentPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            if (!SlideBarHit(currentPoint) && !_isdragging)
            {
                return;
            }

            //Is dragging set to be true, so that we can "drag" the slider while our pointer is not hitting the thing.
            _isdragging = true;

            //Normalized vertical change
            var deltaY = pointer.DeltaSinceLastUpdate.Y / Height;

            ChangePosition(deltaY);


        }
    }
}