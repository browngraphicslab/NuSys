using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NetTopologySuite.Utilities;
using NuSysApp.Tools;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BaseToolView : AnimatableUserControl
    {
        public BasicToolViewModel Vm;
        private Image _dragItem;
        private Image _dragFilterItem;
        private ToolViewable _toolView;
        private enum ViewMode { PieChart, List, BarChart }
        private ViewMode _currentViewMode;


        private enum DragMode { Filter, Scroll };
        private DragMode _currentDragMode = DragMode.Filter;
        private bool _draggedOutside;
        private object currentManipultaionSender;
        private const int _minHeight = 200;
        private const int _minWidth = 200;
        private double _x;
        private double _y;

        public BaseToolView(BasicToolViewModel vm, double x, double y)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            vm.Controller.SetLocation(x, y);
            Vm = vm;
            xFilterComboBox.ItemsSource = Enum.GetValues(typeof(ToolModel.ToolFilterTypeTitle)).Cast<ToolModel.ToolFilterTypeTitle> ();

            xFilterComboBox.SelectedItem = vm.Filter;

            //xTitle.Text = vm.Filter.ToString();
            vm.ReloadPropertiesToDisplay();
            _toolView = new Tools.ListToolView(this);
            _toolView.SetProperties(Vm.PropertiesToDisplay);
            xViewTypeGrid.Children.Add((UIElement)_toolView);
            _currentViewMode = ViewMode.List;
            SetSize(250,450);
            (vm.Controller as BasicToolController).SelectionChanged += OnSelectionChanged;
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            vm.Controller.NumberOfParentsChanged += Controller_NumberOfParentsChanged;
            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            _dragFilterItem = Vm.InitializeDragFilterImage();
            xStackElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xStackElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            _draggedOutside = false;
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
        /// Removes all the handlers, and removes visually. Calls dispose on the toolView and the viewmodel.
        /// </summary>
        public void Dispose()
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            (DataContext as ToolViewModel).Dispose();
            _toolView.Dispose();
            (DataContext as BasicToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
            ((DataContext as BasicToolViewModel).Controller as BasicToolController).SelectionChanged -= OnSelectionChanged;
            (DataContext as BasicToolViewModel).Controller.NumberOfParentsChanged -= Controller_NumberOfParentsChanged;
        }

        /// <summary>
        ///Passes new properties to display to the toolview
        /// </summary>
        private void Vm_PropertiesToDisplayChanged()
        {
            _toolView.SetProperties(Vm.PropertiesToDisplay);
        }

        /// <summary>
        ///Sets up drag image when collection or stack image is starting to be dragged.
        /// </summary>
        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {
            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);
            var button = (Button)sender;
            button.Focus(FocusState.Pointer);
            CapturePointer(args.Pointer);
            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            _dragItem = new Image();
            _dragItem.Source = bmp;
            _dragItem.Width = ((Button)sender).Width;
            _dragItem.Height = ((Button)sender).Height;
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            (sender as FrameworkElement).AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta), true);
            args.Handled = true;
        }

        /// <summary>
        ///Moves drag image accordingly
        /// </summary>
        private void BtnAddOnManipulationDelta(object sender, PointerRoutedEventArgs args)
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
        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var vm = DataContext as ToolViewModel;
            if (vm != null)
            {
                if (sender == xStackElement)
                {
                    vm.CreateStack(r.X, r.Y);
                }
                else
                { 
                    vm.CreateCollection(r.X, r.Y);
                }
            }
            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta));
            args.Handled = true;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            this.Dispose();
        }

        /// <summary>
        ///Returns the that surrounds this entire element
        /// </summary>
        public Canvas getCanvas()
        {
            return xCanvas;
        }

        /// <summary>
        ///Passes new selection to the toolview
        /// </summary>
        private void OnSelectionChanged(object sender)
        {
            if (Vm.Selection != null && (Vm.Controller as BasicToolController).Model.Selected)
            {
                _toolView.SetVisualSelection(Vm.Selection);
            }
            else
            {
                _toolView.SetVisualSelection(new HashSet<string>());
            }
        }

        /// <summary>
        ///Resizing tool
        /// </summary>
        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var vm = (BasicToolViewModel)this.DataContext;
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
        public void SetSize(double width, double height)
        {
            if (width < _minWidth && height < _minHeight)
            {
                return;
            }
            if (width > _minWidth && height > _minHeight)
            {
                (DataContext as BasicToolViewModel).Controller.SetSize(width, height);
            }
            else if (height < _minHeight)
            {
                (DataContext as BasicToolViewModel).Controller.SetSize(width, this.Height);
            }
            else if (width < _minWidth)
            {
                (DataContext as BasicToolViewModel).Controller.SetSize(this.Width, height);
            }
        }

        /// <summary>
        ///Sets a new pie chart view as the tool view
        /// </summary>
        private void XPieChartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentViewMode != ViewMode.PieChart)
            {
                xViewTypeGrid.Children.Remove((UIElement)_toolView);
                _toolView = new PieChartToolView(this);
                _toolView.SetProperties(Vm.PropertiesToDisplay);
                xViewTypeGrid.Children.Add((UIElement)_toolView);
                _currentViewMode = ViewMode.PieChart;
                SetSize(400, this.Height);
            }
        }

        /// <summary>
        ///Sets a new list view as the tool view
        /// </summary>
        private void XListViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentViewMode != ViewMode.List)
            {
                xViewTypeGrid.Children.Remove((UIElement)_toolView);
                _toolView = new Tools.ListToolView(this);
                _toolView.SetProperties(Vm.PropertiesToDisplay);
                xViewTypeGrid.Children.Add((UIElement)_toolView);
                _currentViewMode = ViewMode.List;
                SetSize(this.Width, this.Height);
            }
        }

        /// <summary>
        ///Sets a new bar chart view as the tool view
        /// </summary>
        private void XBarChartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentViewMode != ViewMode.BarChart)
            {
                xViewTypeGrid.Children.Remove((UIElement)_toolView);
                _toolView = new BarChartToolView(this);
                _toolView.SetProperties(Vm.PropertiesToDisplay);
                xViewTypeGrid.Children.Add((UIElement)_toolView);
                _currentViewMode = ViewMode.BarChart;
                SetSize(400, this.Height);
                _toolView.SetVisualSelection(Vm.Selection);
            }
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

            var vm = DataContext as BasicToolViewModel;
            var wvm = SessionController.Instance.ActiveFreeFormViewer;


            var x = e.Delta.Translation.X / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var y = e.Delta.Translation.Y / SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleY;

            if (vm != null)
            {
                vm.Controller.SetLocation(vm.X + x, vm.Y + y);
            }
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
        ///Sets the parent operator
        /// </summary>
        private void XParentOperatorText_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            
            if (Vm.Controller.Model.ParentOperator == ToolModel.ParentOperatorType.And)
            {
                Vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.Or);
                xParentOperatorText.Text = "OR";
            }
            else if (Vm.Controller.Model.ParentOperator == ToolModel.ParentOperatorType.Or)
            {
                Vm.Controller.SetParentOperator(ToolModel.ParentOperatorType.And);
                xParentOperatorText.Text = "AND";
            }
        }

        /// <summary>
        /// When an item (e.g list view item, pie slice, bar chart) is tapped, change the selection accordingly
        /// </summary>
        /// <param name="selection"></param>
        /// <param name="type"></param>
        public void Item_OnTapped(string selection, PointerDeviceType type)
        {

            if (Vm.Selection != null && Vm.Controller.Model.Selected && Vm.Selection.Contains(selection))
            {
                if (type == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                {
                    Vm.Selection.Remove(selection);
                    Vm.Selection = Vm.Selection;
                }
                else
                {
                    Vm.Controller.UnSelect();
                }
            }
            else
            {
                if (type == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                {
                    if (Vm.Selection != null)
                    {
                        Vm.Selection.Add(selection);
                        Vm.Selection = Vm.Selection;
                    }
                    else
                    {
                        Vm.Selection = new HashSet<string> { selection };
                    }
                }
                else
                {
                    Vm.Selection = new HashSet<string> { selection };
                }
            }
        }

        /// <summary>
        /// When an item (e.g list view item, pie slice, bar chart) is double tapped, try to open the detail view
        /// </summary>
        public void Item_OnDoubleTapped(string selection)
        {
            if (!Vm.Selection.Contains(selection) && Vm.Selection.Count == 0 || Vm.Controller.Model.Selected == false)
            {
                Vm.Selection = new HashSet<string> { selection };
            }
            if (Vm.Selection.Count == 1 &&
                Vm.Selection.First().Equals(selection))
            {
                Vm.OpenDetailView();
            }
        }

        /// <summary>
        ///Set up drag item
        /// </summary>
        public void Item_ManipulationStarted(object sender)
        {
            if (getCanvas().Children.Contains(_dragFilterItem))
                getCanvas().Children.Remove(_dragFilterItem);
            _currentDragMode = DragMode.Filter;
            getCanvas().Children.Add(_dragFilterItem);
            _dragFilterItem.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)_dragFilterItem.RenderTransform;
            t.TranslateX = _x;
            t.TranslateY = _y;
            _dragFilterItem.Visibility = Visibility.Collapsed;
            _draggedOutside = false;
            //not a great way of doing this. find out if there is a way to STOP all manipulation events once a new manipulation event has started.
            currentManipultaionSender = sender;

        }

        /// <summary>
        ///Sets that starting point for dragging. This is also to make sure that list isn't visually selected once you click on it, because visual selection will always be based on the logcial selection in the model.
        /// </summary>
        public void Item_PointerPressed(PointerRoutedEventArgs e)
        {
            _x = e.GetCurrentPoint(getCanvas()).Position.X - 25;
            _y = e.GetCurrentPoint(getCanvas()).Position.Y - 25;
            e.Handled = true;
        }

        /// <summary>
        ///Either scroll or drag depending on the location of the point and the origin of the event
        /// </summary>
        public void Item_ManipulationDelta(FrameworkElement sender, ManipulationDeltaRoutedEventArgs e, FrameworkElement boundingScrollingElement = null)
        {
            var el = (FrameworkElement)sender;
            
            if (boundingScrollingElement != null)
            {
                var sp = el.TransformToVisual(boundingScrollingElement).TransformPoint(e.Position);
                if (sp.Y > 0 && sp.Y < boundingScrollingElement.ActualHeight && (sp.X > boundingScrollingElement.ActualWidth || sp.X < 0))
                {
                    _draggedOutside = true;
                    _dragFilterItem.Visibility = Visibility.Visible;
                    _currentDragMode = DragMode.Filter;
                }
                else if (_draggedOutside == true && e.IsInertial)
                {
                    e.Complete();
                }
                else if (_draggedOutside == false)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(boundingScrollingElement, 0);
                    ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                    if (scrollViewer != null)
                    {
                        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta.Translation.Y);
                    }
                    if (_currentDragMode == DragMode.Filter)
                    {
                        _dragFilterItem.Visibility = Visibility.Collapsed;
                        _currentDragMode = DragMode.Scroll;
                    }
                }
            }
            
            if ((_dragFilterItem.RenderTransform as CompositeTransform) != null && e.IsInertial == false)
            {
                var t = (CompositeTransform)_dragFilterItem.RenderTransform;
                var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
                var p = e.Position;
                t.TranslateX += e.Delta.Translation.X / zoom;
                t.TranslateY += e.Delta.Translation.Y / zoom;
            }
        }

        /// <summary>
        ///If the point is located outside the tool, logically set the selection based on selection type (Multi/Single) and either create new tool or add to existing tool
        /// </summary>
        public void Item_ManipulationCompleted(object sender, string selection, ManipulationCompletedRoutedEventArgs e)
        {
            getCanvas().Children.Remove(_dragFilterItem);
            if (_currentDragMode == DragMode.Filter && currentManipultaionSender == sender)
            {
                if (Vm.Selection.Contains(selection) || e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                {
                    Vm.Selection.Add(selection);
                    Vm.Selection = Vm.Selection;
                }
                else
                {
                    Vm.Selection = new HashSet<string>() { selection };
                }

                var wvm = SessionController.Instance.ActiveFreeFormViewer;
                var el = (FrameworkElement)sender;
                var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
                var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(sp.X, sp.Y, 300, 300));
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);

                Vm.FilterIconDropped(hitsStart, wvm, r.X, r.Y);
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
            Vm.Filter = xFilterComboBox.SelectedItem is ToolModel.ToolFilterTypeTitle ? (ToolModel.ToolFilterTypeTitle) xFilterComboBox.SelectedItem : ToolModel.ToolFilterTypeTitle.Title;
            if (Vm.Filter == ToolModel.ToolFilterTypeTitle.AllMetadata)
            {

                Vm.SwitchToAllMetadataTool();
                this.Dispose();
            }
        }

        /// <summary>
        /// This is the handler for when the filter combo box is tapped. All this is meant to do is set the 
        /// combo box to enabled if it isn't.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XFilterComboBox_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            xFilterComboBox.IsEnabled = true;
        }

        /// <summary>
        /// When the refresh button is clicked, refresh the entire filter chain.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xRefreshButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ToolViewModel).Controller.RefreshFromTopOfChain();
        }
    }

}
