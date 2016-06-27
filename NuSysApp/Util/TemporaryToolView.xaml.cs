using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using NuSysApp.Viewers;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{

    /// <summary>
    /// Temporary class for a tool that can be dragged and dropped onto the collection
    /// </summary>
    public sealed partial class TemporaryToolView : AnimatableUserControl
    {
        public ObservableCollection<ToolModel.FilterTitle> Filters { get; set; }
        //public ObservableCollection<string> PropertiesToDisplay { get; set; }

        private Image _dragItem;
        private enum DragMode { Filter };
        private DragMode _currenDragMode = DragMode.Filter;
        public TemporaryToolView(ToolViewModel vm, double x, double y)
        {
            _dragItem = new Image();
            Filters = new ObservableCollection<ToolModel.FilterTitle>()
            { ToolModel.FilterTitle.Type, ToolModel.FilterTitle.Title,  ToolModel.FilterTitle.Creator,  ToolModel.FilterTitle.Date,  ToolModel.FilterTitle.MetadataKeys,  ToolModel.FilterTitle.MetadataValues };
            this.InitializeComponent();
            vm.Controller.SetLocation(x, y);
            vm.Controller.SetSize(100, 100);
            this.DataContext = vm;
            xFilterElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xFilterElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;

            Binding b = new Binding();
            b.Path = new PropertyPath("PropertiesToDisplay");
            xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);

        }

        private void Vm_PropertiesToDisplayChanged(string selection)
        {
            if ((DataContext as ToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            {
                xPropertiesList.SelectedItem = (DataContext as ToolViewModel).Selection;
            }
            
        }

        public void Dispose()
        {
            xFilterElement.PointerPressed -= BtnAddOnManipulationStarting;
            xFilterElement.PointerReleased -= BtnAddOnManipulationCompleted;
            xChooseFilter.Click -= ButtonBase_OnClick;
            xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
            xUniqueButton.Checked -= XUniqueButton_OnChecked;
            (DataContext as ToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
        }



        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;

            if (_currenDragMode == DragMode.Filter)
            {

                var vm = (ToolViewModel)DataContext;

                ToolModel model = new ToolModel();
                ToolController controller = new ToolController(model);
                vm.AddChildFilter(controller);
                ToolViewModel viewmodel = new ToolViewModel(controller);
                TemporaryToolView view = new TemporaryToolView(viewmodel, r.X, r.Y);
                var linkviewmodel = new ToolLinkViewModel(this, view);
                var link = new ToolLinkView(linkviewmodel);
                Canvas.SetZIndex(link, Canvas.GetZIndex(this) - 1);

                wvm.AtomViewList.Add(link);
                wvm.AtomViewList.Add(view);

            }

            ReleasePointerCaptures();
            (sender as FrameworkElement).RemoveHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta));
            args.Handled = true;
        }


        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {

            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);

            var button = (Button)sender;
            button.Focus(FocusState.Pointer);

            CapturePointer(args.Pointer);

            if (sender == xFilterElement)
            {
                _currenDragMode = DragMode.Filter;
            }




            var bmp = new RenderTargetBitmap();
            await bmp.RenderAsync((UIElement)sender);
            _dragItem = new Image();
            _dragItem.Source = bmp;
            _dragItem.Width = 50;
            _dragItem.Height = 50;
            xCanvas.Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            (sender as FrameworkElement).AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(BtnAddOnManipulationDelta), true);
            args.Handled = true;
        }

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


        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (xFilterList.SelectedItems.Count() < 1)
            {
                return;
            }
            ToolModel.FilterTitle selection = (ToolModel.FilterTitle)(xFilterList.SelectedItems[0]);
            var toolViewModel = DataContext as ToolViewModel;
            if (toolViewModel != null)
            {
                toolViewModel.Filter = selection;
            }
            toolViewModel.reloadPropertiesToDisplay();
            //do i need this
            //PropertiesToDisplay = (DataContext as ToolViewModel).PropertiesToDisplay;
            xPropertiesList.ItemsSource = (DataContext as ToolViewModel).PropertiesToDisplay;
            
            bottompanel.Children.Remove(xChooseFilter);
            xGrid.Children.Remove(xFilterList);
            xTitle.Text = selection.ToString();
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            this.Dispose();

        }

        private void UIElement_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {


            e.Handled = true;


        }

        private void Canvas_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {

            var vm = DataContext as ToolViewModel;
            if (vm != null)
            {
                vm.Controller.SetLocation(vm.X + e.Delta.Translation.X, vm.Y + e.Delta.Translation.Y);
            }
            e.Handled = true;
        }

        private void Grid_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void XFilterElement_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void xList_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
            CapturePointer(e.Pointer);
        }

        private void xList_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void xList_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;

        }

        private void XPropertiesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            

            if (xPropertiesList.SelectedItems.Count == 1)
            {
                (DataContext as ToolViewModel).Selection  = ((string)(xPropertiesList.SelectedItems[0]));
            }
        }

        private void XUniqueButton_OnChecked(object sender, RoutedEventArgs e)
        {
            //PropertiesToDisplay = (DataContext as ToolViewModel).PropertiesToDisplay;
            Binding b = new Binding();
            b.Path = new PropertyPath("PropertiesToDisplayUnique");
            xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);

            if ((DataContext as ToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            {
                xPropertiesList.SelectedItem = (DataContext as ToolViewModel).Selection;
            }
        }

        private void XUniqueButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Binding b = new Binding();
            b.Path = new PropertyPath("PropertiesToDisplay");
            xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);

            if ((DataContext as ToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            {
                xPropertiesList.SelectedItem = (DataContext as ToolViewModel).Selection;
            }
        }
    }
}
