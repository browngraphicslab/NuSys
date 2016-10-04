using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ButtonUIElement : BaseInteractiveUIElement
    {
        /// <summary>
        /// The Color of the button when the button is selected
        /// </summary>
        public Color? SelectedBackground;

        /// <summary>
        /// The Color of the button border when the button is selected
        /// </summary>
        public Color? SelectedBorder;

        /// <summary>
        /// Save the original color of the background of the button to reset when the button is no longer selected
        /// </summary>
        private Color _orgBackground;

        /// <summary>
        /// Save the original color of the border of the button to reset when the button is no longer selected
        /// </summary>
        private Color _orgBorder;


        /// <summary>
        /// The width of the Button
        /// </summary>
        private float _width;

        /// <summary>
        /// The width of the Button. Must be greater than or equal to zero.
        /// </summary>
        public override float Width
        {
            get { return _width; }
            set
            {
                Debug.Assert(value >= 0);
                _width = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The height of the Button
        /// </summary>
        private float _height;
        /// <summary>
        /// The height of the Button. Must be greater than or equal to zero.
        /// </summary>
        public override float Height
        {
            get { return _height; }
            set
            {
                Debug.Assert(value >= 0);
                _height = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The background of the Button
        /// </summary>
        public override Color Background { get; set; }

        /// <summary>
        /// The width of the border of the Button
        /// </summary>
        private float _borderWidth;

        /// <summary>
        /// The Width of the Border of the Button. Extends into the Button.
        /// Must be greater than or equal to zero.
        /// </summary>
        public override float BorderWidth
        {
            get { return _borderWidth; }
            set
            {
                Debug.Assert(value >= 0);
                _borderWidth = value >= 0 ? value : 0;
            }
        }

        /// <summary>
        /// The BorderColor of the Button
        /// </summary>
        public override Color Bordercolor { get; set; }


        protected override void DrawBorder(CanvasDrawingSession ds)
        {
            // should be allocated to the passed in ui elemnt
        }

        public override Vector2 InitialOffset { get; set; }

        public override Vector4 ReturnBounds()
        {
            throw new NotImplementedException();
        }

        public ButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
        }



        /// <summary>
        /// Initializer method. Add handlers here
        /// </summary>
        /// <returns></returns>
        public override Task Load()
        {
            Pressed += RectangleButtonUIElement_Pressed;
            Released += RectangleButtonUIElement_Released;
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
        }

        /// <summary>
        /// The dispose method. Remove Handlers here
        /// </summary>
        public override void Dispose()
        {
            Pressed -= RectangleButtonUIElement_Pressed;
            Released -= RectangleButtonUIElement_Released;
            base.Dispose();
        }
    }
}
