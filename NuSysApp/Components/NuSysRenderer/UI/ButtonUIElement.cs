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
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    public class ButtonUIElement : BaseInteractiveUIElement
    {
        /// <summary>
        /// The shape of the button. Can be one of Rectangle/Ellipse/RoudedRectangleUIElement.
        /// </summary>
        protected BaseInteractiveUIElement Shape;
        private Color _orgBackground;
        private Color _orgBorder;

        /// <summary>
        /// The Background Color of the UI Element
        /// </summary>
        public override Color Background {
            get { return Shape.Background; }
            set { Shape.Background = value; } }

        /// <summary>
        /// The color of the Border of the UI Element;
        /// </summary>
        public override Color Bordercolor {
            get { return Shape.Bordercolor; }
            set { Shape.Bordercolor = value; } }
        

        /// <summary>
        /// The width of the Button. Must be greater than or equal to zero.
        /// </summary>
        public override float Width
        {
            get { return Shape.Width; }
            set
            {
                Debug.Assert(value >= 0);
                Shape.Width = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The height of the Button. Must be greater than or equal to zero.
        /// </summary>
        public override float Height
        {
            get { return Shape.Height; }
            set
            {
                Debug.Assert(value >= 0);
                Shape.Height = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The Width of the Border of the Button. Extends into the Button.
        /// Must be greater than or equal to zero.
        /// </summary>
        public override float BorderWidth
        {
            get { return Shape.BorderWidth; }
            set
            {
                Debug.Assert(value >= 0);
                Shape.BorderWidth = value >= 0 ? value : 0;
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

        /// <summary>
        /// The string of text to be displayed on the button
        /// </summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// The image to be displayed on the button
        /// </summary>
        public CanvasBitmap Image { get; set;  }
        
        /// <summary>
        /// The color of the text on the button
        /// </summary>
        public Color ButtonTextColor { get; set; }

        /// <summary>
        /// The horizontal alignment of the text on the button
        /// </summary>
        public CanvasHorizontalAlignment ButtonTextHorizontalAlignment;

        /// <summary>
        /// The vertical alignment of the text on the button
        /// </summary>
        public CanvasVerticalAlignment ButtonTextVerticalAlignment;

        //todo both this and the click handler feel extraneous here, we already have a tapped event which
        //todo (cont.) exists throughout the entire ui. No need to unecessarily emulate xaml although we do that already :)
        public delegate void ButtonTappedHandler(ButtonUIElement item, CanvasPointer pointer);

        /// <summary>
        /// Fired when the Button is pressed
        /// </summary>
        public event ButtonTappedHandler OnPressed;

        /// <summary>
        /// Fired when the Button is released
        /// </summary>
        public event ButtonTappedHandler OnReleased;

        public ButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BaseInteractiveUIElement shapeElement) : base(parent, resourceCreator)
        {
            Shape = shapeElement;

            // Add the shape that was passed in as a child of the button.
            base.AddChild(Shape);

            Shape.Pressed += RectangleButtonUIElement_Pressed;
            Shape.Released += RectangleButtonUIElement_Released;
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

            OnReleased?.Invoke(this, pointer);
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
            OnPressed?.Invoke(this, pointer);
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
            base.Draw(ds);



            // draw the text on the button
            DrawButtonImage(ds);
            DrawButtonText(ds);
        }

        public virtual void DrawButtonImage(CanvasDrawingSession ds)
        {
            var orgTransform = ds.Transform;
            ds.Transform = Shape.Transform.LocalToScreenMatrix;

            if (Image != null)
            {
                ds.DrawImage(Image, new Rect(BorderWidth, BorderWidth, Width - 2 * BorderWidth, Height - 2 * BorderWidth));
            }

            ds.Transform = orgTransform;
        }

        public virtual void DrawButtonText(CanvasDrawingSession ds)
        {
            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Shape.Transform.LocalToScreenMatrix;

            if (ButtonText != null)
            {
                // create a text format object
                var textFormat = new CanvasTextFormat
                {
                    HorizontalAlignment = ButtonTextHorizontalAlignment,
                    VerticalAlignment = ButtonTextVerticalAlignment,
                    WordWrapping = CanvasWordWrapping.NoWrap,
                    TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
                    TrimmingSign = CanvasTrimmingSign.Ellipsis
                };

                // draw the text within the bounds (text auto fills the rect) with text color ButtonTextcolor, and the
                // just created textFormat
                ds.DrawText(ButtonText,
                    new Rect(BorderWidth, BorderWidth, Width - 2 * BorderWidth, Height - 2 * BorderWidth),
                    ButtonTextColor, textFormat);
            }

            ds.Transform = orgTransform;
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
            Shape.Pressed -= RectangleButtonUIElement_Pressed;
            Shape.Released -= RectangleButtonUIElement_Released;
            base.Dispose();
        }

        /// <summary>
        /// Returns the LocalBounds of the shape, used for hit testing. The bounds are given with the offset
        /// of the local matrix assumed to be zero. If the matrix is offset, then the local bounds must be offset accordingly
        /// </summary>
        /// <returns></returns>
        public override Rect GetLocalBounds()
        {
            return Shape.GetLocalBounds();
        }

        /// <summary>
        /// Adds a child to the button
        /// </summary>
        /// <param name="child"></param>
        public override void AddChild(BaseRenderItem child)
        {
            Shape.AddChild(child);
        }

        /// <summary>
        /// Removes a child from the button
        /// </summary>
        /// <param name="child"></param>
        public override void RemoveChild(BaseRenderItem child)
        {
            Shape.RemoveChild(child);
        }
    }
}
