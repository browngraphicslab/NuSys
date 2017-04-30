
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
using NusysIntermediate;

namespace NuSysApp.NusysRenderer
{
    /// <summary>
    /// The buttons in the radial menu
    /// </summary>
    public class RadialMenuButtons : RoundedRectangleUIElement
    {

        private List<ButtonUIElement> _buttons; //A list of the buttons in the menu
        private List<RadialMenuButtonContainer> _buttonInfo; //A list of the button containers with button info (type/image url/action)

        private List<ButtonUIElement> _menuButtons; // This list of menu buttons determines the order they are drawn


        private Dictionary<RectangleUIElement, Action<Vector2>> _dictionary; //Maps the rectangle shapes of buttons to their actions

        public RadialMenuButtons(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<RadialMenuButtonContainer> buttonContainers) : base(parent, resourceCreator)
        {
            _dictionary = new Dictionary<RectangleUIElement, Action<Vector2>>();
            _buttons = new List<ButtonUIElement>();
            _buttonInfo = new List<RadialMenuButtonContainer>();


            //Instantiates the buttons according the the buttons containers that are passed in
            for (var i = 0; i < buttonContainers.Count; i++)
            {
                var rectangle = new RectangleUIElement(this, resourceCreator);
                var button = new TransparentButtonUIElement(this, resourceCreator, rectangle, UIDefaults.PrimaryStyle, buttonContainers[i].Type.ToString());
                _dictionary.Add(rectangle, buttonContainers[i].Action);
                AddChild(button);
                _buttons.Add(button);
            }

            _buttonInfo = buttonContainers;
        }


        /// <summary>
        /// Load all the images and async rsources
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            // set the images for all the buttons
            for (var i = 0; i < _buttons.Count; i++)
            {
                _buttons[i].Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri(_buttonInfo[i].BitMapURI));
            }
           
            base.Load();
        }

        /// <summary>
        /// Returns the action associated with the button shape
        /// </summary>
        /// <param name="shape"></param>
        /// <returns></returns>
        public Action<Vector2> getActionFromShape(RectangleUIElement shape)
        {
            return _dictionary[shape];
        }



        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // set the radius of the circle, and get the center of the menu
            var radius = _buttons[0].Height * 2f;
            var centerX = Transform.LocalPosition.X + 25;
            var centerY = Transform.LocalPosition.Y + 25;

            // Math.Cos and Math.Sin take in radians, so just looked up the angles that are multiples of pi/4 to spread out up to 8 buttons around the radial menu at 45-degree increments
            // the x portion of the angle is Math.Cos(angle in radians), the y portion of the angle is Math.Sin(angle in radians)
            // put it all together, we get the center, and add the x portion to get the x coordinate of the transform, then do the same for the y
            for (var i = 0; i < _buttons.Count; i++)
            {
                var factor = .25 * i;
                _buttons[i].Transform.LocalPosition = new Vector2((float)(centerX  + Math.Cos(Math.PI * factor) * radius), (float)(centerY + Math.Sin(Math.PI * factor) * radius));
            }

            base.Update(parentLocalToScreenTransform);
        }
    }
}


