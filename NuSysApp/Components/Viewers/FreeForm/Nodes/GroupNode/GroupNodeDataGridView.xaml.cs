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

        private void ControllerOnDisposed(object source, object args)
        {
            var vm = (ElementViewModel) DataContext;
            SessionController.Instance.SessionView?.MainCanvas?.RemoveHandler(UIElement.PointerReleasedEvent, _releaseHandler);
            if(vm.Controller != null)
            {
                vm.Controller.Disposed -= ControllerOnDisposed;
                vm.Controller.Disposed -= ControllerOnDisposed;
            }
            DataContext = null;
        }

        private Image _drag;
        private String _id;
        private void OnPointerPressed(object source, PointerRoutedEventArgs args)
        {
            var src = args.OriginalSource as FrameworkElement;
            var gridInfo = src?.DataContext as GroupNodeDataGridInfo;
            if (gridInfo != null)
            {
                src.ManipulationMode = ManipulationModes.All; // for dragging out via touch
                _drag = new Image();//TODO temporary
                var itemController = SessionController.Instance.IdToControllers[gridInfo?.Id].LibraryElementController;
                BitmapImage textimage = new BitmapImage(itemController.SmallIconUri);
                _drag.Source = textimage;

                var point = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
                Canvas.SetLeft(_drag, point.X);
                Canvas.SetTop(_drag, point.Y);
                SessionController.Instance.SessionView.MainCanvas.Children.Add(_drag);
               
                
                _id = gridInfo.Id;
            }
        }

        private async void OnPointerReleased(object source, PointerRoutedEventArgs args)
        {

            var point = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            if (!this.IsPointerInGroup(point))
            {
                var newPos = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.Inverse.TransformPoint(point);
                Debug.Assert(newPos != null);

                // safe check if the id is in IdToControllers before requesting to move it to the current collection
                ElementController controller;
                SessionController.Instance.IdToControllers.TryGetValue(_id ?? "", out controller);
                if (controller != null)
                {
                    await controller.RequestMoveToCollection(SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId, newPos.X, newPos.Y);
                }
            }

            // rmeove the _drag image from the canvas, and reset private variables for dragging element _id and image
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
            // get the data type of the list item template
            var groupNodeDataGridInfo = (e.OriginalSource as FrameworkElement)?.DataContext as GroupNodeDataGridInfo;

            // if groupNodeDataGridInfo is null, double click did not occur on item so return
            if (groupNodeDataGridInfo == null)
            {
                return;
            }

            // the list item template has an element controller id, use that to get the library element Model ContentId
            ElementController elementController;
            SessionController.Instance.IdToControllers.TryGetValue(groupNodeDataGridInfo.Id, out elementController);
            var libraryElementModelId = elementController?.LibraryElementModel.LibraryElementId;

            // get the controller from the data type
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementModelId);

            // return if the controller is null
            Debug.Assert(controller != null);

            // open the detail viewer
            SessionController.Instance.NuSessionView.ShowDetailView(controller);
        }

    }
}
