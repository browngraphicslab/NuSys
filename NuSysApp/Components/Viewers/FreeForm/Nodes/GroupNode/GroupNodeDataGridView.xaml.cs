using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

namespace NuSysApp
{
    public sealed partial class GroupNodeDataGridView : AnimatableUserControl
    {
        private DispatcherTimer _timer;
        private PointerEventHandler _releaseHandler;

        public GroupNodeDataGridView(GroupNodeDataGridViewModel viewModel)
        {
           DataContext = viewModel;
           this.InitializeComponent();

            _releaseHandler = new PointerEventHandler(OnPointerReleased);
            DataGrid.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(OnPointerPressed), true );
            DataGrid.AddHandler(UIElement.ManipulationDeltaEvent, new ManipulationDeltaEventHandler(OnManipulationDelta), true);
            DataGrid.AddHandler(UIElement.PointerReleasedEvent, new PointerEventHandler(OnPointerReleased), true);
            DataGrid.ManipulationMode = ManipulationModes.All;
            SessionController.Instance.SessionView.MainCanvas.AddHandler(UIElement.PointerReleasedEvent, _releaseHandler, true);
            DataGrid.AddHandler(UIElement.DoubleTappedEvent, new DoubleTappedEventHandler(OnDoubleTapped), true);
            DataGrid.SelectedItem = null;
            DataGrid.SelectionChanged += DataGridOnSelectionChanged;
            viewModel.Controller.Disposed += ControllerOnDisposed;
        }

        private void DataGridOnSelectionChanged(object sender, SelectionChangedEventArgs selectionChangedEventArgs)
        {
            DataGrid.SelectedItem = null;
        }

        private void ControllerOnDisposed(object source)
        {
            var vm = (ElementViewModel) DataContext;
            SessionController.Instance.SessionView.MainCanvas.RemoveHandler(UIElement.PointerReleasedEvent, _releaseHandler);
            vm.Controller.Disposed -= ControllerOnDisposed;
            DataContext = null;
            vm.Controller.Disposed -= ControllerOnDisposed;
        }

        private Image _drag;
        private String _id;
        private void OnPointerPressed(object source, PointerRoutedEventArgs args)
        {
            var src = (FrameworkElement) args.OriginalSource;
            if (src.DataContext is GroupNodeDataGridInfo)
            {
                
                _drag = new Image();//TODO temporary
                BitmapImage textimage = new BitmapImage(new Uri("ms-appx:///Assets/icon_new_workspace.png", UriKind.Absolute));
                _drag.Source = textimage;

                var point = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
                Canvas.SetLeft(_drag, point.X);
                Canvas.SetTop(_drag, point.Y);
                SessionController.Instance.SessionView.MainCanvas.Children.Add(_drag);
               
                
                var info = (GroupNodeDataGridInfo) src.DataContext;
                _id = info.Id;
            }
        }

        private async void OnPointerReleased(object source, PointerRoutedEventArgs args)
        {
            if (_id == null)
                return;

            var point = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            if (!this.IsPointerInGroup(point))
            {
                var newPos = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(point);
                var controller = SessionController.Instance.IdToControllers[_id];
                await controller.RequestMoveToCollection(WaitingRoomView.InitialWorkspaceId, newPos.X, newPos.Y);
            }
            SessionController.Instance.SessionView.MainCanvas.Children.Remove(_drag);
            _drag = null;
            _id = null;
            
        }
   

        private bool IsPointerInGroup(Point point)
        {
            var hits = VisualTreeHelper.FindElementsInHostCoordinates(point, SessionController.Instance.SessionView);
            var result = hits.Where((uiElem) => uiElem is GroupNodeDataGridView);
            return result.Any();
        }
        private FrameworkElement _el;
        private bool _doubleTapped;

        private void OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs args)
        {

            if (_drag != null)
            {
                var x = Canvas.GetLeft(_drag);
                var y = Canvas.GetTop(_drag);
                x += args.Delta.Translation.X;
                y += args.Delta.Translation.Y;

                Canvas.SetLeft(_drag, x);
                Canvas.SetTop(_drag, y);
            }
         }
       

        private void OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            _doubleTapped = true;
            var dc = (e.OriginalSource as FrameworkElement)?.DataContext;
        
            if (dc is GroupNodeDataGridInfo)
            {
                var cdc = (GroupNodeDataGridInfo) dc;
                var controller = SessionController.Instance.IdToControllers[cdc.Id];
                var type = controller.LibraryElementModel.Type;

                    if (type == ElementType.Word || type == ElementType.Powerpoint)
                    {
                        return;
                    }
                    else if (type != ElementType.Link)
                    {
                        SessionController.Instance.SessionView.ShowDetailView((dc as ElementViewModel).Controller.LibraryElementController);
                    }

                
            }
        }

    }
}
