using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class RectangleButtonUIElement : RectangleUIElement
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

        public RectangleButtonUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
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
