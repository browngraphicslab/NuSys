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
    public class AddElementMenuUIElement : RectangleUIElement
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

        // determines if we are currently dragging a button
        private bool _isDragging;

        // the type of the element that is being dragged
        private NusysConstants.ElementType _elementType;

        // the layout manager for thebuttons
        private StackLayoutManager _buttonLayoutManager;

        // ui variables
        private float _menuButtonWidth = 50;
        private float _menuButtonHeight = 50;
        private float _menuButtonSpacing = 10;
        private float _menuButtonTopAndBottomMargins = 5;
        private float _menuButtonLeftAndRightMargins = 5;

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

            _addTextNodeButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas))
            {
                Height = _menuButtonHeight,
                Width = _menuButtonWidth,
                Background = Colors.Transparent,
                Bordercolor = Colors.Transparent,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 3,
                ImageBounds = new Rect(_menuButtonWidth / 4, _menuButtonHeight / 4, _menuButtonWidth / 2, _menuButtonHeight / 2)
            };
            AddChild(_addTextNodeButton);

            _addCollectionNodeButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas))
            {
                Height = _menuButtonHeight,
                Width = _menuButtonWidth,
                Background = Colors.Transparent,
                Bordercolor = Colors.Transparent,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 3,
                ImageBounds = new Rect(_menuButtonWidth / 4, _menuButtonHeight / 4, _menuButtonWidth / 2, _menuButtonHeight / 2)
            };
            AddChild(_addCollectionNodeButton);

            _addToolNodeButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas))
            {
                Height = _menuButtonHeight,
                Width = _menuButtonWidth,
                Background = Colors.Transparent,
                Bordercolor = Colors.Transparent,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 3,
                ImageBounds = new Rect(_menuButtonWidth / 4, _menuButtonHeight / 4, _menuButtonWidth / 2, _menuButtonHeight / 2)
            };
            AddChild(_addToolNodeButton);

            _addRecordingNodeButton = new ButtonUIElement(this, Canvas, new RectangleUIElement(this, Canvas))
            {
                Height = _menuButtonHeight,
                Width = _menuButtonWidth,
                Background = Colors.Transparent,
                Bordercolor = Colors.Transparent,
                SelectedBorder = Colors.LightGray,
                BorderWidth = 3,
                ImageBounds = new Rect(_menuButtonWidth / 4, _menuButtonHeight / 4, _menuButtonWidth / 2, _menuButtonHeight / 2)
            };
            AddChild(_addRecordingNodeButton);

            // initialize a list of menu buttons which is useful for writing short code
            _menuButtons = new List<ButtonUIElement>()
            {
                _addRecordingNodeButton,
                _addTextNodeButton,
                _addCollectionNodeButton,
                _addToolNodeButton
            };

            // initialize the height and width based on the number of buttons
            Height = _menuButtonHeight + 2*_menuButtonTopAndBottomMargins;
            Width = _menuButtons.Count*_menuButtonWidth + (_menuButtons.Count - 1)*_menuButtonSpacing +
                    2*_menuButtonLeftAndRightMargins;

            BorderWidth = 3;
            Bordercolor = Colors.LightGray;
            
            // create a new stack layout manager using the ui variables
            _buttonLayoutManager = new StackLayoutManager
            {
                Spacing = _menuButtonSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Width = Width,
                Height = Height,
                ItemHeight = _menuButtonHeight,
                ItemWidth = _menuButtonWidth
            };
            _buttonLayoutManager.SetMargins(_menuButtonLeftAndRightMargins, _menuButtonTopAndBottomMargins);

            // add each button the the stack layout manager and then add dragging and drag completed methods
            foreach (var button in _menuButtons)
            {
                button.Dragging += MenuButton_OnDragging;
                button.DragCompleted += MenuButton_DragCompleted;
                _buttonLayoutManager.AddElement(button);
            }
        }

        /// <summary>
        /// Load all the images and async rsources
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            // set the images for all the buttons
            _addRecordingNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/audio main menu.png"));
            _addTextNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/text node main menu.png"));
            _addCollectionNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/collection main menu.png"));
            _addToolNodeButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/tools icon.png"));

            // load the images for the drag icon
            _textNodeDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/library_thumbnails/text.png"));
            _collectionDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png"));
            _recordingDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/audio main menu.png"));
            _toolDragImg =
                await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/tools icon.png"));
            base.Load();
        }

        public override void Dispose()
        {
            foreach (var button in _menuButtons)
            {
                button.Dragging -= MenuButton_OnDragging;
                button.DragCompleted -= MenuButton_DragCompleted;
            }
            base.Dispose();
        }

        /// <summary>
        /// Fired when a menu button is being dragged
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MenuButton_OnDragging(ButtonUIElement item, CanvasPointer pointer)
        {
            // the first time this is fired, just set the image, element type, transform, and visibility of the dragrect
            if (!_isDragging)
            {
                SetElementType(item);
                SetDragImage(_elementType);
                _dragRect.Transform.LocalPosition = item.Transform.LocalPosition;
                _isDragging = true;
                _dragRect.IsVisible = true;
            }
            else // all other times just move the drag rect to a new location
            {
                _dragRect.Transform.LocalPosition = item.Transform.LocalPosition + pointer.Delta;
            }
        }

        /// <summary>
        /// Fired when drag is completed on a menu button, adds the proper element to the collection
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void MenuButton_DragCompleted(ButtonUIElement item, CanvasPointer pointer)
        {
            // reset visibility of the drag rect
            _isDragging = false;
            _dragRect.IsVisible = false;

            // transform the canvas pointer point to a point on the collection
            var r = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(pointer.CurrentPoint, SessionController.Instance.SessionView.FreeFormViewer.InitialCollection);

            // Add the element at the dropped location          
            AddElementToCollection(new Point(r.X, r.Y));

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

        /// <summary>
        /// Adds the element from the floating menu view to the collection at the specified point
        /// where the point is in workspace coordinates not the session view
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private async void AddElementToCollection(Point position)
        {
            var vm = SessionController.Instance.ActiveFreeFormViewer;

            // take care of filtering out elements that do not require requests to the server
            switch (_elementType)
            {
                case NusysConstants.ElementType.Collection:
                    break;
                case NusysConstants.ElementType.Text:
                    break;
                case NusysConstants.ElementType.Tools:
                    Debug.Assert(false);
                    return; // return after this we are not creating content
                case NusysConstants.ElementType.Recording:
                    Debug.Assert(false);
                    return; // return after this we are not creating content
                default:
                    Debug.Fail($"We do not support adding {_elementType} to the collection yet, please add support for it here");
                    return;
            }
            // Create a new content request
            var createNewContentRequestArgs = new CreateNewContentRequestArgs
            {
                LibraryElementArgs = new CreateNewLibraryElementRequestArgs
                {
                    AccessType =
                        SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType,
                    LibraryElementType = _elementType,
                    Title = _elementType == NusysConstants.ElementType.Collection ? "Unnamed Collection" : "Unnamed Text",
                    LibraryElementId = SessionController.Instance.GenerateId()
                },
                ContentId = SessionController.Instance.GenerateId()
            };

            // execute the content request
            var contentRequest = new CreateNewContentRequest(createNewContentRequestArgs);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(contentRequest);
            contentRequest.AddReturnedLibraryElementToLibrary();

            // create a new add element to collection request
            var newElementRequestArgs = new NewElementRequestArgs
            {
                LibraryElementId = createNewContentRequestArgs.LibraryElementArgs.LibraryElementId,
                ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                Height = Constants.DefaultNodeSize,
                Width = Constants.DefaultNodeSize,
                X = position.X,
                Y = position.Y
            };

            // execute the add element to collection request
            var elementRequest = new NewElementRequest(newElementRequestArgs);

            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(createNewContentRequestArgs.ContentId);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

            await elementRequest.AddReturnedElementToSessionAsync();

            // remove any selections from the activeFreeFormViewer
            //vm.ClearSelection();
        }



        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _buttonLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);
        }
    }
}
