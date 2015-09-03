using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Views.Workspace;

namespace NuSysApp
{
    class MultiSelectMode : AbstractWorkspaceViewMode
    {

        private bool _isMouseDown;
        private Point _startPoint;
        private Point _currentPoint;
        private Point _previousPoint;
        private Rectangle _currentRect;

        public MultiSelectMode(WorkspaceView view) : base(view)
        {
        }

        public override async Task Activate()
        {
            _view.PointerPressed += View_PointerPressed;
            _view.PointerMoved += View_PointerMoved;
            _view.PointerReleased += View_PointerReleased;
            _view.DoubleTapped += View_OnDoubleTapped;

            _view.MultiMenu.Visibility = Visibility.Visible;
            _view.MultiMenu.Delete.Click += Delete_OnClick;
            _view.MultiMenu.Group.Click += Group_OnClick;
        }

        public override async Task Deactivate()
        {
            _view.PointerPressed -= View_PointerPressed;
            _view.PointerMoved -= View_PointerMoved;
            _view.PointerReleased -= View_PointerReleased;
            _view.DoubleTapped -= View_OnDoubleTapped;

            _view.MultiMenu.Visibility = Visibility.Collapsed;
            _view.MultiMenu.Delete.Click -= Delete_OnClick;
            _view.MultiMenu.Group.Click -= Group_OnClick;
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.ClearMultiSelection();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.DeleteMultiSelecttion();
            _view.FloatingMenu.SetActive(Options.Select);
        }

        private void Group_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.GroupFromMultiSelection();
        }

        private async void View_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            _previousPoint = new Point(-1, -1);
            _isMouseDown = true;
            _startPoint = e.GetCurrentPoint(_view).Position;
            _currentPoint = e.GetCurrentPoint(_view).Position;
            _view.InqCanvas.CapturePointer(e.Pointer);
        }

        private void View_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc is NodeViewModel)
            {
                var vm = (NodeViewModel)dc;
                vm.ToggleSelection();
            }

            _isMouseDown = false;

            e.Handled = true;
        }

        private async void View_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isMouseDown)
            {
                _previousPoint = _currentPoint;
                _currentPoint = e.GetCurrentPoint(_view).Position;
                UpdateVisableRect();
            }
        }

        private async void View_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isMouseDown = false;
            _view.InqCanvas.ReleasePointerCaptures();
            SelectContainedComponents();
            _view.InqCanvas.Children.Remove(_currentRect);
        }

        private void UpdateVisableRect()
        {
            if (_currentRect != null)
            {
                _view.InqCanvas.Children.Remove(_currentRect);
            }
            _currentRect = new Rectangle();
            var vm = (WorkspaceViewModel)_view.DataContext;
            Rect transRect = vm.CompositeTransform.Inverse.TransformBounds(new Rect(_startPoint, _currentPoint));
            _currentRect.Width = transRect.Width;
            _currentRect.Height = transRect.Height;
            _currentRect.Stroke = new SolidColorBrush(Colors.Black);
            var startP = vm.CompositeTransform.Inverse.TransformPoint(_startPoint);
            _view.InqCanvas.Children.Add(_currentRect);
            Canvas.SetTop(_currentRect, transRect.Y);
            Canvas.SetLeft(_currentRect, transRect.X);
        }

        private async void SelectContainedComponents()
        {
            if (_currentRect == null)
                return;
            var vm = (WorkspaceViewModel)_view.DataContext;
            Rect r = vm.CompositeTransform.Inverse.TransformBounds(new Rect(_startPoint, _currentPoint));
            //foreach (UIElement element in _view.InqCanvas.Children)
            //{
            //    var line = element as InqLine;
            //    if (line != null)
            //    {
            //        foreach (Point p in line.Points)
            //        {
            //            if (r.Contains(p))
            //            {
            //                if (!line.IsSelected)
            //                {
            //                    selected.Add(line);
            //                }
            //                break;
            //            }
            //        }
            //    }
            //}

            var atoms = vm.AtomViewList;
            foreach (var atom in atoms)
            {
                var atomPoint = atom.TransformToVisual(_view.InqCanvas).TransformPoint(new Point(0, 0));
                var atomRect = new Rect(atomPoint.X, atomPoint.Y, atom.Width, atom.Height);
                atomRect.Intersect(r);
                if (!Double.IsInfinity(atomRect.Width) || !Double.IsInfinity(atomRect.Height))
                {
                    var avm = atom.DataContext as AtomViewModel;
                    if (!avm.IsSelected)
                    {
                        vm.SetMultiSelection(avm);
                    }
                }
            }
        }
    }
}
