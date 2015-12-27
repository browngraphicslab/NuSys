using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class NodeManipulationMode : AbstractWorkspaceViewMode
    {

        private bool _isPinAnimating;

        public NodeManipulationMode(WorkspaceView view) : base(view) { }

        public override async Task Activate()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            foreach (var userControl in vm.AtomViewList)
            {
                userControl.ManipulationMode = ManipulationModes.All;
                userControl.ManipulationDelta += OnManipulationDelta;
                userControl.ManipulationStarting += delegate(object sender, ManipulationStartingRoutedEventArgs args)
                {
                    Debug.WriteLine("Starting!");
                };
            }
        }

        public override async Task Deactivate()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            foreach (var userControl in vm.AtomViewList)
            {
                userControl.ManipulationMode = ManipulationModes.All;
                userControl.ManipulationDelta -= OnManipulationDelta;
            }
        }



        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;
            var s = (UserControl) sender;
            var vm = (NodeViewModel) s.DataContext;
            vm.Translate(e.Delta.Translation.X, e.Delta.Translation.Y);
            // e.Handled = true;

        }
    }
}
