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

namespace NuSysApp
{
    class FloatingMenu : RectangleUIElement
    {
        private ButtonUIElement _addElementButton;
        private ButtonUIElement _openLibraryButton;
        private StackLayoutManager _buttonLayoutManager;

        private LibraryListUIElement _library;

        private AddElementMenuUIElement _addElementMenu;

        /// <summary>
        ///  the initial drag position of the floating menu view
        /// </summary>
        private Vector2 _initialDragPosition;

        public FloatingMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the default height and width of the floating menu view
            Height = UIDefaults.floatingMenuHeight;
            Width = UIDefaults.floatingMenuWidth;

            // set the default background
            Background = Colors.Transparent;

            _addElementButton = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas))
            {
                Background = Colors.DarkSlateGray
            };
            AddChild(_addElementButton);

            _openLibraryButton = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas))
            {
                Background = Colors.DarkSlateGray
            };
            AddChild(_openLibraryButton);

            _addElementMenu = new AddElementMenuUIElement(this, Canvas)
            {
                IsVisible = false
            };
            AddChild(_addElementMenu);

            // initialize the layout manager
            _buttonLayoutManager = new StackLayoutManager
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Spacing = 20
            };
            _buttonLayoutManager.SetSize(Width, Height);
            _buttonLayoutManager.SetMargins(5, 5);
            _buttonLayoutManager.AddElement(_addElementButton);
            _buttonLayoutManager.AddElement(_openLibraryButton);
            _buttonLayoutManager.ArrangeItems(); // arrange the items only so widths and heights are properly instantiated


            DragStarted += FloatingMenu_DragStarted;
            _addElementButton.DragStarted += FloatingMenu_DragStarted;
            _openLibraryButton.DragStarted += FloatingMenu_DragStarted;
            Dragged += FloatingMenuOnDragged;
            _addElementButton.Dragged += FloatingMenuOnDragged;
            _openLibraryButton.Dragged += FloatingMenuOnDragged;
            _openLibraryButton.Tapped += OpenLibraryButtonOnTapped;
            _addElementButton.Tapped += ShowAddElementMenu;
        }

        private void FloatingMenu_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _initialDragPosition = Transform.LocalPosition;
        }

        private void ShowAddElementMenu(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            _addElementMenu.IsVisible = !_addElementMenu.IsVisible;
            var addElementButton = interactiveBaseRenderItem as ButtonUIElement;
            Debug.Assert(addElementButton != null, "The add element button should be a button ui element, make sure something else isn't causing this to open");
            _addElementMenu.Transform.LocalPosition = addElementButton.Transform.LocalPosition + new Vector2(-_addElementMenu.Width/2 + addElementButton.Width/2, -_addElementMenu.Height - 10);
        }

        private void OpenLibraryButtonOnTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            _library.IsVisible = !_library.IsVisible;
            var openLibraryButton = interactiveBaseRenderItem as ButtonUIElement;
            Debug.Assert(openLibraryButton != null, "The open library button should be a button ui element");
            _library.Transform.LocalPosition = openLibraryButton.Transform.LocalPosition + new Vector2(-_library.Width/2 + openLibraryButton.Width/2, openLibraryButton.Height + 10);
        }

        /// <summary>
        /// This load call is fired every time the nusessionviewer is reloaded, so make sure new items are only ever instantiated once by null checking them
        /// before creating them. This is essentially breaking the singleton pattern if you do reinstantiate items
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            // created here because it must be created after the create resources method is called on the main canvas animated control
            if (_library == null)
            {
                _library = new LibraryListUIElement(this, Canvas);
                AddChild(_library);
            }
            _library.IsVisible = false;

            _addElementButton.Image = _addElementButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_mainmenu_add_node.png"));
            _addElementButton.ImageBounds = new Rect(_addElementButton.Width / 4, _addElementButton.Height/4, _addElementButton.Width/2, _addElementButton.Height/2);
            _openLibraryButton.Image = _openLibraryButton.Image ?? await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_library.png"));
            _openLibraryButton.ImageBounds = new Rect(_openLibraryButton.Width / 4, _openLibraryButton.Height / 4, _openLibraryButton.Width / 2, _openLibraryButton.Height / 2);


            base.Load();
        }


        private void FloatingMenuOnDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            Transform.LocalPosition = _initialDragPosition + pointer.Delta;
        }

        public override void Dispose()
        {
            Dragged -= FloatingMenuOnDragged;
            _addElementButton.Dragged -= FloatingMenuOnDragged;
            _openLibraryButton.Dragged -= FloatingMenuOnDragged;
            _addElementButton.Tapped -= ShowAddElementMenu;
            _openLibraryButton.Tapped -= OpenLibraryButtonOnTapped;
            DragStarted -= FloatingMenu_DragStarted;
            _addElementButton.DragStarted -= FloatingMenu_DragStarted;
            _openLibraryButton.DragStarted -= FloatingMenu_DragStarted;

            base.Dispose();
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {



            _buttonLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);

        }
    }
}
