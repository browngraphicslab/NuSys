using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Views.Workspace
{
    public class SelectMode : AbstractWorkspaceViewMode
    {
        public SelectMode(WorkspaceView view) : base(view) { }

        public override void Activate()
        {
            _view.IsDoubleTapEnabled = true;
            _view.DoubleTapped += OnDoubleTapped;
            _view.PointerPressed += OnPointerPressed;

            _view.ManipulationMode = ManipulationModes.All;
           // _view.ManipulationDelta += OnManipulationDelta;
        }

        public override void Deactivate()
        {
            _view.IsDoubleTapEnabled = false;
            _view.PointerPressed -= OnPointerPressed;
            _view.DoubleTapped -= OnDoubleTapped;

            _view.ManipulationMode = ManipulationModes.None;
           // _view.ManipulationDelta -= OnManipulationDelta;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc is WorkspaceViewModel) { 
                var vm = (WorkspaceViewModel)_view.DataContext;
                vm.ClearSelection();
            }
            e.Handled = true;
        }

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            try { 
                var dc = ((FrameworkElement)e.OriginalSource).DataContext;
                Debug.WriteLine(dc);
                var vm = (NodeViewModel)dc;
                vm.ToggleSelection();
            } catch(Exception ex)
            {
                Debug.WriteLine("canvas clicked");
            }
            e.Handled = true;
        }

    }
}
