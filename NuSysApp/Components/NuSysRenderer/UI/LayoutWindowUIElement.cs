using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using System.Numerics;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public enum LayoutStyle
    {
        Horizontal, Vertical, Grid, Custom
    }

    public enum LayoutSorting
    {
        Title, Date
    }

    class LayoutWindowUIElement : DraggableWindowUIElement
    {
        private static float PANEL_WIDTH = 300.0f;
        private static float PANEL_HEIGHT = 503.0f;
        private static float PANEL_INSET = 15.0f;
        private static float ARRANGE_BUTTON_WIDTH = PANEL_WIDTH - 2 * PANEL_INSET;
        private static float ARRANGE_BUTTON_HEIGHT = ARRANGE_BUTTON_WIDTH / 1.61f / 2.5f;
        private static Vector2 ARRANGE_BUTTON_POSITION = new Vector2(PANEL_INSET, PANEL_HEIGHT - PANEL_INSET - ARRANGE_BUTTON_HEIGHT);
        private static String ARRANGE_TEXT = "Arrange";
        private static String ARRANGE_BY_TEXT = @"Arrange by";
        private static Vector2 ARRANGE_BY_TEXT_POSITION = new Vector2(PANEL_INSET, 2 * PANEL_INSET);
        private static float FONT_SIZE = 30.0f;
        private static float BUTTON_SIZE = 100.0f;
        private static float BUTTON_PADDING = ((PANEL_WIDTH - 2 * PANEL_INSET) - 2 * BUTTON_SIZE) / 3.0f;
        private static float LAYOUT_BUTTONS_Y_START = 160.0f;
        private static Vector2 HORIZONTAL_BUTTON_POSITION = new Vector2(PANEL_INSET + BUTTON_PADDING, LAYOUT_BUTTONS_Y_START);
        private static Vector2 VERTICAL_BUTTON_POSITION = new Vector2(PANEL_INSET + BUTTON_SIZE + 2 * BUTTON_PADDING, LAYOUT_BUTTONS_Y_START);
        private static Vector2 GRID_BUTTON_POSITION = new Vector2(PANEL_INSET + BUTTON_PADDING, HORIZONTAL_BUTTON_POSITION.Y + BUTTON_SIZE + BUTTON_PADDING);
        private static Vector2 CUSTOM_BUTTON_POSITION = new Vector2(VERTICAL_BUTTON_POSITION.X, GRID_BUTTON_POSITION.Y);
        private static float DROPDOWN_INSET = 2.0f * PANEL_INSET;
        private LayoutStyle _layoutStyle = LayoutStyle.Horizontal;
        private string _layoutStyleText = "title";
        private LayoutSorting _layoutSorting = LayoutSorting.Title;

        // Buttons
        private ButtonUIElement _arrangeButton;
        private ButtonUIElement _horizontalLayoutButton;
        private ButtonUIElement _verticalLayoutButton;
        private ButtonUIElement _gridLayoutButton;
        private ButtonUIElement _customLayoutButton;
        private ButtonUIElement _dropdownButton;

        // Labels
        private TextboxUIElement _arrangeByLabel;
        private TextboxUIElement _layoutLabel;

        // Dropdown
        private DropdownUIElement _dropdown;

        public LayoutWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Width = PANEL_WIDTH;
            Height = PANEL_HEIGHT;

            // Buttons
            // arrange button
            _arrangeButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
            _arrangeButton.ButtonText = ARRANGE_TEXT;
            _arrangeButton.Width = ARRANGE_BUTTON_WIDTH;
            _arrangeButton.Height = ARRANGE_BUTTON_HEIGHT;
            _arrangeButton.ButtonTextColor = Colors.White;
            _arrangeButton.Background = Colors.Green;
            _arrangeButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _arrangeButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            _arrangeButton.Transform.LocalPosition = ARRANGE_BUTTON_POSITION;

            _arrangeButton.Tapped += _arrangeButton_Tapped;
            AddChild(_arrangeButton);
            
            // horizontal layout button
            _horizontalLayoutButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(parent, resourceCreator));
            AddImageToButton(resourceCreator, "ms-appx:///Assets/node icons/icon_enter.png", _horizontalLayoutButton);
            _horizontalLayoutButton.Transform.LocalPosition = HORIZONTAL_BUTTON_POSITION;
            _horizontalLayoutButton.Tapped += _horizontalButton_Tapped;
            AddChild(_horizontalLayoutButton);

            // vertical layout button
            _verticalLayoutButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(parent, resourceCreator));
            AddImageToButton(resourceCreator, "ms-appx:///Assets/node icons/icon_enter.png", _verticalLayoutButton);
            _verticalLayoutButton.Transform.LocalPosition = VERTICAL_BUTTON_POSITION;
            _verticalLayoutButton.Tapped += _verticalButton_Tapped;
            AddChild(_verticalLayoutButton);

            // grid layout button
            _gridLayoutButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(parent, resourceCreator));
            AddImageToButton(resourceCreator, "ms-appx:///Assets/node icons/icon_enter.png", _gridLayoutButton);
            _gridLayoutButton.Transform.LocalPosition = GRID_BUTTON_POSITION;
            _gridLayoutButton.Tapped += _gridButton_Tapped;
            AddChild(_gridLayoutButton);

            // custom layout button
            _customLayoutButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(parent, resourceCreator));
            AddImageToButton(resourceCreator, "ms-appx:///Assets/node icons/icon_enter.png", _customLayoutButton);
            _customLayoutButton.Transform.LocalPosition = CUSTOM_BUTTON_POSITION;
            _customLayoutButton.Tapped += _customButton_Tapped;
            AddChild(_customLayoutButton);

            // labels
            _arrangeByLabel = new TextboxUIElement(this, resourceCreator);
            _arrangeByLabel.Text = ARRANGE_BY_TEXT;
            _arrangeByLabel.TextColor = Colors.Black;
            _arrangeByLabel.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _arrangeByLabel.TextVerticalAlignment = CanvasVerticalAlignment.Center;
            _arrangeByLabel.Width = ARRANGE_BUTTON_WIDTH;
            _arrangeByLabel.Height = ARRANGE_BUTTON_HEIGHT;
            _arrangeByLabel.FontSize = FONT_SIZE;
            _arrangeByLabel.Transform.LocalPosition = ARRANGE_BY_TEXT_POSITION;
            AddChild(_arrangeByLabel);

            // dropdown menu button
            _dropdownButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
            _dropdownButton.ButtonText = _layoutStyleText;
            _dropdownButton.Width = PANEL_WIDTH - 2 * DROPDOWN_INSET;
            _dropdownButton.Height = 40.0f;
            _dropdownButton.ButtonTextColor = Colors.Black;
            _dropdownButton.SelectedBorder = Colors.Black;
            _dropdownButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            _dropdownButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            _dropdownButton.Tapped += _viewListButton_Tapped;
            _dropdownButton.Transform.LocalPosition = new Vector2(DROPDOWN_INSET, ARRANGE_BY_TEXT_POSITION.Y + _arrangeByLabel.Height);
            AddChild(_dropdownButton);

            // dropdown menu
            _dropdown = new DropdownUIElement(this, resourceCreator, PANEL_WIDTH - 2 * DROPDOWN_INSET);
            _dropdown.AddOption("title", _listButton_Tapped);
            _dropdown.AddOption("date", _listButton_Tapped);
            _dropdown.Layout();
            _dropdown.Transform.LocalPosition = new Vector2(DROPDOWN_INSET, _dropdownButton.Transform.LocalPosition.Y + _dropdownButton.Height);
            _dropdown.IsVisible = false;
            AddChild(_dropdown);
        }

        private async void AddImageToButton(ICanvasResourceCreatorWithDpi resourceCreator, string uri, ButtonUIElement button)
        {
            var bmp = await CanvasBitmap.LoadAsync(resourceCreator, new Uri(uri));
            button.Image = bmp;
        }

        private void _arrangeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            
        }

        private void _horizontalButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Colors.LightGray;
            _layoutStyle = LayoutStyle.Horizontal;
        }
        private void _verticalButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Colors.LightGray;
            _layoutStyle = LayoutStyle.Vertical;
        }

        private void _gridButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Colors.LightGray;
            _layoutStyle = LayoutStyle.Grid;
        }

        private void _customButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Colors.LightGray;
            _layoutStyle = LayoutStyle.Custom;
        }

        private void _viewListButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dropdown.IsVisible = true;
            _dropdownButton.Background = Colors.Gray;
        }

        private void ResetButtonColors()
        {
            _horizontalLayoutButton.Background = Colors.White;
            _verticalLayoutButton.Background = Colors.White;
            _gridLayoutButton.Background = Colors.White;
            _customLayoutButton.Background = Colors.White;
        }

        private void _listButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var button = (ButtonUIElement)item;
            switch (button.ButtonText)
            {
                case "title":
                    _layoutSorting = LayoutSorting.Title;
                    break; 
                case "date":
                    _layoutSorting = LayoutSorting.Date;
                    break;
                default:
                    break;
            }
            _dropdown.IsVisible = false;
            _dropdownButton.Background = Colors.White;
        }

        public override void Dispose()
        {
            _arrangeButton.Tapped -= _arrangeButton_Tapped;
            base.Dispose();
        }
    }
}
