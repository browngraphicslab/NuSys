﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
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
    public sealed partial class MetadataToolView : AnimatableUserControl, ITool
    {
        private Image _dragItem;
        private enum DragMode { Collection, Key, Value, Scroll };

        private DragMode _currentDragMode = DragMode.Key;

        private const int ListBoxHeightOffset = 175;

        private const int _minHeight = 200;

        private const int _minWidth = 200;


        private double _x;

        private double _y;

        public event EventHandler<Point2d> ToolAnchorChanged;

        public MetadataToolView(MetadataToolViewModel vm, double x, double y)
        {
            this.InitializeComponent();
            _dragItem = vm.InitializeDragFilterImage();
            //vm.Controller.SetLocation(x, y);
            xFilterComboBox.ItemsSource = Enum.GetValues(typeof(ToolModel.ToolFilterTypeTitle)).Cast<ToolModel.ToolFilterTypeTitle>();
            xFilterComboBox.SelectedItem = vm.Filter;
            this.DataContext = vm;
            SetSize(400,500);
            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationCompleted), true);

            xStackElement.AddHandler(PointerPressedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationStarting), true);
            xStackElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationCompleted), true);

            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            (vm.Controller as MetadataToolController).SelectionChanged += On_SelectionChanged;
            vm.Controller.NumberOfParentsChanged += Controller_NumberOfParentsChanged;

            vm.ReloadPropertiesToDisplay();
            xMetadataKeysList.ItemsSource = (DataContext as MetadataToolViewModel)?.AllMetadataDictionary.Keys;
        }

        /// <summary>
        ///Set the key list visual selection and refresh the value list
        /// </summary>
        private void On_SelectionChanged(object sender)
        {
            SetKeyListVisualSelection();
            RefreshValueList();
        }

        /// <summary>
        ///Set the key list visual selection and scrolls to include in view
        /// </summary>
        private void SetKeyListVisualSelection()
        {
            UITask.Run(delegate
            {
                var vm = DataContext as MetadataToolViewModel;
                if (vm.Selection != null &&
                    (vm.Controller as MetadataToolController).ToolModel.Selected &&
                    vm.Selection.Item1 != null)
                {
                    if (xMetadataKeysList.SelectedItem != vm.Selection.Item1)
                    {
                        xMetadataKeysList.SelectedItem = vm.Selection.Item1;
                        xMetadataKeysList.ScrollIntoView(xMetadataKeysList.SelectedItem);
                    }
                }
                else
                {
                    xMetadataKeysList.SelectedItem = null;
                }
            });
            
        }
        

        /// <summary>
        ///Set the values list visual selection
        /// </summary>
        private void SetValueListVisualSelection()
        {
            UITask.Run(delegate
            {
                var vm = DataContext as MetadataToolViewModel;
                if (vm.Selection.Item1 != null && vm.Selection.Item2 != null)
                {
                    xMetadataValuesList.SelectedItems.Clear();
                    if (xMetadataValuesList.Items.Any())
                    {
                        foreach (var item in vm.Selection.Item2)
                        {
                            var toAdd = xMetadataValuesList.Items.Where(kvp => ((KeyValuePair<string, double>)kvp).Key.Equals(item)).FirstOrDefault();
                            if (toAdd != null)
                            {
                                xMetadataValuesList.SelectedItems.Add(toAdd);
                            }
                        }
                    }
                }
                else
                {
                    xMetadataValuesList.SelectedItems.Clear();
                }
            });
        }

        private void XSearchBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (xMetadataValuesList.ItemsSource != null)
            {
                RefreshValueList();
            }
        }

        /// <summary>
        ///  Based on the selected key, and the search bar, refreshes the value list and sets visual value selection
        /// </summary>
        public void RefreshValueList()
        {
            UITask.Run(delegate {
                var vm = (DataContext as MetadataToolViewModel);
                if (vm?.Selection?.Item1 != null && vm.Controller.ToolModel.Selected)
                {
                    var filteredList = FilterValuesList(xSearchBox.Text);
                    if (!ScrambledEquals(xMetadataValuesList.Items.Select(item => ((KeyValuePair<string, double>)item).Key), filteredList.Select(item => ((KeyValuePair<string, double>)item).Key)))
                    {
                        //if new filtered list is different from old filtered list, set new list as item source, set the visual selection, and 
                        //scroll into view if necessary.
                        xMetadataValuesList.ItemsSource = filteredList;
                        SetValueListVisualSelection();
                        if (xMetadataValuesList.SelectedItems.Count > 0)
                        {
                            xMetadataValuesList.ScrollIntoView(xMetadataValuesList.SelectedItems.First());
                        }
                    }
                    else
                    {
                        //if new filtered list is the same as old filtered list, just set the visual selection and do not refresh the value list item source
                        SetValueListVisualSelection();
                    }
                }
                else
                {
                    xMetadataValuesList.ItemsSource = null;
                }
            });
        }

        /// <summary>
        ///  compares two lists
        /// </summary>
        public bool ScrambledEquals<T>(IEnumerable<T> list1, IEnumerable<T> list2)
        {
            if (list1 == null || list2 == null)
            {
                return false;
            }
            var cnt = new Dictionary<T, int>();
            foreach (T s in list1)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]++;
                }
                else {
                    cnt.Add(s, 1);
                }
            }
            foreach (T s in list2)
            {
                if (cnt.ContainsKey(s))
                {
                    cnt[s]--;
                }
                else {
                    return false;
                }
            }
            return cnt.Values.All(c => c == 0);
        }

        /// <summary>
        ///  Based on a search string, returns the filtered values list
        /// </summary>
        private List<KeyValuePair<string, double>> FilterValuesList(string search)
        {
            var filteredValuesList = new List<string>();
            var vm = (DataContext as MetadataToolViewModel);
            if (vm == null || !vm.AllMetadataDictionary.ContainsKey(vm.Selection.Item1))
            {
                return new List<KeyValuePair<string, double>>();
            }
            var listOfKvpMetadataValueToNumOfOccurrences = vm.AllMetadataDictionary[vm.Selection.Item1].Where(
                    item => item.Key?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            return listOfKvpMetadataValueToNumOfOccurrences.OrderByDescending(item => item.Value).ToList();

            return listOfKvpMetadataValueToNumOfOccurrences.OrderBy(key => !string.IsNullOrEmpty(key.Key) && char.IsNumber(key.Key[0]))
                    .ThenBy(key => key.Key).ToList();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            this.Dispose();
        }

        public void Dispose()
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            (DataContext as MetadataToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
            ((DataContext as MetadataToolViewModel).Controller as MetadataToolController).SelectionChanged -= On_SelectionChanged;
            (DataContext as MetadataToolViewModel).Controller.NumberOfParentsChanged -= Controller_NumberOfParentsChanged;
            (DataContext as ToolViewModel)?.Dispose();

        }

        /// <summary>
        ///If the number of parents is greater than 1, this sets the visibility of the parent operator (AND/OR) grid 
        /// </summary>
        private void Controller_NumberOfParentsChanged(int numOfParents)
        {
            if (numOfParents > 1)
            {
                xParentOperatorGrid.Visibility = Visibility.Visible;
            }
            else
            {
                xParentOperatorGrid.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        ///Sets the parent operator
        /// </summary>
        private void XParentOperatorText_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = DataContext as ToolViewModel;
            if (vm.Controller.ToolModel.ParentOperator == ToolModel.ParentOperatorType.And)
            {
                vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.Or);
                xParentOperatorText.Text = "OR";
            }
            else if (vm.Controller.ToolModel.ParentOperator == ToolModel.ParentOperatorType.Or)
            {
                vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.And);
                xParentOperatorText.Text = "AND";
            }
        }

        /// <summary>
        ///item source of metadata keys list
        /// </summary>
        private void Vm_PropertiesToDisplayChanged()
        {
            UITask.Run(delegate
            {
                var vm = DataContext as MetadataToolViewModel;
                Debug.Assert(vm != null);
                xMetadataKeysList.ItemsSource = vm.AllMetadataDictionary.Keys;
            });
        }

        /// <summary>
        ///Sets up drag image when collection or stack image is starting to be dragged.
        /// </summary>
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

        /// <summary>
        ///Moves drag image accordingly
        /// </summary>
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

        /// <summary>
        ///Creates a stack or collection based on which element was being dragged.
        /// </summary>
        private async void CollectionBtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;
            var vm = DataContext as ToolViewModel;
            if (vm != null)
            {
                if (sender == xCollectionElement)
                {
                    vm.CreateCollection(r.X, r.Y);
                }
                else if(sender == xStackElement)
                {
                    vm.CreateStack(r.X, r.Y);
                }
            }
            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(CollectionBtnAddOnManipulationDelta));
            args.Handled = true;
        }

        /// <summary>
        ///Dragging to move tool.
        /// </summary>
        private void Tool_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
            xFilterComboBox.IsEnabled = false;
        }

        /// <summary>
        ///Dragging to move tool.
        /// </summary>
        private void Tool_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var vm = DataContext as ToolViewModel;
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var x = e.Delta.Translation.X / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var y = e.Delta.Translation.Y / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;
            if (vm != null)
            {
                //vm.Controller.SetLocation(vm.X + x, vm.Y + y);
            }
            ToolAnchorChanged?.Invoke(this, new Point2d(vm.X + x, vm.Y + y));
        }

        /// <summary>
        /// This is necessary so that when you drag the filter combo box, the drop down list does not appear at the end of the drag.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XFilterComboBox_OnManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            xFilterComboBox.IsEnabled = true;

        }

        /// <summary>
        ///Resizing tool
        /// </summary>
        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

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

        /// <summary>
        ///Set size taking into account the min height and min width;
        /// </summary>
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
            }
            ToolAnchorChanged?.Invoke(this, new Point2d(_x, _y));

        }

        /// <summary>
        ///Sets that starting point for dragging. This is also to make sure that pie chart isn't visually selected once you click on it, because visual selection will always be based on the logcial selection in the model.
        /// </summary>
        private void xList_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _x = e.GetCurrentPoint(xCanvas).Position.X - 25;
            _y = e.GetCurrentPoint(xCanvas).Position.Y - 25;
            e.Handled = true;
        }

        /// <summary>
        ///Set up drag item for dragging filter
        /// </summary>
        private async void xListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (xCanvas.Children.Contains(_dragItem))
            {
                xCanvas.Children.Remove(_dragItem);

            }
            if (_currentDragMode == DragMode.Collection)
            {
                _dragItem = (DataContext as ToolViewModel).InitializeDragFilterImage();
            }
            _dragItem = (DataContext as ToolViewModel).InitializeDragFilterImage();
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX = _x;
            t.TranslateY = _y;
        }

        /// <summary>
        ///Either scroll or drag depending on the location of the point.
        /// </summary>
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
            else if (_dragItem.Visibility == Visibility.Collapsed)
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

        /// <summary>
        ///If the point is located outside the tool, logically set the selection based on selection type (Multi/Single) and either create new tool or add to existing tool
        /// </summary>
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
                    vm.Selection = new Tuple<string, HashSet<string>>(GetTextFromListItemGrid(sender as Grid), new HashSet<string>());
                }
                else if (_currentDragMode == DragMode.Value)
                {
                    if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                    {
                        vm.Selection.Item2.Add(GetTextFromListItemGrid(sender as Grid));
                        vm.Selection = vm.Selection;
                    }
                    else
                    {
                        vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                            new HashSet<string>() { GetTextFromListItemGrid(sender as Grid) });
                    }
                }
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);
                //vm.FilterIconDropped(hitsStart, wvm, r.X, r.Y);
            }
        }

        /// <summary>
        /// This returns the text that is inside the list item grid
        /// </summary>
        /// <param name="grid"></param>
        /// <returns></returns>
        private string GetTextFromListItemGrid(Grid grid)
        {
            if(grid.Children[0] as TextBlock != null)
            {
                return (grid.Children[0] as TextBlock).Text;
            }
            else
            {
                return (grid.Children[1] as TextBlock).Text;
            }
        }

        /// <summary>
        ///Sets the drag mode as either key or value based on which list triggered event
        /// </summary>
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

        /// <summary>
        ///When the list item is tapped, set the logical selection.
        /// </summary>
        private void KeyListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (DataContext as MetadataToolViewModel);
            if (vm.Controller.ToolModel.Selected &&
                vm.Selection.Item1.Equals(
                    GetTextFromListItemGrid(sender as Grid)))
            {
                vm.Controller.UnSelect();
            }
            else
            {
                Debug.Assert(vm != null);
                vm.Selection = new Tuple<string, HashSet<string>>(GetTextFromListItemGrid(sender as Grid), new HashSet<string>());
            }
        }

        /// <summary>
        ///When the list item is tapped, set the logical selection based on the type of selection (multi/single).
        /// </summary>
        private void ValueListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var vm = (DataContext as MetadataToolViewModel);
            if (vm.Controller.ToolModel.Selected && vm.Selection.Item2 != null &&
                vm.Selection.Item2.Contains(
                    GetTextFromListItemGrid(sender as Grid)))
            {
                if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                {
                    //if tapped item is already selected and in multiselect mode, remove item from selection
                    vm.Selection.Item2.Remove(GetTextFromListItemGrid(sender as Grid));
                    vm.Selection = vm.Selection;
                }
                else
                {
                    //if tapped item is already selected and in single select mode, remove all selections
                    vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1, new HashSet<string>());
                }
            }
            else
            {
                Debug.Assert(vm != null);
                if (xMetadataKeysList.SelectedItems.Count == 1)
                {
                    if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                    {
                        //if tapped item is not selected and in multiselect mode, add item to selection

                        if (vm.Selection != null)
                        {
                            var selection = GetTextFromListItemGrid(sender as Grid);
                            vm.Selection.Item2.Add(selection);
                            vm.Selection = vm.Selection;
                        }
                        else
                        {
                            vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                            new HashSet<string>() { GetTextFromListItemGrid(sender as Grid) });
                        }
                    }
                    else
                    {
                        //if tapped item is not selected and in single mode, set the item as the only selection
                        vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                             new HashSet<string>() { GetTextFromListItemGrid(sender as Grid) });
                    }
                }
            }
        }

        /// <summary>
        ///If the item that was double tapped is the only selected item, attempt to open the detail view.
        /// </summary>
        private void ValueListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var vm = (DataContext as MetadataToolViewModel);
            var textTapped = GetTextFromListItemGrid(sender as Grid);
            if (!vm.Selection.Item2.Contains(textTapped) && vm.Selection.Item2.Count == 0)
            {
                vm.Selection = new Tuple<string, HashSet<string>>(vm.Selection.Item1,
                            new HashSet<string>() { textTapped });
            }
            if (vm.Selection.Item2.Count == 1 &&
                vm.Selection.Item2.First().Equals(textTapped))
            {
                vm.OpenDetailView();
            }
        }

        /// <summary>
        /// When the selection of the filter combo box changes, either set a new filter on the basic tool view,
        /// or switch to an AllMetadata tool.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XFilterComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var filter = xFilterComboBox.SelectedItem is ToolModel.ToolFilterTypeTitle ? (ToolModel.ToolFilterTypeTitle)xFilterComboBox.SelectedItem : ToolModel.ToolFilterTypeTitle.Title;
            if (filter != ToolModel.ToolFilterTypeTitle.AllMetadata)
            {

                //(DataContext as MetadataToolViewModel).SwitchToBasicTool(filter);
                this.Dispose();
            }
        }

        private void XFilterComboBox_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            xFilterComboBox.IsEnabled = true;
        }

        /// <summary>
        /// This is just so that when you set the selection programatically before even creating the metadatatool, 
        /// such as when clicking on a tag of an image, the metadata values list will scroll to the selection.
        /// </summary>
        private void xMetadataValuesList_Loaded(object sender, RoutedEventArgs e)
        {
            xMetadataValuesList.ScrollIntoView(xMetadataValuesList.SelectedItem);
        }

        private void FetchAnalysisModels(ToggleButton button)
        {
            var libraryElementIds =
                (DataContext as MetadataToolViewModel)?.Controller?.GetUpdatedDataList()
                    .Where(
                        id =>
                            !SessionController.Instance.ContentController.HasAnalysisModel(
                                SessionController.Instance.ContentController.GetLibraryElementController(id)
                                    .LibraryElementModel.ContentDataModelId));

            Debug.Assert(libraryElementIds != null);
            if (libraryElementIds == null)
            {
                return;
            }
            int returned = 0;
            int needed = libraryElementIds.Count();

            //var button = (Button) sender;

            button.Background = new SolidColorBrush(Colors.Orange);
            button.Content = "0 %";
            button.HorizontalContentAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
            button.VerticalContentAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

            Task.Run(async delegate
            {

                foreach (var id in libraryElementIds)
                {
                    var libraryController = SessionController.Instance.ContentController.GetLibraryElementController(id);
                    if (libraryController == null)
                    {
                        returned ++;
                        continue;
                    }
                    if (!SessionController.Instance.ContentController.HasAnalysisModel(libraryController.LibraryElementModel.ContentDataModelId))
                    {
                        await Task.Delay(10);
                    }
                    Task.Run(async delegate
                        {
                            await SessionController.Instance.NuSysNetworkSession.FetchAnalysisModelAsync(libraryController.LibraryElementModel.ContentDataModelId);
                            returned++;
                            if (returned == needed)
                            {
                                UITask.Run(delegate
                                {
                                    button.Background = new SolidColorBrush(Colors.Green);
                                    button.Content = null;
                                    RefreshFomTopOfChain();
                                    button.Background = new SolidColorBrush(Colors.Transparent);
                                    button.Content = "Hide Suggested Tags";
                                });
                            }
                            else
                            {
                                UITask.Run(delegate
                                {
                                    button.Content = Math.Round((double)100 * ((double)returned / (double)needed), 0) + " %";
                                });
                            }
                        });
                    
                }
                

            });
            
        }

        /// <summary>
        /// This is the method called when the refresh button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xRefreshButton_Click(object sender, RoutedEventArgs e)
        {
             RefreshFomTopOfChain();
        }

        public void RefreshFomTopOfChain()
        {
            var vm = (DataContext as MetadataToolViewModel);
            if(vm == null)
            {
                return;
            }
            Task.Run(delegate
            {
                vm.Controller.RefreshFromTopOfChain();
            });
        }

        /// <summary>
        /// When the fetch analysis toggle switch is checked. fetch, the analysis models, set the include suggest tags bool, reload from 
        /// top of filter chain.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xFetchAnalysisModelsButton_Checked(object sender, RoutedEventArgs e)
        {
            var model = ((DataContext as MetadataToolViewModel)?.Controller as MetadataToolController)?.Model as MetadataToolModel;
            if(model != null)
            {
                model.SetIncludeSuggestedTags(true);
                FetchAnalysisModels(sender as ToggleButton);
            }
        }

        /// <summary>
        /// When the fetch analysis toggle switch is unchecked.  set the include suggest tags bool, reload from 
        /// top of filter chain.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xFetchAnalysisModelsButton_Unchecked(object sender, RoutedEventArgs e)
        {
            var model = ((DataContext as MetadataToolViewModel)?.Controller as MetadataToolController)?.Model as MetadataToolModel;
            if (model != null)
            {
                model.SetIncludeSuggestedTags(false);
                    RefreshFomTopOfChain();
            }
        }
    }
}
