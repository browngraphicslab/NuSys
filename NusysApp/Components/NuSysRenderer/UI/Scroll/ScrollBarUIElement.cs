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

        /// <summary>
        /// BarLength represets the length of the scrollbar that can be occupied by the slider
        /// In other words, it is the length of the entire UIElement - the lengths of the arrow buttons
        /// </summary>
        private float _barLength;


        /// <summary>
        /// BarWidth represents the width of the scrollbar, which is the same as the width of the
        /// UIElement
        /// </summary>
        private float _barWidth;



        #endregion private members
        #region public members



        public enum Orientation { Horizontal, Vertical}

        /// <summary>
        /// Color of the slider. 
        /// </summary>
        public Color ScrollBarColor = Constants.MED_BLUE;

        /// <summary>
        /// Color of the slider when selected
        /// </summary>
        public Color SelectedScrollBarColor = Constants.DARK_BLUE;



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
        /// bool for whether or not the bar range is one, if the bar range is one
        /// then we hide the bar
        /// </summary>
        private bool _barRangeNotOne = true;

        /// <summary>
        /// What the user has set the scrollbar to be, default is true
        /// </summary>
        private bool _isVisible = true;

        public bool IsVisible
        {
            get { return base.IsVisible && _barRangeNotOne && _isVisible; }
            set
            {
                base.IsVisible = value;
                _isVisible = value;
            }
        }

        /// <summary>
        /// Range is the normalized length of the slider. The end position - start position.
        /// </summary>
        public float Range
        {
            set
            {
                _range = Math.Min(1, value);
                _barRangeNotOne = _range != 1f; //Scrolling should only be active if range is not equal to 1

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

        /// <summary>
        /// load in icons for buttons
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            _plusArrow.Image = _plusArrow.Image ??
                               await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/down arrow.png"));
            _minusArrow.Image = _minusArrow.Image ??
                                await
                                    CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/up arrow.png"));

            base.Load();
        }

        /// <summary>
        /// Initializes arrow buttons and listens to pressed events
        /// </summary>
        private void CreateArrows()
        {
            _plusArrow = new TransparentButtonUIElement(this, ResourceCreator);
            _minusArrow = new TransparentButtonUIElement(this, ResourceCreator);

            AddChild(_plusArrow);
            AddChild(_minusArrow);

            _plusArrow.Pressed += PlusArrow_Pressed;
            _minusArrow.Pressed += MinusArrow_Pressed;
        }
        /// <summary>
        /// Changes position by a negative delta (in vertical orientation, moves up
        /// while in horizontal orientation, moves left)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MinusArrow_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ChangePosition(-0.05f);
        }
        /// <summary>
        /// Changes position by a positive delta (in horizontal orientation, moves to right
        /// while in vertical orientation, moves down)
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void PlusArrow_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ChangePosition(0.05f);
        }
        /// <summary>
        /// Initializes slider and listens to its events
        /// </summary>
        private void CreateSlider()
        {
            _slider = new RectangleUIElement(this, ResourceCreator)
            {
                Background = ScrollBarColor
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

        /// <summary>
        /// Updates position based on horizontal/vertical distance dragged
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
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
            base.Update(parentLocalToScreenTransform);

            if (_orientation == Orientation.Horizontal)
            {
                _barWidth = Height;
                _barLength = Width - 2 * _barWidth;
                _slider.Height = _barWidth;
                _slider.Transform.LocalPosition = new Vector2(_barWidth + _barLength * Position, 0);

                if (_barLength * Range >= UIDefaults.MinSliderSize)
                {
                    _slider.Height = _barLength * Range;
                    _slider.Transform.LocalPosition = new Vector2(_barWidth + _barLength * Position, 0);

                }
                else
                {
                    _slider.Height = UIDefaults.MinSliderSize;
                    _slider.Transform.LocalPosition = new Vector2(_barWidth + (_barLength - _slider.Width) * Position,0);
                }
                _plusArrow.Width = _barWidth;
                _plusArrow.Height = _barWidth;
                _minusArrow.Width = _barWidth;
                _minusArrow.Height = _barWidth;
                _plusArrow.Transform.LocalPosition = new Vector2(Width - _barWidth, 0);
                _minusArrow.Transform.LocalPosition = new Vector2(0, 0);

                _plusArrow.ImageBounds = new Rect(1, 1, _barWidth - 2, _barWidth - 5);
                _minusArrow.ImageBounds = new Rect(1, 1, _barWidth - 2, _barWidth - 5);
            }
            else if (_orientation == Orientation.Vertical)
            {
                _barWidth = Width;
                _barLength = Height - 2 * _barWidth;
                _slider.Width = _barWidth;

                if(_barLength*Range >= UIDefaults.MinSliderSize)
                {
                    _slider.Height = _barLength * Range;
                    _slider.Transform.LocalPosition = new Vector2(0, _barWidth + (_barLength - _slider.Height) * Position);

                }else
                {
                    _slider.Height = UIDefaults.MinSliderSize;
                    _slider.Transform.LocalPosition = new Vector2(0, _barWidth + (_barLength - _slider.Height) * Position);
                }

                _plusArrow.Width = _barWidth;
                _plusArrow.Height = _barWidth;
                _minusArrow.Width = _barWidth;
                _minusArrow.Height = _barWidth;

                _plusArrow.Transform.LocalPosition = new Vector2(0, Height - _barWidth);
                _minusArrow.Transform.LocalPosition = new Vector2(0, 0);

                _plusArrow.ImageBounds = new Rect(1, 1, _barWidth - 2, _barWidth - 5);
                _minusArrow.ImageBounds = new Rect(1, 1, _barWidth - 2, _barWidth - 5);

            }

           
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
                ChangePosition(0.035f);
            }

            if (delta > 0)
            {
                ChangePosition(-0.035f);

            }

        }
        /// <summary>
        /// DIsposes ScrollBar events including slider and buttons
        /// </summary>
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

        /// <summary>
        /// Override allows us to hide the bar if it would be the entire height and should be hiddwen
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsVisible)
            {
                base.Draw(ds);
            }
        }
    }
}