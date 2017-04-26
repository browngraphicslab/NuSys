using Microsoft.Graphics.Canvas;
using NuSysApp.NusysRenderer;
using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{

    /// <summary>
    /// The Radial Menu class provides a way to make radial menus with the specified action, label, and image for the buttons (The information for the buttons is stored in the button containers which get passed in as a parameter). 
    /// </summary>
    public class RadialMenu : EllipseUIElement
    {

        private AddElementMenuUIElement _addElementMenu;
        private bool _disabled = false;
        private int _selectedButtonIndex;
        private float _originalHeight;
        private float _originalWidth;

        private EllipseButtonUIElement _selectedHighlight;

        private RadialMenuButtons _radialMenuButtons;

        public RadialMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<RadialMenuButtonContainer> buttonContainers) : base(parent, resourceCreator)
        {
            // set the default height and width of the floating menu view
            Height = 100;
            Width = 100;


            // set the default background
            Background = Colors.Transparent;

            // The circle that highlights selected elements on the radial menu
            _selectedHighlight = new EllipseButtonUIElement(this, Canvas)
            {
                Width = 90,
                Height = 90,
                Background = Colors.AliceBlue,
                IsVisible = false
            };
            AddChild(_selectedHighlight);

            //Instantiate the buttons
            _radialMenuButtons = new RadialMenuButtons(this, Canvas, buttonContainers)
            {
                IsVisible = false
            };
            AddChild(_radialMenuButtons);


            _originalWidth = Width;
            _originalHeight = Height;


            DragStarted += RadialMenu_DragStarted;
            Dragged += RadialMenuOnDragged;
            DragCompleted += RadialMenuOnDragCompleted;

        }


        private async void RadialMenuOnDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var hitTest = this.HitTest(pointer.CurrentPoint);
            var casted = hitTest?.Parent as RadialMenuButtons;

            //If drag ends on a buttone
            if (casted != null && hitTest != this)
            {
                //Perform that button's action
                _radialMenuButtons.getActionFromShape(hitTest as RectangleUIElement).Invoke(this.Transform.LocalPosition);

            }
            
            //Hide everything
            _selectedHighlight.IsVisible = false;
            IsVisible = false;
            _radialMenuButtons.IsVisible = false;


        }

        private void RadialMenu_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _radialMenuButtons.IsVisible = true;
        }



        private void RadialMenuOnDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var hitTest = this.HitTest(pointer.CurrentPoint);
            var casted = hitTest?.Parent as RadialMenuButtons;

            //If the user is touching a button
            if (casted != null && hitTest != this)
            {
                //Put the highlight over the button
                _selectedHighlight.IsVisible = true;
                _selectedHighlight.Transform.LocalPosition = new Vector2(hitTest.Transform.Parent.LocalPosition.X - 20, hitTest.Transform.Parent.LocalPosition.Y - 3);
            }
            else
            {
                //Hide the highlight
                _selectedHighlight.IsVisible = false;              
            }       
        }       
    }
}
