﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    /// <summary>
    /// This class will be the ui element for changing and viewing the session settings.
    /// It should toggle on and off by the settings button in the session view
    /// </summary>
    public class SessionSettingsMenu : ResizeableWindowUIElement
    {
        /// <summary>
        /// the button for toggling the element title resizing setting
        /// </summary>
        private ButtonUIElement _resizeElementTitlesButton;

        /// <summary>
        /// the button for toggling the visibility of links
        /// </summary>
        private ButtonUIElement _showLinksButton;

        /// <summary>
        /// the button for toggling the visibility of the minimap.
        /// </summary>
        private ButtonUIElement _showMinimapButton;

        /// <summary>
        /// the button for docking the bread crumb trail to the side of the window.
        /// </summary>
        private ButtonUIElement _dockBreadCrumbsButton;

        /// <summary>
        /// slider for changing the session's font and button sizes. 
        /// </summary>
        private SliderUIElement _textSizeSlider;

        /// <summary>
        /// Constructor will instatiate the private buttons.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public SessionSettingsMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            _resizeElementTitlesButton = new RectangleButtonUIElement(this, resourceCreator);
            AddChild(_resizeElementTitlesButton);

            _showLinksButton = new RectangleButtonUIElement(this, resourceCreator);
            AddChild(_showLinksButton);

            _showMinimapButton = new RectangleButtonUIElement(this, resourceCreator);
            AddChild(_showMinimapButton);

            _dockBreadCrumbsButton = new RectangleButtonUIElement(this, resourceCreator);
            AddChild(_dockBreadCrumbsButton);

            _textSizeSlider = new SliderUIElement(this, resourceCreator, 1, 10)
            {
                SliderPosition = (float)((SessionController.Instance.SessionSettings.TextScale - .75)/3),
                Width = 200,
                Height = 50
            };
            AddChild(_textSizeSlider);

            MinWidth = 220;
            MinHeight = 350;

            _resizeElementTitlesButton.Transform.LocalPosition = new Vector2(10, 35);
            _showLinksButton.Transform.LocalPosition = new Vector2(10, 100);
            _showMinimapButton.Transform.LocalPosition = new Vector2(10, 165);
            _dockBreadCrumbsButton.Transform.LocalPosition = new Vector2(10, 230);
            _textSizeSlider.Transform.LocalPosition = new Vector2(10, 295);
            _resizeElementTitlesButton.Tapped += ResizeElementTitlesButtonOnTapped;
            _showLinksButton.Tapped += ShowLinksButtonOnTapped;
            _showMinimapButton.Tapped += ShowMinimapTapped;
            _dockBreadCrumbsButton.Tapped += DockBreadCrumbsButtonTapped;
            _textSizeSlider.OnSliderMoved += SliderChanged;
            _textSizeSlider.OnSliderMoveCompleted += TextSizeSliderOnOnSliderMoveCompleted;
            SessionController.Instance.SessionSettings.ResizeElementTitlesChanged += SessionSettingsOnResizeElementTitlesChanged;
            SessionController.Instance.SessionSettings.LinkVisibilityChanged += SessionSettingsOnLinkVisibilityChanged;
            SessionController.Instance.SessionSettings.BreadCrumbPositionChanged += SessionSettingsBreadCrumbPositionChanged;
            SessionController.Instance.SessionSettings.MinimapVisiblityChanged += SessionSettingsMinimapVisiblityChanged;
            SessionController.Instance.SessionSettings.TextScaleChanged += SessionSettingsTextScaleChanged;
            SetButtonText();
        }

        /// <summary>
        /// dispose method simple removes the button event handlers
        /// </summary>
        public void Dispose()
        {
            _resizeElementTitlesButton.Tapped -= ResizeElementTitlesButtonOnTapped;
            _showLinksButton.Tapped -= ShowLinksButtonOnTapped;
            _showMinimapButton.Tapped -= ShowMinimapTapped;
            _dockBreadCrumbsButton.Tapped -= DockBreadCrumbsButtonTapped;
            _textSizeSlider.OnSliderMoved -= SliderChanged;
            _textSizeSlider.OnSliderMoveCompleted -= TextSizeSliderOnOnSliderMoveCompleted;
            SessionController.Instance.SessionSettings.ResizeElementTitlesChanged -= SessionSettingsOnResizeElementTitlesChanged;
            SessionController.Instance.SessionSettings.LinkVisibilityChanged -= SessionSettingsOnLinkVisibilityChanged;
            SessionController.Instance.SessionSettings.BreadCrumbPositionChanged -= SessionSettingsBreadCrumbPositionChanged;
            SessionController.Instance.SessionSettings.MinimapVisiblityChanged -= SessionSettingsMinimapVisiblityChanged;
            SessionController.Instance.SessionSettings.TextScaleChanged -= SessionSettingsTextScaleChanged;
        }

        /// <summary>
        /// Event handler fired whenever the text slider has completed a drag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="d"></param>
        private void TextSizeSliderOnOnSliderMoveCompleted(object sender, double d)
        {
            SessionController.Instance.SessionSettings.SaveToFile();
        }

        /// <summary>
        /// private method to refresh and set the button titles
        /// </summary>
        private void SetButtonText()
        {
            _resizeElementTitlesButton.ButtonText = "Resize Titles: " + SessionController.Instance.SessionSettings.ResizeElementTitles.ToString();
            _showLinksButton.ButtonText = "Show Links: "+SessionController.Instance.SessionSettings.LinksVisible.ToString();
            _showMinimapButton.ButtonText = "Show Minimap: " + SessionController.Instance.SessionSettings.MinimapVisible.ToString();
            _dockBreadCrumbsButton.ButtonText = "Dock Bread Crumb Trail: " + SessionController.Instance.SessionSettings.BreadCrumbsDocked.ToString();
        }

        /// <summary>
        /// event handler for when the global text scale changes from the session settings object
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SessionSettingsTextScaleChanged(object sender, double e)
        {
            SetButtonText();
        }


        /// <summary>
        /// event handler for whenever the session's setting for minimap visiblity changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SessionSettingsMinimapVisiblityChanged(object sender, bool e)
        {
            SetButtonText();
        }

        /// <summary>
        /// event handler for whenever the session's setting for bread crumb trail visiblity changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SessionSettingsBreadCrumbPositionChanged(object sender, bool e)
        {
            SetButtonText();
        }

        /// <summary>
        /// event handler called whenever the visibility setting for links changes globally.
        /// Should just set the correct button text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="b"></param>
        private void SessionSettingsOnLinkVisibilityChanged(object sender, LinkVisibilityOption visibility)
        {
            SetButtonText();
        }


        /// <summary>
        /// Event handler called whenever the setting for ElementTitleResizing changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="b"></param>
        private void SessionSettingsOnResizeElementTitlesChanged(object sender, bool b)
        {
            SetButtonText();
        }


        /// <summary>
        /// event handler for when the user changes the slider value in this menu.
        /// Should change the global scale value;
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="currSliderPosition"></param>
        private void SliderChanged(SliderUIElement sender, double currSliderPosition)
        {
            SessionController.Instance.SessionSettings.TextScale = Math.Round(Math.Max(currSliderPosition * 3,0) + .75,1);
        }


        /// <summary>
        /// Event Handler fired every time the resizeElementTitleButton is tapped.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ResizeElementTitlesButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.SessionSettings.ResizeElementTitles = !SessionController.Instance.SessionSettings.ResizeElementTitles;
        }

        /// <summary>
        /// event handler for when the bread crumbs visibility option toggle is pressed
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void DockBreadCrumbsButtonTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.SessionSettings.BreadCrumbsDocked = !SessionController.Instance.SessionSettings.BreadCrumbsDocked;
            SessionController.Instance.NuSessionView.UpdateUI();
            if (SessionController.Instance.SessionSettings.BreadCrumbsDocked)
            {
                SessionController.Instance.NuSessionView.TrailBox.IsVisible = true;
            }
        }

        /// <summary>
        /// event handler for when the minimap visibility option is toggled.
        /// Should just change the session setting for the minimap
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ShowMinimapTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.SessionSettings.MinimapVisible = !SessionController.Instance.SessionSettings.MinimapVisible;
            SessionController.Instance.SessionView.FreeFormViewer.InvalidateMinimap();
        }

        /// <summary>
        /// Event handler for when the link visibility button is pressed.
        /// Shoudl toggle the option in the sessionSetting object.
        ///  </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ShowLinksButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //Weird syntax but just modularly increments enum valure
            SessionController.Instance.SessionSettings.LinksVisible = (LinkVisibilityOption)(((int)SessionController.Instance.SessionSettings.LinksVisible + 1) % Enum.GetNames(typeof(LinkVisibilityOption)).Length);
        }

    }
}
