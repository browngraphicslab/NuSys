using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;

namespace NuSysApp
{
    public class TouchPointCounter : AbstractWorkspaceViewMode
    {

        public static int NumTouchPoints;

        public TouchPointCounter(WorkspaceView view) :base(view)
        {
            
        }

        public async override Task Activate()
        {
            NumTouchPoints = 0;

            _view.ManipulationMode = ManipulationModes.All;
            _view.ManipulationStarting += ViewOnManipulationStarting;
            _view.ManipulationCompleted += ViewOnManipulationCompleted;
            // _view.PointerReleased += ViewOnPointerCanceled;
        }

        public async override Task Deactivate()
        {
            _view.ManipulationStarting -= ViewOnManipulationStarting;
            _view.ManipulationCompleted -= ViewOnManipulationCompleted;

        }

        private void ViewOnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs manipulationCompletedRoutedEventArgs)
        {
            NumTouchPoints--;
        }

        private void ViewOnManipulationStarting(object sender, ManipulationStartingRoutedEventArgs manipulationStartingRoutedEventArgs)
        {
            NumTouchPoints++;
        }
        
    }
}