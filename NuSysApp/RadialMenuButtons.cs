
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
    public class RadialMenuButtons : RoundedRectangleUIElement
    {
        // variables for all the buttons

        private List<ButtonUIElement> _buttons;
        private List<RadialMenuButtonContainer> _buttonInfo;

        // images for the drag rect

        private List<CanvasBitmap> _dragImages;

        // drag rect used to display a drag image when we are moving items onto the canvas
        private RectangleUIElement _dragRect;

        // the type of the element that is being dragged
        private NusysConstants.ElementType _elementType;

        // the layout manager for thebuttons
        //private StackLayoutManager _buttonLayoutManager;

        // ui variables
        private float _menuButtonWidth = 50;
        private float _menuButtonHeight = 50;
        private float _menuButtonSpacing = 10;
        private float _menuButtonTopAndBottomMargins = 15;
        private float _menuButtonLeftAndRightMargins = 15;

        /// <summary>
        /// This list of menu buttons determines the order they are drawn
        /// </summary>
        private List<ButtonUIElement> _menuButtons;


        public RadialMenuButtons(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, List<RadialMenuButtonContainer> buttonContainers) : base(parent, resourceCreator)
        {
            _buttons = new List<ButtonUIElement>();
            _buttonInfo = new List<RadialMenuButtonContainer>();
            _dragImages = new List<CanvasBitmap>();

            _dragRect = new RectangleUIElement(this, Canvas)
            {
                IsVisible = true,
                IsHitTestVisible = false,
                Background = Colors.Transparent
            };
            AddChild(_dragRect);

            for (var i = 0; i < buttonContainers.Count; i++)
            {
                var button = new TransparentButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, buttonContainers[i].Type.ToString());
                AddChild(button);
                _buttons.Add(button);
            }

            _buttonInfo = buttonContainers;


            //Background = Constants.LIGHT_BLUE;


            // add each button the the stack layout manager and then add dragging and drag completed methods
            foreach (var button in _buttons)
            {
                button.Dragged += MenuButton_OnDragged;
                button.DragCompleted += MenuButton_DragCompleted;
                button.DragStarted += MenuButton_DragStarted;
            }
        }

        private void MenuButton_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SetElementType(item as ButtonUIElement);
            SetDragImage(_elementType);
            _dragRect.Transform.LocalPosition = item.Transform.LocalPosition;
            _dragRect.IsVisible = true;
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
                //_dragImages[i] = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri(_buttonInfo[i].BitMapURI));
            }
            //_buttons[1].Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/recording.png"));
            //_addTextNodeButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/text.png"));
            //_addCollectionNodeButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/collection.png"));
            //_addToolNodeButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));

            // load the images for the drag icon
            /*_textNodeDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/text.png"));
            _collectionDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/collection.png"));
            _recordingDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/recording.png"));
            _toolDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));
           */
            base.Load();
        }


        public override void Dispose()
        {
            foreach (var button in _menuButtons)
            {
                button.Dragged -= MenuButton_OnDragged;
                button.DragCompleted -= MenuButton_DragCompleted;
                button.DragStarted -= MenuButton_DragStarted;
            }
            base.Dispose();
        }

        /// <summary>
        /// Fired when a menu button is being dragged
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MenuButton_OnDragged(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {

            // move the drag rect to a new location
            _dragRect.Transform.LocalPosition = interactiveBaseRenderItem.Transform.LocalPosition + pointer.Delta;
        }

        /// <summary>
        /// Fired when drag is completed on a menu button, adds the proper element to the collection
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private async void MenuButton_DragCompleted(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            // reset the visibility of the drag rect
            _dragRect.IsVisible = false;

            // Add the element at the dropped location          
            await StaticServerCalls.AddElementToWorkSpace(pointer.CurrentPoint, _elementType).ConfigureAwait(false);

        }

        /// <summary>
        /// resizes all component buttons of the add element menu
        /// </summary>
        /// <param name="e"></param>
        public void Resize(double e)
        {
            foreach (ButtonUIElement button in _buttons)
            {
                button.Resize(e);
            }
        }

        /// <summary>
        /// Sets the drag image of the dragrect based on the current _elementType, call setElementType with the clicked on button
        /// to set the element type correctly
        /// </summary>
        /// <param name="button"></param>
        private void SetDragImage(NusysConstants.ElementType type)
        {
            bool imageSet = false;

            for (var i = 0; i < _buttons.Count; i++)
            {
                if (type == _buttonInfo[i].Type)
                {
                    //_dragRect.Image = _dragImages[i];
                    imageSet = true;
                }
            }
            if (!imageSet)
            {
                Debug.Assert(false, "Load an image in the load method, and set it correctly here");
            }
        }

        /// <summary>
        /// Sets the current element type based on the passed in ButtonUIElement
        /// </summary>
        /// <param name="button"></param>
        private void SetElementType(ButtonUIElement button)
        {
            for (var i = 0; i < _buttons.Count; i++)
            {
                if (button == _buttons[i])
                {
                    _elementType = _buttonInfo[i].Type;
                }
            }
            //else
            //{
            //   Debug.Assert(false, "Add support for the proper element type here");
            //}

        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // get the radius of the circle
            var radius = _buttons[0].Height * 2f;
            var centerX = Transform.LocalPosition.X + 25;
            var centerY = Transform.LocalPosition.Y + 25;

            for (var i = 0; i < _buttons.Count; i++)
            {
                var factor = .25 * i;
                _buttons[i].Transform.LocalPosition = new Vector2((float)(centerX  + Math.Cos(Math.PI * factor) * radius), (float)(centerY + Math.Sin(Math.PI * factor) * radius));
            }
            // Math.Cos and Math.Sin take in radians, so just looked up the angles which were pi/2, 5pi/6, 7pi/6 and 3pi/2
            // the x portion of the angle is Math.Cos(angle in radians), the y portion of the angle is Math.Sin(angle in radians)
            // put it all together, we get the center, and add the x portion to get the x coordinate of the transform, then do the same for the y
            /*_addTextNodeButton.Transform.LocalPosition = new Vector2((float)(center.X + Math.Cos(Math.PI / 2) * radius), (float)(center.Y + Math.Sin(Math.PI / 2) * radius));
            _addCollectionNodeButton.Transform.LocalPosition = new Vector2((float)(center.X + Math.Cos(5 * Math.PI / 6) * radius), (float)(center.Y + Math.Sin(5 * Math.PI / 6) * radius));
            _addToolNodeButton.Transform.LocalPosition = new Vector2((float)(center.X + Math.Cos(7 * Math.PI / 6) * radius), (float)(center.Y + Math.Sin(7 * Math.PI / 6) * radius));
            _addRecordingNodeButton.Transform.LocalPosition = new Vector2((float)(center.X + Math.Cos(3 * Math.PI / 2) * radius), (float)(center.Y + Math.Sin(3 * Math.PI / 2) * radius));
            */
            //_buttonLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}


