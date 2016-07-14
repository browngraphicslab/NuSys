using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        private ToolViewable _toolView;
        private enum ViewMode { PieChart, List }
        private ViewMode _currentViewMode;

        private const int _minHeight = 200;
        private const int _minWidth = 200;

        public BaseToolView(BasicToolViewModel vm, double x, double y)
        {
            this.DataContext = vm;
            this.InitializeComponent();
            vm.Controller.SetLocation(x, y);
            Vm = vm;
            xTitle.Text = vm.Filter.ToString();
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

            xStackElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xStackElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
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
        
        public void Dispose()
        {
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
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            (DataContext as ToolViewModel).Dispose();
            _toolView.Dispose();
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
            if (_currentViewMode == ViewMode.List)
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
            if (_currentViewMode == ViewMode.PieChart)
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
        ///Dragging to move tool.
        /// </summary>
        private void Tool_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
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
    }

}
