using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Media;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MyToolkit.Utilities;

namespace NuSysApp
{
    public class SelectMode : AbstractWorkspaceViewMode
    {

        private bool _released;
        private bool _doubleTapped;
        private PointerEventHandler _pointerPressedHandler;
        private PointerEventHandler _pointerReleasedHandler;
        private DoubleTappedEventHandler _doubleTappedHandler;

        public SelectMode(FreeFormViewer view):base(view)
        {
            _pointerPressedHandler = OnPointerPressed;
            _pointerReleasedHandler = OnPointerReleased;
            _doubleTappedHandler = OnDoubleTapped;
        }
        public override async Task Activate()
        {
            _view.IsDoubleTapEnabled = true;

            _view.ManipulationMode = ManipulationModes.All;

            _view.AddHandler(UIElement.PointerPressedEvent, _pointerPressedHandler, false );
            _view.AddHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler, false );
            _view.AddHandler(UIElement.DoubleTappedEvent, _doubleTappedHandler, true );
        }

        public override async Task Deactivate()
        {
            _view.IsDoubleTapEnabled = false;

            _view.RemoveHandler(UIElement.PointerPressedEvent, _pointerPressedHandler);
            _view.RemoveHandler(UIElement.PointerReleasedEvent, _pointerReleasedHandler);
            _view.RemoveHandler(UIElement.DoubleTappedEvent, _doubleTappedHandler);

            _view.ManipulationMode = ManipulationModes.None;
  
        //    var vm = _view.DataContext as FreeFormViewerViewModel;
        //    vm.ClearSelection();
        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _released = false;
            await Task.Delay(100);
            if (!_released)
                return;

            await Task.Delay(50);
            if (_doubleTapped)
            {
                _doubleTapped = false;
                return;
            }

            var dc = ((FrameworkElement)e.OriginalSource).DataContext as ElementViewModel;
            if (dc == null)
                return;

            
            var viwerVm = (FreeFormViewerViewModel)_view.DataContext;
            var isCtrlDown =  (CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
           
            if (!isCtrlDown) {


                if (dc is FreeFormViewerViewModel)
                {
                    viwerVm.ClearSelection();
                    return;
                }

                viwerVm.ClearSelection();
                viwerVm.AddSelection(dc);
            }
            else
            {
                if (dc is FreeFormViewerViewModel)
                {
                    return;
                }

                if (dc.IsSelected)
                {
                    viwerVm.RemoveSelection(dc);
                }
                else
                {
                    viwerVm.AddSelection(dc);
                }               

            }
        }

        private async void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _released = true;
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _doubleTapped = true;
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if ((dc is ElementViewModel || dc is LinkViewModel) && !(dc is FreeFormViewerViewModel) )
            {
                if (dc is ElementViewModel)
                {
                    var vm = (ElementViewModel)dc;

                    if (vm.ElementType == ElementType.Word || vm.ElementType == ElementType.Powerpoint)
                    {
                        SessionController.Instance.SessionView.OpenFile(vm);
                    }
                    else
                    {
                        SessionController.Instance.SessionView.ShowFullScreen((ElementModel)vm.Model);
                    }

                }
                if (dc is LinkViewModel)
                {
                    var vm = (LinkViewModel)dc;
                    SessionController.Instance.SessionView.ShowFullScreen((LinkModel)vm.Model);
                }
               // vm.ToggleSelection();
                //e.Handled = true;
            }   
        }
    }
}
