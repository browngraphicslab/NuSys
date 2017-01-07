using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using System.Numerics;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    public class ScrollBarUIElement : RectangleUIElement
    {
        #region private members
        /// <summary>
        /// Position of the (start of the) slider bar
        /// </summary>
        private float _position;

        /// <summary>
        /// Normalized length of the slider. The end - start.
        /// </summary>
        private float _range;

        /// <summary>
        /// Orientation can be either vertical, horizontal, or both
        /// </summary>
        private Orientation _orientation;


        /// <summary>
        /// The actual slider that the user drags
        /// </summary>
        private RectangleUIElement _slider;
        /// <summary>
        /// The arrow button in the scroll bar that scrolls down in vertical mode and scrolls right in horizontal mode
        /// </summary>
        private ButtonUIElement _plusArrow;
        /// <summary>
        /// The arrow button in the scroll bar that scrolls up in vertical mode and scrolls left in horizontal mode
        /// </summary>
        private ButtonUIElement _minusArrow;

        #endregion private members
        #region public members
        /// <summary>
        /// TODO: Support horizontal scroll bars.
        /// </summary>
        public enum Orientation { Horizontal, Vertical}

        /// <summary>
        /// Color of the slider. 
        /// </summary>
        public Color ScrollBarColor = Constants.MED_BLUE;

        /// <summary>
        /// Color of the slider when selected
        /// </summary>
        public Color SelectedScrollBarColor = Constants.DARK_BLUE;


        public float BarLength { set; get; }

        public float BarWidth { set; get; }

        /// <summary>
        /// Invoked when the user slides the scroll bar to a new position. Position is
        /// double from 0 to 1. It repreesents the "start" of the scroll bar.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="position"></param>
        public delegate void ScrollBarPositionChangedHandler(object source, float position);

        /// <summary>
        /// Position is a float from 0 to 1 representing the start of the scroll bar
        /// </summary>
        public event ScrollBarPositionChangedHandler ScrollBarPositionChanged;

        /// <summary>
        /// Range is the normalized length of the slider. The end position - start position.
        /// </summary>
        public float Range
        {
            set
            {
                _range = Math.Min(1, value);
                IsVisible = _range != 1;
            }
            get
            {
                return _range;
            }
        }

        
        /// <summary>
        /// Position of the (start of the) slider bar
        /// </summary>
        public float Position
        {
            set
            {
                _position = value;
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

            Background = Constants.LIGHT_BLUE;
            Position = 0;
            Range = 0;
            
            CreateSlider();
            CreateArrows();

            Pressed += ScrollBarUIElement_Pressed;
            PointerWheelChanged += ScrollBarUIElement_PointerWheelChanged;
        }

        private void CreateArrows()
        {
            _plusArrow = new TransparentButtonUIElement(this, ResourceCreator);
            _minusArrow = new TransparentButtonUIElement(this, ResourceCreator);

            AddChild(_plusArrow);
            AddChild(_minusArrow);

            _plusArrow.Pressed += PlusArrow_Pressed;
            _minusArrow.Pressed += MinusArrow_Pressed;
        }

        private void MinusArrow_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ChangePosition(-0.05f);
        }

        private void PlusArrow_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ChangePosition(0.05f);
        }

        private void CreateSlider()
        {
            _slider = new RectangleUIElement(this, ResourceCreator)
            {
                Background = Constants.MED_BLUE
            };
            _slider.Dragged += Slider_Dragged;
            _slider.Pressed += Slider_Pressed;
            _slider.Released += Slider_Released;

            AddChild(_slider);
        }

        private void Slider_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _slider.Background = ScrollBarColor;
        }

        private void Slider_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _slider.Background = SelectedScrollBarColor;
        }
        private void Slider_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var currentPoint = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
            float delta = 0;
            if (_orientation == Orientation.Horizontal)
            {
                delta = pointer.DeltaSinceLastUpdate.X / Width;
            }
            else if (_orientation == Orientation.Vertical)
            {
                delta = pointer.DeltaSinceLastUpdate.Y / Height;
            }

            ChangePosition(delta);

        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_orientation == Orientation.Horizontal)
            {
                BarWidth = Height;
                BarLength = Width - 2 * BarWidth;
                _slider.Width = BarLength * Range;
                _slider.Height = BarWidth;
                _slider.Transform.LocalPosition = new Vector2(BarWidth + BarLength * Position, 0);

                _plusArrow.Width = BarWidth;
                _minusArrow.Height = BarWidth;
                _plusArrow.Transform.LocalPosition = new Vector2(Width - BarWidth, 0);
                _minusArrow.Transform.LocalPosition = new Vector2(0,0);

            }
            else if (_orientation == Orientation.Vertical)
            {
                BarWidth = Width;
                BarLength = Height - 2 * BarWidth;
                _slider.Width = BarWidth;
                _slider.Height = BarLength * Range;
                _slider.Transform.LocalPosition = new Vector2(0, BarWidth + BarLength * Position);

                _plusArrow.Width = BarWidth;
                _minusArrow.Height = BarWidth;

                _plusArrow.Transform.LocalPosition = new Vector2(0, Height - BarWidth);
                _minusArrow.Transform.LocalPosition = new Vector2(0, 0);

            }
            base.Update(parentLocalToScreenTransform);

           
        }
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;


            var orgTransform = ds.Transform;
            ds.Transform = Transform.LocalToScreenMatrix;

            base.Draw(ds);

            ds.Transform = orgTransform;

        }
        

        /// <summary>
        /// Call this method to change the position based on the (normalized) delta passed in.
        /// </summary>
        /// <param name="delta"></param>
        public void ChangePosition(float delta)
        {

            if (delta < 0)
            {
                //If you're going up (position going down), set position + delta, with 0 as min.
                Position = Math.Max(0, Position + delta);
            }

            if (delta > 0)
            {
                //If you're going down (position going up), set position + delta, with 1-range being maximum.
                Position = (Position + delta + Range > 1f) ? 1f - Range : Position + delta;

            }

            ScrollBarPositionChanged?.Invoke(this, Position);
        }
        private void ScrollBarUIElement_PointerWheelChanged(InteractiveBaseRenderItem item, CanvasPointer pointer, float delta)
        {
            if (delta < 0)
            {
                //If you're going up (position going down), set position + delta, with 0 as min.
                Position = (float) Math.Max(0, Position - 0.035f);
            }

            if (delta > 0)
            {
                //If you're going down (position going up), set position + delta, with 1-range being maximum.
                Position = (Position + 0.035 + Range > 1f) ? 1f - Range : Position + 0.035f;

            }
            ScrollBarPositionChanged?.Invoke(this, Position);

        }

        public override void Dispose()
        {
            Pressed -= ScrollBarUIElement_Pressed;
            PointerWheelChanged -= ScrollBarUIElement_PointerWheelChanged;

            _slider.Pressed -= Slider_Pressed;
            _slider.Dragged -= Slider_Dragged;


            _minusArrow.Pressed -= MinusArrow_Pressed;
            _plusArrow.Pressed -= PlusArrow_Pressed;
            base.Dispose();
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

            float newPosition = 0;
            if (_orientation == Orientation.Horizontal)
            {
                newPosition = currentPoint.X / Width;
            }
            else if (_orientation == Orientation.Vertical)
            {
                newPosition = currentPoint.Y / Height;
            }
            Position = (newPosition + Range > 1) ? 1 - Range : newPosition;
            //Sets the position to the point you clicked.
            ScrollBarPositionChanged?.Invoke(this, newPosition);

        }


    }
}