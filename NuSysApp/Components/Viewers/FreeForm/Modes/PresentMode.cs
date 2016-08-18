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

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //this is most likely deprecated
            if (SessionController.Instance.SessionView.FreeFormViewer.MultiMenu.Visibility == Visibility.Visible)
            {
                return;
            }

            // try to explore the selected object, if it is explorable
            var frameWorkElemToBeExplored = e.OriginalSource as FrameworkElement;
            if (frameWorkElemToBeExplored != null)
            {
                SessionController.Instance.SessionView.EnterPresentationMode(frameWorkElemToBeExplored.DataContext as ElementViewModel);
            }
        }
    }
}
