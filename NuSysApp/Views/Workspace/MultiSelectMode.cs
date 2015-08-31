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
            var vm = (WorkspaceViewModel)_view.DataContext;
        }

        public override async Task Activate()
        {
            _view.PointerPressed += View_PointerPressed;
            _view.PointerMoved += View_PointerMoved;
            _view.PointerReleased += View_PointerReleased;
            _view.DoubleTapped += View_OnDoubleTapped;
        }

        public override async Task Deactivate()
        {
            _view.PointerPressed -= View_PointerPressed;
            _view.PointerMoved -= View_PointerMoved;
            _view.PointerReleased -= View_PointerReleased;
            _view.DoubleTapped -= View_OnDoubleTapped;
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
            _currentRect.Width = Math.Abs(_currentPoint.X - _startPoint.X);
            _currentRect.Height = Math.Abs(_currentPoint.Y - _startPoint.Y);
            _currentRect.Stroke = new SolidColorBrush(Colors.Black);
            var vm = (WorkspaceViewModel)_view.DataContext;
            var startP = vm.CompositeTransform.Inverse.TransformPoint(_startPoint);
            var currentP = vm.CompositeTransform.Inverse.TransformPoint(_currentPoint);
            _view.InqCanvas.Children.Add(_currentRect);
            if (_currentPoint.X > _startPoint.X)
            {
                if (_currentPoint.Y > _startPoint.Y)
                {
                    Canvas.SetTop(_currentRect, startP.Y);
                    Canvas.SetLeft(_currentRect, startP.X);
                }
                else
                {
                    Canvas.SetTop(_currentRect, currentP.Y);
                    Canvas.SetLeft(_currentRect, startP.X);
                }
            }
            else
            {
                if (_currentPoint.Y > _startPoint.Y)
                {
                    Canvas.SetTop(_currentRect, startP.Y);
                    Canvas.SetLeft(_currentRect, currentP.X);
                }
                else
                {
                    Canvas.SetTop(_currentRect, currentP.Y);
                    Canvas.SetLeft(_currentRect, currentP.X);
                }
            }
        }

        private async void SelectContainedComponents()
        {
            if (_currentRect == null)
                return;
            Rect r = new Rect();
            r.Width = _currentRect.Width;
            r.Height = _currentRect.Height;
            var vm = (WorkspaceViewModel)_view.DataContext;
            var startP = vm.CompositeTransform.Inverse.TransformPoint(_startPoint);
            var currentP = vm.CompositeTransform.Inverse.TransformPoint(_startPoint);
            if (currentP.X > startP.X)
            {
                if (currentP.Y > startP.Y)
                {
                    r.X = startP.X;
                    r.Y = startP.Y;
                }
                else
                {
                    r.Y = currentP.Y;
                    r.X = startP.X;
                }
            }
            else
            {
                if (_currentPoint.Y > _startPoint.Y)
                {
                    r.Y = startP.Y;
                    r.X = currentP.X;
                }
                else
                {
                    r.X = currentP.X;
                    r.Y = currentP.Y;
                }
            }
            List<ISelectable> selected = new List<ISelectable>();
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
                        selected.Add(avm);
                    }
                }
            }
        }
    }
}
