using System;
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
        /// the button for toggling the visibility of the bread crumb trail.
        /// </summary>
        private ButtonUIElement _showBreadCrumbsButton;

        /// <summary>
        /// Constructor will instatiate the private buttons.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public SessionSettingsMenu(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Background = Colors.Aquamarine;
            _resizeElementTitlesButton = new ButtonUIElement(this,resourceCreator,new RectangleUIElement(_resizeElementTitlesButton,resourceCreator))
            {
                Width = 200,
                Height = 50,
                Background = Colors.Blue,
                ButtonTextColor = Colors.Red
            };
            AddChild(_resizeElementTitlesButton);

            _showLinksButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(_showLinksButton, resourceCreator))
            {
                Width = 200,
                Height = 50,
                Background = Colors.Blue,
                ButtonTextColor = Colors.Red
            };
            AddChild(_showLinksButton);

            _showMinimapButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(_showMinimapButton, resourceCreator))
            {
                Width = 200,
                Height = 50,
                Background = Colors.Blue,
                ButtonTextColor = Colors.Red
            };
            AddChild(_showMinimapButton);

            _showBreadCrumbsButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(_showBreadCrumbsButton, resourceCreator))
            {
                Width = 200,
                Height = 50,
                Background = Colors.Blue,
                ButtonTextColor = Colors.Red
            };
            AddChild(_showBreadCrumbsButton);

            MinWidth = 220;
            MinHeight = 295;

            _resizeElementTitlesButton.Transform.LocalPosition = new Vector2(10, 35);
            _showLinksButton.Transform.LocalPosition = new Vector2(10, 100);
            _showMinimapButton.Transform.LocalPosition = new Vector2(10, 165);
            _showBreadCrumbsButton.Transform.LocalPosition = new Vector2(10, 230);
            _resizeElementTitlesButton.Tapped += ResizeElementTitlesButtonOnTapped;
            _showLinksButton.Tapped += ShowLinksButtonOnTapped;
            _showMinimapButton.Tapped += ShowMinimapTapped;
            _showBreadCrumbsButton.Tapped += _showBreadCrumbsButton_Tapped;
            SessionController.Instance.SessionSettings.ResizeElementTitlesChanged += SessionSettingsOnResizeElementTitlesChanged;
            SessionController.Instance.SessionSettings.LinkVisibilityChanged += SessionSettingsOnLinkVisibilityChanged;
            SessionController.Instance.SessionSettings.BreadCrumbVisibilityChanged += SessionSettings_BreadCrumbVisibilityChanged;
            SessionController.Instance.SessionSettings.MinimapVisiblityChanged += SessionSettings_MinimapVisiblityChanged;
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
            _showBreadCrumbsButton.Tapped -= _showBreadCrumbsButton_Tapped;
            SessionController.Instance.SessionSettings.ResizeElementTitlesChanged -= SessionSettingsOnResizeElementTitlesChanged;
            SessionController.Instance.SessionSettings.LinkVisibilityChanged -= SessionSettingsOnLinkVisibilityChanged;
        }

        /// <summary>
        /// private method to refresh and set the button titles
        /// </summary>
        private void SetButtonText()
        {
            _resizeElementTitlesButton.ButtonText = "Resize Titles: " + SessionController.Instance.SessionSettings.ResizeElementTitles.ToString();
            _showLinksButton.ButtonText = "Show Links: "+SessionController.Instance.SessionSettings.LinksVisible.ToString();
            _showMinimapButton.ButtonText = "Show Minimap: " + SessionController.Instance.SessionSettings.MinimapVisible.ToString();
            _showBreadCrumbsButton.ButtonText = "Show Bread Crumb Trail: " + SessionController.Instance.SessionSettings.BreadCrumbsVisible.ToString();
        }

        /// <summary>
        /// event handler for whenever the session's setting for minimap visiblity changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SessionSettings_MinimapVisiblityChanged(object sender, bool e)
        {
            SetButtonText();
        }

        /// <summary>
        /// event handler for whenever the session's setting for bread crumb trail visiblity changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SessionSettings_BreadCrumbVisibilityChanged(object sender, bool e)
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
        private void _showBreadCrumbsButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.SessionSettings.BreadCrumbsVisible = !SessionController.Instance.SessionSettings.BreadCrumbsVisible;
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
