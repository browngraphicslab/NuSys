using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Views.Workspace
{
    public class FloatingMenuMode : AbstractWorkspaceViewMode
    {
        public FloatingMenuMode(WorkspaceView view) : base(view) { }

        public override void Activate()
        {
            _view.IsDoubleTapEnabled = true;
            _view.DoubleTapped += OnDoubleTapped;
        }

        public override void Deactivate()
        {
            _view.IsDoubleTapEnabled = false;
            _view.DoubleTapped -= OnDoubleTapped;
        }

        protected void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            var floatingMenuTransform = new CompositeTransform();

            var p = e.GetPosition(_view);
            floatingMenuTransform.TranslateX = p.X;
            floatingMenuTransform.TranslateY = p.Y;
            vm.FMTransform = floatingMenuTransform;

            // FM.Visibility = FM.Visibility == Visibility.Collapsed ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
