using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace NuSysApp
{
  

        public class RadialMenu : EllipseUIElement
        {
            private ButtonUIElement _addElementButton;
            private ButtonUIElement _openLibraryButton;
            private StackLayoutManager _buttonLayoutManager;

            private AddElementMenuUIElement _addElementMenu;

            private bool _disabled = false;

            private bool _movable = true;

            /// <summary>
            ///  the initial drag position of the floating menu view
            /// </summary>
            private Vector2 _initialDragPosition;

            private float _originalHeight;
            private float _originalWidth;

            public RadialMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
            {
                // set the default height and width of the floating menu view
                Height = 100;
                Width = 100;


            // set the default background
            Background = Colors.Turquoise;

            _addElementButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "add element")
                {
                    IsVisible = false
                };
                AddChild(_addElementButton);

                _openLibraryButton = new EllipseButtonUIElement(this, Canvas, UIDefaults.PrimaryStyle, "open library")
                {
                    IsVisible = false
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

                _originalWidth = Width;
                _originalHeight = Height;

            //SetAccessibilitySize(SessionController.Instance.SessionSettings.TextScale);

                DragStarted += FloatingMenu_DragStarted;
                Dragged += FloatingMenuOnDragged;
                
                Tapped += OpenLibraryButtonOnTapped;
                DragCompleted += RadialMenuOnDragCompleted;
                
        }

        private void RadialMenuOnDragCompleted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (!_movable)
            {
                _addElementButton.IsVisible = false;
                _openLibraryButton.IsVisible = false;
            }
        }

        private void FloatingMenu_DragStarted(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_movable)
            {
                _initialDragPosition = Transform.LocalPosition;
            } else
            {
                _addElementButton.IsVisible = true;
                _openLibraryButton.IsVisible = true;
            }
        }

        private void FloatingMenuOnDragged(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_movable)
            {
                Transform.LocalPosition = _initialDragPosition + pointer.Delta;
            }
        }

        private void OpenLibraryButtonOnTapped(InteractiveBaseRenderItem interactiveBaseRenderItem, CanvasPointer pointer)
        {
            if (_movable)
            {
                Background = Colors.Yellow;
                _movable = false;
            } else
            {
                Background = Colors.Turquoise;
                _movable = true;
            }
        }
    }
    }
