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
using Windows.Foundation;

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

    public class LayoutWindowUIElement : DraggableWindowUIElement
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
        private static float FONT_SIZE = 20.0f;
        private static float BUTTON_SIZE = 100.0f;
        private static Rect BUTTON_IMAGEBOUNDS = new Rect(BUTTON_SIZE/4, BUTTON_SIZE/4, BUTTON_SIZE/2, BUTTON_SIZE/2);
        private static float BUTTON_PADDING = ((PANEL_WIDTH - 2 * PANEL_INSET) - 2 * BUTTON_SIZE) / 3.0f;
        private static float LAYOUT_BUTTONS_Y_START = 160.0f;
        private static Vector2 HORIZONTAL_BUTTON_POSITION = new Vector2(PANEL_INSET + BUTTON_PADDING, LAYOUT_BUTTONS_Y_START);
        private static Vector2 VERTICAL_BUTTON_POSITION = new Vector2(PANEL_INSET + BUTTON_SIZE + 2 * BUTTON_PADDING, LAYOUT_BUTTONS_Y_START);
        private static Vector2 GRID_BUTTON_POSITION = new Vector2(PANEL_INSET + BUTTON_PADDING, HORIZONTAL_BUTTON_POSITION.Y + BUTTON_SIZE + BUTTON_PADDING);
        private static Vector2 CUSTOM_BUTTON_POSITION = new Vector2(VERTICAL_BUTTON_POSITION.X, GRID_BUTTON_POSITION.Y);
        private static float DROPDOWN_INSET = 2.0f * PANEL_INSET;
        private static string LAYOUT_STYLE_TITLE_TEXT = "title";
        private static string LAYOUT_STYLE_DATE_TEXT = "date";
        private static string CLOSE_BUTTON_TEXT = "X X X X X";
        private static float CLOSE_BUTTON_SIZE = 100.0f;
        private static String CUSTOM_LAYOUT_TEXT = "Draw to arrange";

        private LayoutStyle _layoutStyle = LayoutStyle.Horizontal;
        private LayoutSorting _layoutSorting = LayoutSorting.Title;

        // Buttons
        private ButtonUIElement _arrangeButton;
        private ButtonUIElement _horizontalLayoutButton;
        private ButtonUIElement _verticalLayoutButton;
        private ButtonUIElement _gridLayoutButton;
        private ButtonUIElement _customLayoutButton;
        private ButtonUIElement _dropdownButton;
        private ButtonUIElement _closePanelButton;

        // Labels
        private TextboxUIElement _arrangeByLabel;
        private TextboxUIElement _layoutLabel;

        // Dropdown
        private DropdownUIElement _dropdown;

        // Layout handler
        public delegate void LayoutHandler(LayoutStyle style, LayoutSorting sorting);
        public event LayoutHandler DoLayout;

        /// <summary>
        /// A window with controls for laying out nodes.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public LayoutWindowUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Width = PANEL_WIDTH;
            Height = PANEL_HEIGHT;

            Background = Colors.White;
            Bordercolor = Constants.DARK_BLUE;
            BorderWidth = 2;

            TopBarColor = Constants.MED_BLUE;

            // Buttons
            // arrange button
            _arrangeButton = new RectangleButtonUIElement(this, resourceCreator);
            _arrangeButton.ButtonText = ARRANGE_TEXT;
            _arrangeButton.Width = ARRANGE_BUTTON_WIDTH;
            _arrangeButton.Height = ARRANGE_BUTTON_HEIGHT;
            _arrangeButton.Transform.LocalPosition = ARRANGE_BUTTON_POSITION;

            _arrangeButton.Tapped += ArrangeButtonTapped;
            AddChild(_arrangeButton);
            
            // horizontal layout button
            _horizontalLayoutButton = new EllipseButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle, "horizontal");
            AddImageToButton(resourceCreator, "ms-appx:///Assets/layout_icons/horizontal_layout_icon.png", _horizontalLayoutButton);
            _horizontalLayoutButton.SelectedBackground = Constants.MED_BLUE;
            _horizontalLayoutButton.Transform.LocalPosition = HORIZONTAL_BUTTON_POSITION;
            _horizontalLayoutButton.Width = BUTTON_SIZE;
            _horizontalLayoutButton.Height = BUTTON_SIZE;
            _horizontalLayoutButton.ImageBounds = BUTTON_IMAGEBOUNDS;

            _horizontalLayoutButton.Tapped += HorizontalButtonTapped;

            AddChild(_horizontalLayoutButton);

            // vertical layout button
            _verticalLayoutButton = new EllipseButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle, "vertical");
            AddImageToButton(resourceCreator, "ms-appx:///Assets/layout_icons/vertical_layout_icon.png", _verticalLayoutButton);
            _verticalLayoutButton.SelectedBackground = Constants.MED_BLUE;
            _verticalLayoutButton.Transform.LocalPosition = VERTICAL_BUTTON_POSITION;
            _verticalLayoutButton.Width = BUTTON_SIZE;
            _verticalLayoutButton.Height = BUTTON_SIZE;
            _verticalLayoutButton.ImageBounds = BUTTON_IMAGEBOUNDS;

            _verticalLayoutButton.Tapped += VerticalButtonTapped;

            AddChild(_verticalLayoutButton);

            // grid layout button
            _gridLayoutButton = new EllipseButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle,  "grid");
            AddImageToButton(resourceCreator, "ms-appx:///Assets/layout_icons/grid_layout_icon.png", _gridLayoutButton);
            _gridLayoutButton.SelectedBackground = Constants.MED_BLUE;
            _gridLayoutButton.Transform.LocalPosition = GRID_BUTTON_POSITION;
            _gridLayoutButton.Width = BUTTON_SIZE;
            _gridLayoutButton.Height = BUTTON_SIZE;
            _gridLayoutButton.ImageBounds = BUTTON_IMAGEBOUNDS;

            _gridLayoutButton.Tapped += GridButtonTapped;

            AddChild(_gridLayoutButton);

            // custom layout button
            _customLayoutButton = new EllipseButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle, "custom");
            AddImageToButton(resourceCreator, "ms-appx:///Assets/node icons/icon_enter.png", _customLayoutButton);
            _customLayoutButton.SelectedBackground = Constants.MED_BLUE;
            _customLayoutButton.Transform.LocalPosition = CUSTOM_BUTTON_POSITION;
            _customLayoutButton.Width = BUTTON_SIZE;
            _customLayoutButton.Height = BUTTON_SIZE;
            _customLayoutButton.ImageBounds = BUTTON_IMAGEBOUNDS;

            _customLayoutButton.Tapped += CustomButtonTapped;

            AddChild(_customLayoutButton);

            ResetButtonColors();

            _horizontalLayoutButton.Background = Constants.MED_BLUE;

            // labels
            _arrangeByLabel = new TextboxUIElement(this, resourceCreator);
            _arrangeByLabel.Text = ARRANGE_BY_TEXT;
            _arrangeByLabel.TextColor = Constants.ALMOST_BLACK;
            _arrangeByLabel.TextHorizontalAlignment = CanvasHorizontalAlignment.Center;
            _arrangeByLabel.TextVerticalAlignment = CanvasVerticalAlignment.Center;
            _arrangeByLabel.Width = ARRANGE_BUTTON_WIDTH;
            _arrangeByLabel.Height = ARRANGE_BUTTON_HEIGHT;
            _arrangeByLabel.FontSize = 30;
            _arrangeByLabel.Transform.LocalPosition = ARRANGE_BY_TEXT_POSITION;
            AddChild(_arrangeByLabel);

            // dropdown menu button - custom button
            _dropdownButton = new RectangleButtonUIElement(this, resourceCreator)
            {
                Background = Colors.White,
                Bordercolor = Constants.DARK_BLUE,
                BorderWidth = 1
            };
            _dropdownButton.ButtonText = LAYOUT_STYLE_TITLE_TEXT;
            _dropdownButton.Width = PANEL_WIDTH - 2 * DROPDOWN_INSET;
            _dropdownButton.Height = 40.0f;
            _dropdownButton.ButtonTextColor = Constants.ALMOST_BLACK;
            _dropdownButton.SelectedBackground = Constants.LIGHT_BLUE;
            _dropdownButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            _dropdownButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;

            _dropdownButton.Tapped += ViewListButtonTapped;

            _dropdownButton.Transform.LocalPosition = new Vector2(DROPDOWN_INSET, ARRANGE_BY_TEXT_POSITION.Y + _arrangeByLabel.Height);
            AddChild(_dropdownButton);

            // dropdown menu
            _dropdown = new DropdownUIElement(this, resourceCreator);
            _dropdown.Width = PANEL_WIDTH - 2 * DROPDOWN_INSET;
            _dropdown.AddOption("title");
            _dropdown.AddOption("date");
            _dropdown.Selected += ListButtonTapped;
            _dropdown.Transform.LocalPosition = new Vector2(DROPDOWN_INSET, _dropdownButton.Transform.LocalPosition.Y + _dropdownButton.Height);
            _dropdown.IsVisible = false;
            AddChild(_dropdown);
        }

