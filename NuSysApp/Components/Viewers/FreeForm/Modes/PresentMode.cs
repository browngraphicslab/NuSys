using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class PresentMode : AbstractWorkspaceViewMode
    {
        private PointerEventHandler _pointerPressedHandler;

        /// <summary>
        /// Mode to be set in the FreeFormViewer when the user tries to start a presentation
        /// </summary>
        /// <param name="view"></param>
        public PresentMode(FreeFormViewer view):base(view)
        {
            _pointerPressedHandler = OnPointerPressed;
        }

       
        public override async Task Activate()
        {
            //TODO possibly clear the current selections
            _view.IsDoubleTapEnabled = false;
   
            _view.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, false);

        }

        public override async Task Deactivate()
        {

            _view.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);

        }

        /// <summary>
        /// When the user clicks on something, try and start a presentation at that element
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //this is most likely deprecated
            if (SessionController.Instance.SessionView.FreeFormViewer.MultiMenu.Visibility == Visibility.Visible)
            {
                return;
            }

            // try to present starting with the selected object, if it is presentation
            var frameWorkElemToBeExplored = e.OriginalSource as FrameworkElement;
            if (frameWorkElemToBeExplored != null && frameWorkElemToBeExplored.DataContext as ElementViewModel!=null)
            {
                //SessionController.Instance.SessionView.EnterPresentationMode(frameWorkElemToBeExplored.DataContext as ElementViewModel);
            }
        }
    }
}
