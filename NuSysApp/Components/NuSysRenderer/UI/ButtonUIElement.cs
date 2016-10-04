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
    public class ButtonUIElement : BaseInteractiveUIElement
    {
        /// <summary>
        /// The shape of the button. Can be one of Rectangle/Ellipse/RoudedRectangleUIElement.
        /// </summary>
        private BaseInteractiveUIElement _shape;
        private Color _orgBackground;
        private Color _orgBorder;

        /// <summary>
        /// The Background Color of the UI Element
        /// </summary>
        public override Color Background {
            get { return _shape.Background; }
            set { _shape.Background = value; } }

        /// <summary>
        /// The color of the Border of the UI Element;
        /// </summary>
        public override Color Bordercolor {
            get { return _shape.Bordercolor; }
            set { _shape.Bordercolor = value; } }

        /// <summary>
        /// The InitialOffset of the UIElement from the parent's upper left corner.
        /// Offsets from the top left of the screen if the parent is null.
        /// </summary>
        public override Vector2 InitialOffset {
            get { return _shape.InitialOffset; }
            set { _shape.InitialOffset = value; } }

        /// <summary>
        /// A func that calls a function which gets the bounds of the parent of the UIElement
        /// </summary>
        public Func<Vector4> GetParentBounds
        {
            get { return _shape.GetParentBounds; }
            set { _shape.GetParentBounds = value; }
        }

        /// <summary>
        /// A method which will return a vector containing the bounds of the element.
        /// The bounds are defined as the bounding box in which items can be contained.
        /// The Vector4 has the upper left x, upper left y, and lower right x, lower right y
        /// </summary>
        /// <returns></returns>
        public override Vector4 ReturnBounds()
        {
            return _shape.ReturnBounds();
        }


        /// <summary>
        /// Returns the shape's screen to local matrix
        /// </summary>
        public Func<Matrix3x2> GetParentScreenToLocalMatrix
        {
            get { return _shape.GetParentScreenToLocalMatrix; }
            set { _shape.GetParentScreenToLocalMatrix = value; }
        }

        

        /// <summary>
        /// The width of the Button. Must be greater than or equal to zero.
        /// </summary>
        public override float Width
        {
            get { return _shape.Width; }
            set
            {
                Debug.Assert(value >= 0);
                _shape.Width = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The height of the Button. Must be greater than or equal to zero.
        /// </summary>
        public override float Height
        {
            get { return _shape.Height; }
            set
            {
                Debug.Assert(value >= 0);
                _shape.Height = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The Width of the Border of the Button. Extends into the Button.
        /// Must be greater than or equal to zero.
        /// </summary>
        public override float BorderWidth
        {
            get { return _shape.BorderWidth; }
            set
            {
                Debug.Assert(value >= 0);
                _shape.BorderWidth = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The background color to be set while the button is in the pressed state.
        /// </summary>
        public Color? SelectedBackground { get; set; }

        /// <summary>
        /// The border color to be set while the button is in the pressed state.
        /// </summary>
        public Color? SelectedBorder { get; set; }

        public delegate void ButtonClickedHandler(ButtonUIElement item, CanvasPointer pointer);

        public event ButtonClickedHandler Clicked;

        public ButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BaseInteractiveUIElement shapeElement) : base(parent, resourceCreator)
        {
            _shape = shapeElement;

            // Add the shape that was passed in as a child of the button.
            AddChild(_shape);
        }



        /// <summary>
        /// Initializer method. Add handlers here
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            _shape.Pressed += RectangleButtonUIElement_Pressed;
            _shape.Released += RectangleButtonUIElement_Released;
            return base.Load();
        }

        /// <summary>
        /// Fired when the button is released. Changes the appearance of the button to reflect unselected appearance.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void RectangleButtonUIElement_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // reset the Background and Bordercolor to the original colors
            Background = _orgBackground;
            Bordercolor = _orgBorder;
        }

        /// <summary>
        /// Fired when the button is pressed. Changes the appearance of the button to reflect selected appearance.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void RectangleButtonUIElement_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            // save the Background and Bordercolor to reset them when the button is no longer pressed
            _orgBackground = Background;
            _orgBorder = Bordercolor;

            // set the Background and Border to SelectedBackground and SelectedBorder if either of those is not null
            Background = SelectedBackground ?? Background;
            Bordercolor = SelectedBorder ?? Background;

            // Fire the button's Clicked event. 
            Clicked?.Invoke(this, pointer);
        }

        /// <summary>
        /// Draws the shape the button is modeled as.
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            // return if the item is disposed or is not visible
            if (IsDisposed || !IsVisible)
                return;

            // Delegate drawing to the shape.
            _shape.Draw(ds);
            base.Draw(ds);
        }

        protected override void DrawBorder(CanvasDrawingSession ds)
        {
            //This has been left empty as the shape draws it's own border. 
        }

        /// <summary>
        /// The dispose method. Remove Handlers here
        /// </summary>
        public override void Dispose()
        {
            _shape.Pressed -= RectangleButtonUIElement_Pressed;
            _shape.Released -= RectangleButtonUIElement_Released;
            base.Dispose();
        }

        /// <summary>
        /// Returns the LocalBounds of the shape, used for hit testing. The bounds are given with the offset
        /// of the local matrix assumed to be zero. If the matrix is offset, then the local bounds must be offset accordingly
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return _shape.GetLocalBounds();
        }
    }
}
