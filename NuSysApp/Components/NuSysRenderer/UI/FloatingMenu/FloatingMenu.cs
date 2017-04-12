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
    public class FloatingMenu : RectangleUIElement
    {
        private ButtonUIElement _addElementButton;
        private ButtonUIElement _openLibraryButton;
        private StackLayoutManager _buttonLayoutManager;

        private AddElementMenuUIElement _addElementMenu;

        private bool _disabled = false;

        /// <summary>
        ///  the initial drag position of the floating menu view
        /// </summary>
        private Vector2 _initialDragPosition;

        private float _originalHeight;
        private float _originalWidth;

        public FloatingMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            // set the default height and width of the floating menu view
            Height = UIDefaults.floatingMenuHeight;
            Width = UIDefaults.floatingMenuWidth;


            // set the default background
            Background = Colors.Transparent;

            _addElementButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "add element");
            AddChild(_addElementButton);

            _openLibraryButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "open library");
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

            _originalWidth = Width;
            _originalHeight = Height;

            SetAccessibilitySize(SessionController.Instance.SessionSettings.TextScale);

            DragStarted += FloatingMenu_DragStarted;
            _addElementButton.DragStarted += FloatingMenu_DragStarted;
            _openLibraryButton.DragStarted += FloatingMenu_DragStarted;
            Dragged += FloatingMenuOnDragged;
            _addElementButton.Dragged += FloatingMenuOnDragged;
            _openLibraryButton.Dragged += FloatingMenuOnDragged;
            _openLibraryButton.Tapped += OpenLibraryButtonOnTapped;
            _addElementButton.Tapped += ShowAddElementMenu;
            SessionController.Instance.SessionSettings.TextScaleChanged += SessionSettings_TextScaleChanged;
        }

        private void SessionSettings_TextScaleChanged(object sender, double e)
        {
            SetAccessibilitySize(e);
        }

        private void SetAccessibilitySize(double e)
        {
            _addElementButton.Resize(e);
            _openLibraryButton.Resize(e);
            _addElementMenu.Resize(e);

            Height = _originalHeight * (float)e;
            Width = _originalWidth * (float)e;
            _buttonLayoutManager.SetSize(Width, Height);
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
            _addElementMenu.Transform.LocalPosition = new Vector2(_addElementButton.Transform.LocalPosition.X - 15, _addElementButton.Transform.LocalPosition.Y);
        }

        private void OpenLibraryButtonOnTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            SessionController.Instance.NuSessionView.Library.IsVisible = !SessionController.Instance.NuSessionView.Library.IsVisible;
        }

        /// <summary>
        /// This load call is fired every time the nusessionviewer is reloaded, so make sure new items are only ever instantiated once by null checking them
        /// before creating them. This is essentially breaking the singleton pattern if you do reinstantiate items
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            _addElementButton.Image = _addElementButton.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/new icons/add elements white.png"));
            _openLibraryButton.Image = _openLibraryButton.Image ?? await MediaUtil.LoadCanvasBitmapAsync(Canvas, new Uri("ms-appx:///Assets/collection main menu.png"));

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
            SessionController.Instance.SessionSettings.TextScaleChanged -= SessionSettings_TextScaleChanged;

            base.Dispose();
        }


        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_disabled)
            {
                IsVisible = false;
                return;
            }

            _buttonLayoutManager.ArrangeItems();
            base.Update(parentLocalToScreenTransform);

        }

        public void HideFloatingMenu()
        {
            IsVisible = false;
            _disabled = true;
        }

        public void ShowFloatingMenu()
        {
            IsVisible = true;
            _disabled = false;
        }
    }
}
