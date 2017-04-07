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

namespace NuSysApp
{
    public class AddElementMenuUIElement : RoundedRectangleUIElement
    {
        // variables for all the buttons
        private ButtonUIElement _addTextNodeButton;
        private ButtonUIElement _addCollectionNodeButton;
        private ButtonUIElement _addToolNodeButton;
        private ButtonUIElement _addRecordingNodeButton;
        private ButtonUIElement _addVariableNode;
        private ButtonUIElement _addDisplayNode;

        // images for the drag rect
        private CanvasBitmap _textNodeDragImg;
        private CanvasBitmap _collectionDragImg;
        private CanvasBitmap _toolDragImg;
        private CanvasBitmap _recordingDragImg;
        private CanvasBitmap _variableDragImg;
        private CanvasBitmap _displayDragImg;

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


        public AddElementMenuUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

            _dragRect = new RectangleUIElement(this, Canvas)
            {
                IsVisible = false,
                IsHitTestVisible = false
            };
            AddChild(_dragRect);

            _addTextNodeButton = new TransparentButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "text");
            AddChild(_addTextNodeButton);

            _addCollectionNodeButton = new TransparentButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "collection");
            AddChild(_addCollectionNodeButton);

            _addToolNodeButton = new TransparentButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "tools");
            AddChild(_addToolNodeButton);

            _addRecordingNodeButton = new TransparentButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "record");
            AddChild(_addRecordingNodeButton);

            _addVariableNode = new TransparentButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "variable");
            AddChild(_addVariableNode);

            _addDisplayNode = new TransparentButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "display");
            AddChild(_addDisplayNode);

            // initialize a list of menu buttons which is useful for writing short code
            _menuButtons = new List<ButtonUIElement>()
            {
                _addRecordingNodeButton,
                _addTextNodeButton,
                _addCollectionNodeButton,
                _addToolNodeButton,
                _addVariableNode,
                _addDisplayNode
            };

            Background = Constants.LIGHT_BLUE;
           

            // add each button the the stack layout manager and then add dragging and drag completed methods
            foreach (var button in _menuButtons)
            {
                button.Dragged += MenuButton_OnDragging;
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
            _addRecordingNodeButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/recording.png"));
            _addTextNodeButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/text.png"));
            _addCollectionNodeButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/collection.png"));
            _addToolNodeButton.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));
            _addVariableNode.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));
            _addDisplayNode.Image = await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));

            // load the images for the drag icon
            _textNodeDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/text.png"));
            _collectionDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/collection.png"));
            _recordingDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/recording.png"));
            _toolDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));
            _displayDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));
            _variableDragImg =
                await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));
            base.Load();
        }

        public override void Dispose()
        {
            foreach (var button in _menuButtons)
            {
                button.Dragged -= MenuButton_OnDragging;
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
        private void MenuButton_OnDragging(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
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

            var dragDestination = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemAt(pointer.CurrentPoint, null, 2);

            /*
            if (_elementType == NusysConstants.ElementType.Variable || _elementType == NusysConstants.ElementType.Text)
            {
                if (dragDestination is CustomViewerDisplay || dragDestination is CustomDisplayElementRenderItem)
                {
                    await CustomViewerDisplay.AddElementToDisplay(_elementType);
                    return;
                }
                else if (dragDestination.Parent is CustomViewerDisplay || dragDestination.Parent is CustomDisplayElementRenderItem)
                {
                    await CustomViewerDisplay.AddElementToDisplay(_elementType);
                    return;
                }
            }*/

            // Add the element at the dropped location          
            await StaticServerCalls.AddElementToWorkSpace(pointer.CurrentPoint, _elementType).ConfigureAwait(false);

            if (_elementType == NusysConstants.ElementType.Variable)
            {
                /*
                var pop = new CenteredPopup(Parent.Parent,ResourceCreator,"Enter the metadata key: ");
                var textBox = new ScrollableTextboxUIElement(pop,ResourceCreator, false, false);
                textBox.TextChanged += delegate(InteractiveBaseRenderItem item, string text)
                {
                    var controller = SessionController.Instance.ContentController.AllLibraryElementControllers
                        .OfType<VariableLibraryElementController>()
                        .OrderBy(i => DateTime.Parse(i.LibraryElementModel.Timestamp))
                        .Last();
                    controller.SetMetadataKey(text);
                };
                pop.AddChild(textBox);
                textBox.Width = 200;
                textBox.Height = 75;
                pop.Height += 120;
                Parent.Parent.AddChild(pop);*/
            }

        }

        /// <summary>
        /// resizes all component buttons of the add element menu
        /// </summary>
        /// <param name="e"></param>
        public void Resize(double e)
        {
            _addTextNodeButton.Resize(e);
            _addToolNodeButton.Resize(e);
            _addCollectionNodeButton.Resize(e);
            _addRecordingNodeButton.Resize(e);
        }

        /// <summary>
        /// Sets the drag image of the dragrect based on the current _elementType, call setElementType with the clicked on button
        /// to set the element type correctly
        /// </summary>
        /// <param name="button"></param>
        private void SetDragImage(NusysConstants.ElementType type)
        {
            if (type == NusysConstants.ElementType.Text)
            {
                _dragRect.Image = _textNodeDragImg;
            }
            else if (type == NusysConstants.ElementType.Collection)
            {
                _dragRect.Image = _collectionDragImg;
            }
            else if (type == NusysConstants.ElementType.Recording)
            {
                _dragRect.Image = _recordingDragImg;
            }
            else if (type == NusysConstants.ElementType.Tools)
            {
                _dragRect.Image = _toolDragImg;
            }
            else if (type == NusysConstants.ElementType.Variable)
            {
                _dragRect.Image = _variableDragImg;
            }
            else if (type == NusysConstants.ElementType.Display)
            {
                _dragRect.Image = _displayDragImg;
            }
            else
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
            if (button == _addTextNodeButton)
            {
                _elementType = NusysConstants.ElementType.Text;
            }
            else if (button == _addCollectionNodeButton)
            {
                _elementType = NusysConstants.ElementType.Collection;
            }
            else if (button == _addRecordingNodeButton)
            {
                _elementType = NusysConstants.ElementType.Recording;
            }
            else if (button == _addToolNodeButton)
            {
                _elementType = NusysConstants.ElementType.Tools;
            }
            else if (button == _addVariableNode)
            {
                _elementType = NusysConstants.ElementType.Variable;
            }
            else if (button == _addDisplayNode)
            {
                _elementType = NusysConstants.ElementType.Display;
            }
            else
            {
                Debug.Assert(false, "Add support for the proper element type here");
            }

        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            // get the radius of the circle
            var radius = _addTextNodeButton.Height * 2f;
            var center = Transform.LocalPosition;

            // Math.Cos and Math.Sin take in radians, so just looked up the angles which were pi/2, 5pi/6, 7pi/6 and 3pi/2
            // the x portion of the angle is Math.Cos(angle in radians), the y portion of the angle is Math.Sin(angle in radians)
            // put it all together, we get the center, and add the x portion to get the x coordinate of the transform, then do the same for the y
            _addTextNodeButton.Transform.LocalPosition = new Vector2((float) (center.X + Math.Cos(Math.PI / 2) * radius), (float) (center.Y +   Math.Sin(Math.PI / 2) * radius));
            _addCollectionNodeButton.Transform.LocalPosition = new Vector2((float)(center.X + Math.Cos(5 * Math.PI / 6) * radius), (float)(center.Y + Math.Sin(5 * Math.PI / 6) * radius));
            _addToolNodeButton.Transform.LocalPosition = new Vector2((float)(center.X + Math.Cos(7 * Math.PI / 6) * radius), (float)(center.Y + Math.Sin(7 * Math.PI / 6)  * radius));
            _addRecordingNodeButton.Transform.LocalPosition = new Vector2((float)(center.X + Math.Cos(3 * Math.PI / 2) * radius), (float)(center.Y + Math.Sin(3 * Math.PI / 2)  * radius));

            _addVariableNode.Transform.LocalPosition = new Vector2((float)(center.X + 105 + Math.Cos(Math.PI / 2) * radius), (float)(center.Y + Math.Sin(Math.PI / 2) * radius));
            _addDisplayNode.Transform.LocalPosition = new Vector2((float)(center.X + 105 + Math.Cos(3 * Math.PI / 2) * radius), (float)(center.Y + Math.Sin(3 * Math.PI / 2) * radius));


            //_buttonLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
