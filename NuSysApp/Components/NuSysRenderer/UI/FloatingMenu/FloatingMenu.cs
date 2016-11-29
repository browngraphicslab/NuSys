using System;
using System.Collections.Generic;
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
            _addElementButton.Tapped += ShowAddElementMenu;
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



            Dragged += FloatingMenuOnDragged;
            _addElementButton.Dragging += FloatingMenuOnDragged;
            _openLibraryButton.Dragging += FloatingMenuOnDragged;
            _openLibraryButton.Tapped += OpenLibraryButtonOnTapped;
        }

        private void ShowAddElementMenu(ButtonUIElement item, CanvasPointer pointer)
        {
            _addElementMenu.IsVisible = !_addElementMenu.IsVisible;
            _addElementMenu.Transform.LocalPosition = item.Transform.LocalPosition + new Vector2(-_addElementMenu.Width/2 + item.Width/2, -_addElementMenu.Height - 10);
        }

        private void OpenLibraryButtonOnTapped(ButtonUIElement item, CanvasPointer pointer)
        {
            _library.IsVisible = !_library.IsVisible;
            _library.Transform.LocalPosition = item.Transform.LocalPosition + new Vector2(-_library.Width/2 + item.Width/2, item.Height + 10);
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
            Transform.LocalPosition += pointer.DeltaSinceLastUpdate;

        }

        public override void Dispose()
        {
            Dragged -= FloatingMenuOnDragged;
            _addElementButton.Dragging -= FloatingMenuOnDragged;
            _openLibraryButton.Dragging -= FloatingMenuOnDragged;
            _addElementButton.Tapped -= ShowAddElementMenu;
            _openLibraryButton.Tapped -= OpenLibraryButtonOnTapped;

            base.Dispose();
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {



            _buttonLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);

        }
    }
}
