﻿using System;
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
            set { Shape.Bordercolor = value; }
        }



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
        /// Enables or disables the button
        /// </summary>
        public Boolean Enabled { get; set; }

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
        public override ICanvasImage Image
        {
            get { return Shape.Image; }
            set { Shape.Image = value; }
        }

        /// <summary>
        /// The color of the text on the button
        /// </summary>
        public Color ButtonTextColor { get; set; } = UIDefaults.TextColor;

        /// <summary>
        /// The size of the text on the button
        /// </summary>
        public float ButtonTextSize { get; set; } = UIDefaults.ButtonTextSize;

        /// <summary>
        /// The horizontal alignment of the text on the button
        /// </summary>
        public CanvasHorizontalAlignment ButtonTextHorizontalAlignment;

        /// <summary>
        /// The vertical alignment of the text on the button
        /// </summary>
        public CanvasVerticalAlignment ButtonTextVerticalAlignment;

        /// <summary>
        /// The bounds of the image we want to draw on the button, 
        /// The image is drawn within these bounds, unless it is set to null
        ///  in which case the image is drawn in the local bounds used for hit testing
        /// </summary>
        public override Rect? ImageBounds
        {
            get { return Shape.ImageBounds; }
            set { Shape.ImageBounds = value; }
        }


        /// <summary>
        /// For instantiating a button, pass in the usual parent and resource creator.  
        /// Then pass in another baseInteractiveUIElement to be used as the shape of the button.
        /// 
        /// The button will encapsulate that shape.  
        /// FOR MOST CASES, YOU WILL NOT NEED TO USE THIS CONSTRUCTOR - YOU SHOULD BE INSTANTIATING A SPECIFIC BUTTON.
        /// See EllipseButtonUIElement, DraggableButtonUIElement, RoundedRectButtonUIElement and RectangleButtonUIElement.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="shapeElement"></param>
        public ButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BaseInteractiveUIElement shape) : base(parent, resourceCreator)
        {
            Shape = shape;

            // Add the shape that was passed in as a child of the button.
            base.AddChild(Shape);

            // add all the manipulation methods
            Shape.Pressed += Shape_Pressed;
            Shape.Released += Shape_Released;
            Shape.Dragged += Shape_Dragged;
            Shape.Tapped += Shape_Tapped;
            Shape.DoubleTapped += Shape_DoubleTapped;

            Enabled = true;   
        }


        /// <summary>
        /// Fired the double tapped event on the button when the shape double tap event is fired
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Shape_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnDoubleTapped(pointer);
        }

        /// <summary>
        /// Fired the tapped event on the button when the shape tap event is fired
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Shape_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnTapped(pointer);
        }

        /// <summary>
        /// Fires the drag event on the button when the shape dragged event is fired
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Shape_Dragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnDragged(pointer);
        }

        /// <summary>
        /// Fires the release event on the button when the released event on the shape is fired
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Shape_Released(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnReleased(pointer);
        }

        /// <summary>
        /// overrides the normal on released event to set background colors correctly
        /// </summary>
        /// <param name="pointer"></param>
        public override void OnReleased(CanvasPointer pointer)
        {
            if (!Enabled)
            {
                return;
            }
           
            // reset the Background and Bordercolor to the original colors
            Background = _orgBackground;
            Bordercolor = _orgBorder;

            base.OnReleased(pointer);
        }

        /// <summary>
        /// Fires the pressed event on the button when the pressed event on the shape is fired
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void Shape_Pressed(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            OnPressed(pointer);
        }

        /// <summary>
        /// Overrides the normal on pressed event to set background colors correctly
        /// </summary>
        /// <param name="pointer"></param>
        public override void OnPressed(CanvasPointer pointer)
        {
            if (!Enabled)
            {
                return;
            }

            // save the Background and Bordercolor to reset them when the button is no longer pressed
            _orgBackground = Background;
            _orgBorder = Bordercolor;

            // set the Background and Border to SelectedBackground and SelectedBorder if either of those is not null
            Background = SelectedBackground ?? Background;
            Bordercolor = SelectedBorder ?? Background;

            base.OnPressed(pointer);
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

            DrawButtonText(ds);
        }
    
        public virtual void DrawButtonText(CanvasDrawingSession ds)
        {
            // save the current transform of the drawing session
            var orgTransform = ds.Transform;
            ds.Transform = Shape.Transform.LocalToScreenMatrix;

            if (ButtonText != null)
            {
                // draw the text within the bounds (text auto fills the rect) with text color ButtonTextcolor, and the
                // just created textFormat
                ds.DrawText(ButtonText, GetTextBoundingBox(),ButtonTextColor, GetCanvasTextFormat());
            }

            ds.Transform = orgTransform;
        }

        /// <summary>
        /// get text bounding box. this is overriden in classes where the shape is not a rectangle/the text is not to be drawn
        /// inside the button shape.
        /// </summary>
        /// <returns></returns>
        protected virtual Rect GetTextBoundingBox()
        {
            return new Rect(BorderWidth, BorderWidth, Width - 2*BorderWidth, Height - 2*BorderWidth);
        }

        /// <summary>
        /// get canvas text format. this will be overridden if you need to change the wrapping style, etc. for text that is not drawn inside
        /// the button shape.
        /// </summary>
        /// <returns></returns>
        protected virtual CanvasTextFormat GetCanvasTextFormat()
        {
            // create a text format object
            var textFormat = new CanvasTextFormat
            {
                HorizontalAlignment = ButtonTextHorizontalAlignment,
                VerticalAlignment = ButtonTextVerticalAlignment,
                WordWrapping = CanvasWordWrapping.NoWrap,
                TrimmingGranularity = CanvasTextTrimmingGranularity.Character,
                TrimmingSign = CanvasTrimmingSign.Ellipsis,
                FontSize = ButtonTextSize
            };

            return textFormat;
        }


        protected override void DrawBorder(CanvasDrawingSession ds)
        {
            //This has been left empty as the shape draws it's own border. 
        }

        protected override void DrawBackground(CanvasDrawingSession ds)
        {
            //This has been left empty as the shape draws it's own background. 
        }

        protected override void DrawImage(CanvasDrawingSession ds)
        {
            //This has been left empty as the shape draws it's own image. 
        }

        /// <summary>
        /// The dispose method. Remove Handlers here
        /// </summary>
        public override void Dispose()
        {
            Shape.Pressed -= Shape_Pressed;
            Shape.Released -= Shape_Released;
            Shape.Dragged -= Shape_Dragged;
            Shape.Tapped -= Shape_Tapped;
            Shape.DoubleTapped -= Shape_DoubleTapped;
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
