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

        // images for the drag rect
        private CanvasBitmap _textNodeDragImg;
        private CanvasBitmap _collectionDragImg;
        private CanvasBitmap _toolDragImg;
        private CanvasBitmap _recordingDragImg;

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

            _addTextNodeButton = new DraggableButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "text");
            AddChild(_addTextNodeButton);

            _addCollectionNodeButton = new DraggableButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "collection");

            AddChild(_addCollectionNodeButton);

            _addToolNodeButton = new DraggableButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "tools");

            AddChild(_addToolNodeButton);

            _addRecordingNodeButton = new DraggableButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "record");

            AddChild(_addRecordingNodeButton);

            // initialize a list of menu buttons which is useful for writing short code
            _menuButtons = new List<ButtonUIElement>()
            {
                _addRecordingNodeButton,
                _addTextNodeButton,
                _addCollectionNodeButton,
                _addToolNodeButton
            };

            Background = Constants.LIGHT_BLUE;
            
            ///***RADIAL DESIGN***
            /// hardcoding this for now
            _addTextNodeButton.Transform.LocalPosition = new Vector2(Transform.LocalPosition.X, Transform.LocalPosition.Y - _addTextNodeButton.Height*2f);
            _addCollectionNodeButton.Transform.LocalPosition = new Vector2(Transform.LocalPosition.X - _addCollectionNodeButton.Width*2f, 
                Transform.LocalPosition.Y - _addCollectionNodeButton.Height);
            _addToolNodeButton.Transform.LocalPosition = new Vector2(Transform.LocalPosition.X - _addToolNodeButton.Width*2f, 
                Transform.LocalPosition.Y + _addToolNodeButton.Height);
            _addRecordingNodeButton.Transform.LocalPosition = new Vector2(Transform.LocalPosition.X, Transform.LocalPosition.Y + _addRecordingNodeButton.Height*2f);

            ///***PREVIOUS DESIGN - box***
            // create a new stack layout manager using the ui variables
            //_buttonLayoutManager = new StackLayoutManager
            //{
            //    Spacing = _menuButtonSpacing,
            //    HorizontalAlignment = HorizontalAlignment.Stretch,
            //    VerticalAlignment = VerticalAlignment.Center,
            //    Width = Width,
            //    Height = Height,
            //    ItemHeight = _menuButtonHeight,
            //    ItemWidth = _menuButtonWidth
            //};
            //_buttonLayoutManager.SetMargins(_menuButtonLeftAndRightMargins, _menuButtonTopAndBottomMargins);

            // add each button the the stack layout manager and then add dragging and drag completed methods
            foreach (var button in _menuButtons)
            {
                button.Dragged += MenuButton_OnDragging;
                button.DragCompleted += MenuButton_DragCompleted;
                button.DragStarted += MenuButton_DragStarted;
                //_buttonLayoutManager.AddElement(button);
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
            _addRecordingNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/recording.png"));
            _addTextNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/text.png"));
            _addCollectionNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/collection.png"));
            _addToolNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));

            // load the images for the drag icon
            _textNodeDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/text.png"));
            _collectionDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/collection.png"));
            _recordingDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/recording.png"));
            _toolDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/new icons/tools.png"));
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
        private void MenuButton_DragCompleted(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            // reset the visibility of the drag rect
            _dragRect.IsVisible = false;

            // Add the element at the dropped location          
            StaticServerCalls.AddElementToCurrentCollection(pointer.CurrentPoint, _elementType);

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
            else
            {
                Debug.Fail("Load an image in the load method, and set it correctly here");
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
            else
            {
                Debug.Fail("Add support for the proper element type here");
            }

        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            //_buttonLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
