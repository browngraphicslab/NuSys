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

            _showLinksButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(_resizeElementTitlesButton, resourceCreator))
            {
                Width = 200,
                Height = 50,
                Background = Colors.Blue,
                ButtonTextColor = Colors.Red
            };
            AddChild(_showLinksButton);

            MinWidth = 220;
            MinHeight = 165;

            _resizeElementTitlesButton.Transform.LocalPosition = new Vector2(10, 35);
            _showLinksButton.Transform.LocalPosition = new Vector2(10, 100);
            _resizeElementTitlesButton.Tapped += ResizeElementTitlesButtonOnTapped;
            _showLinksButton.Tapped += ShowLinksButtonOnTapped;
            SessionController.Instance.SessionSettings.ResizeElementTitlesChanged += SessionSettingsOnResizeElementTitlesChanged;
            SessionController.Instance.SessionSettings.LinkVisibilityChanged += SessionSettingsOnLinkVisibilityChanged;
            SetButtonText();
        }

        /// <summary>
        /// dispose method simple removes the button event handlers
        /// </summary>
        public void Dispose()
        {
            _resizeElementTitlesButton.Tapped -= ResizeElementTitlesButtonOnTapped;
            _showLinksButton.Tapped -= ShowLinksButtonOnTapped;
        }

        /// <summary>
        /// Event handler for when the link visibility button is pressed.
        /// Shoudl toggle the option in the sessionSetting object.
        ///  </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ShowLinksButtonOnTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.SessionSettings.LinksVisible = !SessionController.Instance.SessionSettings.LinksVisible;
        }

        /// <summary>
        /// event handler called whenever the visibility setting for links changes globally.
        /// Should just set the correct button text
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="b"></param>
        private void SessionSettingsOnLinkVisibilityChanged(object sender, bool b)
        {
            SetButtonText();  
        }

        /// <summary>
        /// private method to refresh and set the button titles
        /// </summary>
        private void SetButtonText()
        {
            _resizeElementTitlesButton.ButtonText = "Resize Titles: " + SessionController.Instance.SessionSettings.ResizeElementTitles.ToString();
            _showLinksButton.ButtonText = "Show Links: "+SessionController.Instance.SessionSettings.LinksVisible.ToString(); 
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
    }
}
