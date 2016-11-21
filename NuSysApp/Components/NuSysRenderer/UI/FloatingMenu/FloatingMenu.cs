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

        public FloatingMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the default height and width of the floating menu view
            Height = UIDefaults.floatingMenuHeight;
            Width = UIDefaults.floatingMenuWidth;

            // set the default background
            Background = Colors.Transparent;

            _addElementButton = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas));
            _addElementButton.Background = Colors.DarkSlateGray;
            AddChild(_addElementButton);
            
            _openLibraryButton = new ButtonUIElement(this, Canvas, new EllipseUIElement(this, Canvas));
            _openLibraryButton.Background = Colors.DarkSlateGray;
            AddChild(_openLibraryButton);

            // initialize the layout manager
            _buttonLayoutManager = new StackLayoutManager();
            _buttonLayoutManager.HorizontalAlignment = HorizontalAlignment.Stretch;
            _buttonLayoutManager.VerticalAlignment = VerticalAlignment.Stretch;
            _buttonLayoutManager.SetSize(Width, Height);
            _buttonLayoutManager.SetMargins(5, 5);
            _buttonLayoutManager.Spacing = 20;
            _buttonLayoutManager.AddElement(_addElementButton);
            _buttonLayoutManager.AddElement(_openLibraryButton);
            _buttonLayoutManager.ArrangeItems(); // arrange the items only so widths and heights are properly instantiated



            Dragged += FloatingMenuOnDragged;
            _addElementButton.Dragging += FloatingMenuOnDragged;
            _openLibraryButton.Dragging += FloatingMenuOnDragged;
            _openLibraryButton.Tapped += OpenLibraryButtonOnTapped;
        }

        private void OpenLibraryButtonOnTapped(ButtonUIElement item, CanvasPointer pointer)
        {
            _library.IsVisible = !_library.IsVisible;
            _library.Transform.LocalPosition = new Vector2(-50, Height- 20);
        }

        public override async Task Load()
        {
            // created here because it must be created after the create resources method is called on the main canvas animated control
            _library = new LibraryListUIElement(this, Canvas)
            {
                BorderWidth = 5,
                Bordercolor = Colors.Black,
                TopBarColor = Colors.DarkSlateGray,
                Height = 400,
                Width = 400,
                MinWidth = 400,
                MinHeight = 400
            };
            AddChild(_library);
            _library.IsVisible = false;

            _addElementButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_mainmenu_add_node.png"));
            _addElementButton.ImageBounds = new Rect(_addElementButton.Width / 4, _addElementButton.Height/4, _addElementButton.Width/2, _addElementButton.Height/2);
            _openLibraryButton.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/icon_library.png"));
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
            base.Dispose();
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {



            _buttonLayoutManager.ArrangeItems();

            base.Update(parentLocalToScreenTransform);

        }
    }
}
