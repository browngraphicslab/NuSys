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

        //these are used to update the displayed rectangle
        private Point _startPoint;
        private Point _currentPoint;

        //this is used to adjust the inq points to the correct values for the node
        private Point _canvasStartPoint;

        private Rectangle _currentRect;

        public MultiSelectMode(WorkspaceView view) : base(view)
        {
        }

        public override async Task Activate()
        {
            
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.ClearMultiSelection();
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

            _view.MultiMenu.Visibility = Visibility.Collapsed;
            _view.MultiMenu.Delete.Click -= Delete_OnClick;
            _view.MultiMenu.Group.Click -= Group_OnClick;
       //     var vm = (WorkspaceViewModel)_view.DataContext;
      //      vm.ClearMultiSelection();
        }

        private void Delete_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.DeleteMultiSelecttion();
            _view.SwitchMode(Options.SelectNode, false);
        }

        private void Group_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.GroupFromMultiSelection();
            _view.SwitchMode(Options.SelectNode, false);
        }

        private async void View_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
                _view.MultiMenu.Delete.Click -= Delete_OnClick;
                _view.MultiMenu.Group.Click -= Group_OnClick;
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            if (dc is NodeViewModel)
            {
                return;
            }
            _isMouseDown = true;
            _startPoint = e.GetCurrentPoint(_view).Position;
            _currentPoint = e.GetCurrentPoint(_view).Position;
            _canvasStartPoint = e.GetCurrentPoint(_view.InqCanvas).Position;
            _view.InqCanvas.CapturePointer(e.Pointer);
        }

        private void View_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var dc = ((FrameworkElement)e.OriginalSource).DataContext;
            var nvm = dc as NodeViewModel;
            if (nvm != null)
            {
                var vm = (WorkspaceViewModel)_view.DataContext;
                vm.SetMultiSelection(nvm);
                _view.MultiMenu.Visibility = Visibility.Visible;
                _view.MultiMenu.Delete.Click += Delete_OnClick;
                _view.MultiMenu.Group.Click += Group_OnClick;
            }

            _isMouseDown = false;

            e.Handled = true;
        }

        private async void View_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_isMouseDown)
            {
                _currentPoint = e.GetCurrentPoint(_view).Position;
                UpdateVisableRect();
            }
        }

        private async void View_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            _isMouseDown = false;
            _view.InqCanvas.ReleasePointerCaptures();
            SelectContainedComponents();
            // TODO: add again
           // _view.InqCanvas.Children.Remove(_currentRect);
        }

        private void UpdateVisableRect()
        {
            if (_currentRect != null)
            {
                // TODO: add again
               
                //_view.InqCanvas.Children.Remove(_currentRect);
            }
            _currentRect = new Rectangle();
            var vm = (WorkspaceViewModel)_view.DataContext;
            Rect transRect = vm.CompositeTransform.Inverse.TransformBounds(new Rect(_startPoint, _currentPoint));
            _currentRect.Width = transRect.Width;
            _currentRect.Height = transRect.Height;
            _currentRect.Stroke = new SolidColorBrush(Colors.Black);
            var startP = vm.CompositeTransform.Inverse.TransformPoint(_startPoint);

            // TODO: add again
           // _view.InqCanvas.Children.Add(_currentRect);
            Canvas.SetTop(_currentRect, transRect.Y);
            Canvas.SetLeft(_currentRect, transRect.X);
        }

        private async void SelectContainedComponents()
        {
            var vm = (WorkspaceViewModel)_view.DataContext;
            vm.ClearMultiSelection();
            if (_currentRect == null)
                return;
            Rect r = vm.CompositeTransform.Inverse.TransformBounds(new Rect(_startPoint, _currentPoint));


            foreach (var atom in vm.Children.Values)
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

            if (vm.MultiSelectedAtomViewModels.Count > 0)
            {
                Canvas.SetLeft(_view.MultiMenu, _startPoint.X);
                Canvas.SetTop(_view.MultiMenu, _startPoint.Y);
                _view.MultiMenu.Visibility = Visibility.Visible;
                _view.MultiMenu.Delete.Click += Delete_OnClick;
                _view.MultiMenu.Group.Click += Group_OnClick;
            }
            else
            {
                var selectedLines = new List<InqLineModel>();
                Point topLeft = new Point(r.X, r.Y);
                foreach (InqLineModel model in _view.InqCanvas.ViewModel.Model.Lines)
                {
                    InqLineModel newModel = new InqLineModel(DateTime.UtcNow.Ticks.ToString());
                    newModel.Stroke = model.Stroke;
                    newModel.StrokeThickness = model.StrokeThickness;
                    bool isContained = false;
                    foreach (var point in model.Points)
                    {
                        //we need to adjust the point so that it is in the correct place on the node's canvas
                        newModel.AddPoint(new Point2d(point.X - topLeft.X, point.Y - topLeft.Y));
                        if (!isContained && r.Contains(point))
                        {
                            isContained = true;
                            NetworkConnector.Instance.RequestDeleteSendable(model.Id);
                            selectedLines.Add(newModel);
                        }
                    }
                }
                if (selectedLines.Count > 0)
                {
                    vm.PromoteInk(r, selectedLines);
                }
            }
        }
    }
}
