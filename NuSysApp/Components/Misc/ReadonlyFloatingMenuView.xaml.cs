using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class ReadonlyFloatingMenuView : UserControl
    {
        /// <summary>
        /// This menu is to be seen in readonly mode
        /// </summary>
        public ReadonlyFloatingMenuView()
        {
            this.InitializeComponent();
            
        }

        /// <summary>
        /// When the present button is clicked, allow the user to start presentations by selecting nodes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xPresentButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.DeactivateAllButtons();
            xPresentButton.Activate();
            SessionController.Instance.SwitchMode(Options.Presentation);
            //SessionController.Instance.SessionView.EnterPresentationMode();
        }

        /// <summary>
        /// When the explore button is clicked, allow the user to start exploring by selecting nodes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xExploreButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.DeactivateAllButtons();
            xExploreButton.Activate();
            SessionController.Instance.SwitchMode(Options.Exploration);
           
            
        }

        /// <summary>
        /// Will revert to a normal collection if applicable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xRevertButton_Tapped(object sender, TappedRoutedEventArgs e)
        {
            this.DeactivateAllButtons();
            //xRevertButton.Activate();
            this.RevertToNormalWorkspace();

        }

        /// <summary>
        /// Will open the current collection as a normal, read-and-write-able workspace. Make sure
        /// to fill this in later, when the functionality can be achieved after some server development.
        /// </summary>
        private void RevertToNormalWorkspace()
        {
            // TODO implement
            SessionController.Instance.SessionView.ToggleReadonly();
        }

        /// <summary>
        /// Sets all buttons to inactive
        /// </summary>
        public void DeactivateAllButtons()
        {
            xPresentButton.Deactivate();
            xExploreButton.Deactivate();
            xRevertButton.Deactivate();
        }
    }
}