/*
            // close button
            _closePanelButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator));
            _closePanelButton.ButtonText = CLOSE_BUTTON_TEXT;
            _closePanelButton.Width = CLOSE_BUTTON_SIZE;
            _closePanelButton.Height = CLOSE_BUTTON_SIZE;
            _closePanelButton.ButtonTextColor = Colors.Black;
            _closePanelButton.ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Left;
            _closePanelButton.ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center;
            _closePanelButton.Tapped += _closeButton_Tapped;
            _closePanelButton.Transform.LocalPosition = new Vector2(0.0f, 0.0f);
            //AddChild(_closePanelButton);
*/
        /// <summary>
        /// Calls the arrange callback.
        /// </summary>
        public void Arrange()
        {
            DoLayout?.Invoke(_layoutStyle, _layoutSorting);
        }

        /// <summary>
        /// Calls the arrange callback if the arrange configuration is custom.
        /// </summary>
        public void NotifyArrangeCustom()
        {
            if (_layoutStyle == LayoutStyle.Custom)
            {
                DoLayout?.Invoke(_layoutStyle, _layoutSorting);
            }
        }

        /// <summary>
        /// Helper method for adding images to buttons.
        /// </summary>
        /// <param name="resourceCreator"></param>
        /// <param name="uri"></param>
        /// <param name="button"></param>
        private async void AddImageToButton(ICanvasResourceCreatorWithDpi resourceCreator, string uri, ButtonUIElement button)
        {
            var bmp = await CanvasBitmap.LoadAsync(resourceCreator, new Uri(uri));
            button.Image = bmp;
        }

        /// <summary>
        /// Callback for when the arrange button is tapped.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ArrangeButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            Arrange();
        }

        /// <summary>
        /// Callback for the horizontal arrange button.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void HorizontalButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Constants.MED_BLUE;
            _layoutStyle = LayoutStyle.Horizontal;
            _arrangeButton.ButtonText = ARRANGE_TEXT;
            _arrangeButton.Background = Constants.MED_BLUE;
            _arrangeButton.Enabled = true;
        }

        /// <summary>
        /// Callback for the vertical arrange button.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void VerticalButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Constants.MED_BLUE;
            _layoutStyle = LayoutStyle.Vertical;
            _arrangeButton.ButtonText = ARRANGE_TEXT;
            _arrangeButton.Background = Constants.MED_BLUE;
            _arrangeButton.Enabled = true;
        }

        /// <summary>
        /// Callback for the grid arrange button.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void GridButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Constants.MED_BLUE;
            _layoutStyle = LayoutStyle.Grid;
            _arrangeButton.ButtonText = ARRANGE_TEXT;
            _arrangeButton.Background = Constants.MED_BLUE;
            _arrangeButton.Enabled = true;
        }

        /// <summary>
        /// Callback for the custom arrange button.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void CustomButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            ResetButtonColors();
            var button = (ButtonUIElement)item;
            button.Background = Constants.MED_BLUE;
            _layoutStyle = LayoutStyle.Custom;
            _arrangeButton.ButtonText = CUSTOM_LAYOUT_TEXT;
            _arrangeButton.Background = Constants.MED_BLUE;
            _arrangeButton.Enabled = false;
        }

        /// <summary>
        /// Callback for the arrange type list button.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ViewListButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _dropdown.IsVisible = true;
            _dropdownButton.Background = Constants.LIGHT_BLUE;
        }

        /// <summary>
        /// Resets the button colors to their defaults.
        /// </summary>
        private void ResetButtonColors()
        {
            _horizontalLayoutButton.Background = Constants.DARK_BLUE;
            _verticalLayoutButton.Background = Constants.DARK_BLUE;
            _gridLayoutButton.Background = Constants.DARK_BLUE;
            _customLayoutButton.Background = Constants.DARK_BLUE;
        }

        /// <summary>
        /// Called when one of the arrange type list types is selected.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ListButtonTapped(DropdownUIElement sender, string item)
        {
            switch (item)
            {
                case "title":
                    _layoutSorting = LayoutSorting.Title;
                    _dropdownButton.ButtonText = LAYOUT_STYLE_TITLE_TEXT;
                    break; 
                case "date":
                    _layoutSorting = LayoutSorting.Date;
                    _dropdownButton.ButtonText = LAYOUT_STYLE_DATE_TEXT;
                    break;
                default:
                    break;
            }
            _dropdown.IsVisible = false;
            _dropdownButton.Background = Colors.White;
        }

        public override void Dispose()
        {
            _arrangeButton.Tapped -= ArrangeButtonTapped;

            base.Dispose();
        }
    }
}
