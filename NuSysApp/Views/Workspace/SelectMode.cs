using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace NuSysApp.Views.Workspace
{
    public class SelectMode : AbstractWorkspaceViewMode
    {
        public SelectMode(WorkspaceView view) : base(view) { }

        public override void Activate()
        {
            _view.PointerPressed += OnPointerPressed;
        }

        public override void Deactivate()
        {
            _view.PointerPressed -= OnPointerPressed;
        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.ClearSelection();
        }
    }
}
