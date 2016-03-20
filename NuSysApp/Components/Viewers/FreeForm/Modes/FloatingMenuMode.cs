using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class FloatingMenuMode : AbstractWorkspaceViewMode
    {
        public FloatingMenuMode(FreeFormViewer view) : base(view) { }

        public override async Task Activate()
        {
            _view.IsRightTapEnabled = true;
            _view.RightTapped += OnRightTapped;

            var fm = SessionController.Instance.SessionView.FloatingMenu.Panel;
            fm.ManipulationMode = ManipulationModes.All;
            fm.ManipulationDelta += OnManipulationDelta;

        }

        public override async Task Deactivate()
        {
            _view.IsRightTapEnabled = false;
            _view.RightTapped-= OnRightTapped;
            var fm = SessionController.Instance.SessionView.FloatingMenu.Panel;
            fm.ManipulationMode = ManipulationModes.None;
            fm.ManipulationDelta -= OnManipulationDelta;
        }

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var os = (FrameworkElement) e.OriginalSource;

            var compositeTransform = (CompositeTransform)SessionController.Instance.SessionView.FloatingMenu.RenderTransform;
            compositeTransform.TranslateX += e.Delta.Translation.X;
            compositeTransform.TranslateY += e.Delta.Translation.Y;

            e.Handled = true;
        }

        protected void OnRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)sender).DataContext;
            if (!(dc is FreeFormViewerViewModel))
            {
                e.Handled = true;
                return;
            }

            var p = e.GetPosition(null);
            var t = (CompositeTransform)SessionController.Instance.SessionView.FloatingMenu.RenderTransform;
            t.TranslateX = p.X - 70;
            t.TranslateY = p.Y - 100;
            e.Handled = true;
        }
    }
}
