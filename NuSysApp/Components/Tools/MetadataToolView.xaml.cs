using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class MetadataToolView : AnimatableUserControl
    {
        private Image _dragItem;
        private enum DragMode { Collection, Key, Value, Scroll };

        private DragMode _currentDragMode = DragMode.Key;

        private const int ListBoxHeightOffset = 175;

        private const int _minHeight = 200;

        private const int _minWidth = 200;

        private double _x;

        private double _y;

        public MetadataToolView(MetadataToolViewModel vm, double x, double y)
        {
            this.InitializeComponent();
            _dragItem = vm.InitializeDragFilterImage();
            vm.Controller.SetLocation(x, y);
            this.DataContext = vm;
            SetSize(400,500);
            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationCompleted), true);
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            (vm.Controller as MetadataToolController).SelectionChanged += On_SelectionChanged;
            vm.Controller.NumberOfParentsChanged += Controller_NumberOfParentsChanged;

            vm.ReloadPropertiesToDisplay();
            xMetadataKeysList.ItemsSource = (DataContext as MetadataToolViewModel)?.AllMetadataDictionary.Keys;

        }

        private void On_SelectionChanged(object sender)
        {
            var vm = DataContext as MetadataToolViewModel;
            if (vm.Selection != null &&
                (vm.Controller as MetadataToolController).Model.Selected &&
                vm.Selection.Item1 != null)
            {
                xMetadataValuesList.ItemsSource =
                       vm.AllMetadataDictionary[vm.Selection.Item1];
                xMetadataKeysList.SelectedItem = vm.Selection.Item1;
                xMetadataKeysList.ScrollIntoView(xMetadataKeysList.SelectedItem);

                if (vm.Selection.Item2 != null)
                {
                    xMetadataValuesList.SelectedItem = vm.Selection.Item2;
                    xMetadataValuesList.ScrollIntoView(xMetadataValuesList.SelectedItem);
                }
                else
                {
                    xMetadataValuesList.SelectedItem = null;
                }
            }
            else
            {
                xMetadataKeysList.SelectedItem = null;
                xMetadataValuesList.ItemsSource = new List<string>();
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            (DataContext as ToolViewModel)?.Dispose();
            this.Dispose();
        }

        public void Dispose()
        {
            (DataContext as MetadataToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
            ((DataContext as MetadataToolViewModel).Controller as MetadataToolController).SelectionChanged -= On_SelectionChanged;
            (DataContext as MetadataToolViewModel).Controller.NumberOfParentsChanged -= Controller_NumberOfParentsChanged;

        }

        private void Controller_NumberOfParentsChanged(int numOfParents)
        {
            //xParentOperatorPickerList.Visibility = Visibility.Visible;
            //xViewTypeGrid.Visibility = Visibility.Collapsed;
            if (numOfParents > 1)
            {
                xParentOperatorGrid.Visibility = Visibility.Visible;
            }
            else
            {
                xParentOperatorGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void XParentOperatorText_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = DataContext as ToolViewModel;
            if (vm.Controller.Model.ParentOperator == ToolModel.ParentOperatorType.And)
            {
                vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.Or);
                xParentOperatorText.Text = "OR";
            }
            else if (vm.Controller.Model.ParentOperator == ToolModel.ParentOperatorType.Or)
            {
                vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.And);
                xParentOperatorText.Text = "AND";
            }
        }

        private void Vm_PropertiesToDisplayChanged()
        {
            var vm = DataContext as MetadataToolViewModel;
            Debug.Assert(vm != null);
            xMetadataKeysList.ItemsSource = vm.AllMetadataDictionary.Keys;
        }

        private async void CollectionBtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {

            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);

            var button = (Button)sender;
            button.Focus(FocusState.Pointer);

            CapturePointer(args.Pointer);

            if (sender == xCollectionElement)
            {
                _currentDragMode = MetadataToolView.DragMode.Collection;
            }

            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            _dragItem = new Image();
            _dragItem.Source = bmp;
            _dragItem.Width = 50;
            _dragItem.Height = 50;
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            (sender as FrameworkElement).AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationDelta), true);
            args.Handled = true;
        }

        private void CollectionBtnAddOnManipulationDelta(object sender, PointerRoutedEventArgs args)
        {
            if (_dragItem == null)
                return;
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var p = args.GetCurrentPoint(xCanvas).Position;
            t.TranslateX = p.X - _dragItem.ActualWidth / 2;
            t.TranslateY = p.Y - _dragItem.ActualHeight / 2;
            args.Handled = true;
        }

        private async void CollectionBtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;
            if (_currentDragMode == DragMode.Collection)
            {
                var vm = DataContext as ToolViewModel;
                if (vm != null)
                {
                    vm.CreateCollection(r.X, r.Y);
                }
            }
            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationDelta));
            args.Handled = true;
        }

        private void Tool_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Tool_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as ToolViewModel;
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var x = e.Delta.Translation.X / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var y = e.Delta.Translation.Y / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;
            if (vm != null)
            {
                vm.Controller.SetLocation(vm.X + x, vm.Y + y);
            }
        }

        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var vm = (ToolViewModel)this.DataContext;
            var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var resizeX = vm.Width + e.Delta.Translation.X / zoom;
            var resizeY = vm.Height + e.Delta.Translation.Y / zoom;
            if (resizeX > MinWidth && resizeY > MinHeight)
            {
                SetSize(resizeX, resizeY);

            }
            else if (resizeX > MinWidth)
            {
                SetSize(resizeX, vm.Height);
            }
            else if (resizeY > MinHeight)
            {
                SetSize(vm.Width, resizeY);
            }
        }

        private void SetSize(double width, double height)
        {
            if (width < _minWidth && height < _minHeight)
            {
                return;
            }
            var vm = (DataContext as MetadataToolViewModel);
            if (width > _minWidth && height > _minHeight)
            {
                vm.Controller.SetSize(width, height);
                xMetadataKeysList.Height = height - ListBoxHeightOffset;
                xMetadataValuesList.Height = height - ListBoxHeightOffset;
                xMetadataKeysList.Width = width / 2;
                xMetadataValuesList.Width = width / 2;
            }
            else if (height < _minHeight)
            {
                vm.Controller.SetSize(width, this.Height);
                xMetadataKeysList.Width = width / 2;
                xMetadataValuesList.Width = width / 2;
            }
            else if (width < _minWidth)
            {
                vm.Controller.SetSize(this.Width, height);
                xMetadataKeysList.Height = height - ListBoxHeightOffset;
                xMetadataValuesList.Height = height - ListBoxHeightOffset;
            }
        }


        

        private void xList_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _x = e.GetCurrentPoint(xCanvas).Position.X - 25;
            _y = e.GetCurrentPoint(xCanvas).Position.Y - 25;
            e.Handled = true;
        }

        private async void xListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);
            if (_currentDragMode == DragMode.Collection)
            {
                _dragItem = (DataContext as ToolViewModel).InitializeDragFilterImage();
            }
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX = _x;
            t.TranslateY = _y;
        }

        private void xListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            ListView list = new ListView();
            if (_currentDragMode == DragMode.Key)
            {
                list = xMetadataKeysList;
            }
            else if (_currentDragMode == DragMode.Value)
            {
                list = xMetadataValuesList;
            }
            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(list).TransformPoint(e.Position);
            if (sp.X < list.ActualWidth && sp.X > 0 && sp.Y > 0 && sp.Y < list.ActualHeight)
            {
                Border border = (Border)VisualTreeHelper.GetChild(list, 0);
                ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta.Translation.Y);
                }
                if (_dragItem.Visibility == Visibility.Visible)
                {
                    _dragItem.Visibility = Visibility.Collapsed;
                }
            }
            else if (_dragItem.Visibility == Visibility.Collapsed && !e.IsInertial)
            {
                _dragItem.Visibility = Visibility.Visible;
            }
            if ((_dragItem.RenderTransform as CompositeTransform) != null)
            {

                var t = (CompositeTransform)_dragItem.RenderTransform;
                var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;

                var p = e.Position;
                t.TranslateX += e.Delta.Translation.X / zoom;
                t.TranslateY += e.Delta.Translation.Y / zoom;
            }
        }

        private async void xListItem_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            xCanvas.Children.Remove(_dragItem);
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(sp.X, sp.Y, 300, 300));

            if (_dragItem.Visibility == Visibility.Visible)
            {
                var vm = (DataContext as MetadataToolViewModel);
                if (_currentDragMode == DragMode.Key)
                {
                    vm.Selection = new Tuple<string, string>((((Grid)sender).Children[0] as TextBlock).Text, null);
                }
                else if (_currentDragMode == DragMode.Value)
                {
                    vm.Selection = new Tuple<string, string>(vm.Selection.Item1, (((Grid)sender).Children[0] as TextBlock).Text);
                }
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);
                vm.FilterIconDropped(hitsStart, wvm, r.X, r.Y);
            }
        }

        private void XList_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (sender == xMetadataKeysList)
            {
                _currentDragMode = DragMode.Key;
            }
            else if (sender == xMetadataValuesList)
            {
                _currentDragMode = DragMode.Value;
            }
        }

        private void KeyListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (DataContext as MetadataToolViewModel);
            if (vm.Controller.Model.Selected &&
                vm.Selection.Item1.Equals(
                    ((sender as Grid).Children[0] as TextBlock).Text))
            {
                vm.Controller.UnSelect();
            }
            else
            {
                Debug.Assert(vm != null);
                vm.Selection = new Tuple<string, string>(((sender as Grid).Children[0] as TextBlock).Text, null);
            }
        }

        private void ValueListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (DataContext as MetadataToolViewModel);
            if (vm.Controller.Model.Selected && vm.Selection.Item2 != null &&
                vm.Selection.Item2.Equals(
                    ((sender as Grid).Children[0] as TextBlock).Text))
            {
                vm.Selection = new Tuple<string, string>(vm.Selection.Item1, null);
            }
            else
            {
                Debug.Assert(vm != null);
                if (xMetadataKeysList.SelectedItems.Count == 1)
                {
                    vm.Selection = new Tuple<string, string>(vm.Selection.Item1, ((sender as Grid).Children[0] as TextBlock).Text);
                }
            }
        }

        private void ValueListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (DataContext as MetadataToolViewModel);
            if (vm.Selection.Item2 != (((sender as Grid).Children[0] as TextBlock).Text))
            {
                vm.Selection = new Tuple<string, string>(vm.Selection.Item1, ((sender as Grid).Children[0] as TextBlock).Text);
            }
            vm.OpenDetailView();
        }
    }
}
