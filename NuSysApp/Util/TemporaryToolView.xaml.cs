using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using NuSysApp.Viewers;
using WinRTXamlToolkit.Controls;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
namespace NuSysApp
{

    /// <summary>
    /// Temporary class for a tool that can be dragged and dropped onto the collection
    /// </summary>
    public sealed partial class TemporaryToolView : AnimatableUserControl
    {
        //public ObservableCollection<string> PropertiesToDisplay { get; set; }

        private Image _dragItem;


        private enum DragMode { Filter, Collection, Scroll };
        private enum ViewMode { PieChart, List }

        private ViewMode _currentViewMode;
        private DragMode _currentDragMode = DragMode.Filter;


        private const int MinWidth = 250;
        private const int MinHeight = 300;
        private const int ListBoxHeightOffset = 175;

        private double _x;
        private double _y;

        public TemporaryToolView(BasicToolViewModel vm, double x, double y)
        {
            this.InitializeComponent();
            _dragItem= vm.InitializeDragFilterImage();
            _currentViewMode = ViewMode.List;
            vm.Controller.SetLocation(x, y);
            this.DataContext = vm;
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            xTitle.Text = vm.Filter.ToString();
            xPropertiesList.Height = vm.Height - 175;
            xPieChart.Height = vm.Height - 175;
            xPieChart.Width = vm.Width;

            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            //xPropertiesList.Loaded += (s, e) => Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            //    () => xPropertiesList.ScrollIntoView(xPropertiesList.SelectedItem));


            //Binding b = new Binding();
            //b.Path = new PropertyPath("PropertiesToDisplayUnique");
            //xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);

            Binding bb = new Binding();
            bb.Path = new PropertyPath("PieChartDictionary");
            xPieSeries.SetBinding(PieSeries.ItemsSourceProperty, bb);
            //(PieChart.Series[0] as PieSeries).ItemsSource = (DataContext as BasicToolViewModel).PieChartDictionary;

        }
        private void Vm_PropertiesToDisplayChanged()
        {
            if ((DataContext as BasicToolViewModel).Selection != null && ((DataContext as BasicToolViewModel).Controller as BasicToolController).Model.Selected && xPropertiesList.SelectedItems.Count == 0)
            {
                //xPropertiesList.SelectedItem = GetListItem((DataContext as BasicToolViewModel).Selection);
                xPropertiesList.SelectedItem = ((DataContext as BasicToolViewModel).Selection);
                xPropertiesList.ScrollIntoView(xPropertiesList.SelectedItem);
            }
            Binding bb = new Binding();
            bb.Path = new PropertyPath("PieChartDictionary");
            xPieSeries.SetBinding(PieSeries.ItemsSourceProperty, bb);
            //(PieChart.Series[0] as PieSeries).ItemsSource = (DataContext as BasicToolViewModel).PieChartDictionary;

        }

        public void Dispose()
        {
            (DataContext as BasicToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
        }



        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
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

            if (sender == xCollectionElement)
            {
                _currentDragMode = DragMode.Collection;
            }

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




        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
            (DataContext as ToolViewModel).Dispose();
            this.Dispose();

        }

        private void Tool_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {


            e.Handled = true;


        }

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


        private void XFilterElement_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void xListItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _x = e.GetCurrentPoint(xCanvas).Position.X - 25;
            _y = e.GetCurrentPoint(xCanvas).Position.Y - 25;
           
        }


        private async void xListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            
            //var x = e.OriginalSource as ListBox;
            //var y = x.SelectedItems;
            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);
            if (_currentDragMode == DragMode.Collection)
            {
                _currentDragMode = DragMode.Filter;
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
            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(xPropertiesList).TransformPoint(e.Position);

            if (sp.X < Width && sp.X > 0 && sp.Y > 0 && sp.Y < xGrid.ActualHeight)
            {
                Border border = (Border) VisualTreeHelper.GetChild(xPropertiesList, 0);
                ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta.Translation.Y);
                }
                if (_currentDragMode == DragMode.Filter)
                {
                    _dragItem.Visibility = Visibility.Collapsed;
                    _currentDragMode = DragMode.Scroll;
                }
                
            }
            else if(_currentDragMode == DragMode.Scroll)
            {
                _dragItem.Visibility = Visibility.Visible;
                _currentDragMode = DragMode.Filter;
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

            if (_currentDragMode == DragMode.Filter)
            {
                if (_currentViewMode == ViewMode.List)
                {
                    //xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
                    //xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
                    //xPropertiesList.SelectedItem = ((sender as Grid).Children[0] as TextBlock).Text;
                    //xPropertiesList.SelectionChanged += XPropertiesList_OnSelectionChanged;
                    (DataContext as BasicToolViewModel).Selection = (((Grid)sender).Children[0] as TextBlock).Text;
                    

                }

                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);
                if (hitsStart.Where(uiElem => (uiElem is TemporaryToolView)).ToList().Any())
                {
                    var hitsStartList = hitsStart.Where(uiElem => (uiElem is TemporaryToolView)).ToList();
                    (DataContext as ToolViewModel).AddFilterToExistingTool(hitsStartList, wvm);
                }

                else if (hitsStart.Where(uiElem => (uiElem is MetadataToolView)).ToList().Any())
                {
                    var hitsStartList = hitsStart.Where(uiElem => (uiElem is MetadataToolView)).ToList();
                    (DataContext as ToolViewModel).AddFilterToExistingTool(hitsStartList, wvm);
                }
                else if (hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList().Any())
                {
                    var hitsStartList = hitsStart.Where(uiElem => (uiElem is ToolFilterView)).ToList();
                    (DataContext as ToolViewModel).AddFilterToFilterToolView(hitsStartList, wvm);
                }
                else
                {
                    (DataContext as ToolViewModel).AddNewFilterTool(r.X, r.Y, wvm);
                }

            }
        }


        




        public async Task AddNode(Point pos, Size size, ElementType elementType, string libraryId)
        {
            Task.Run(async delegate
            {
                if (elementType != ElementType.Collection)
                {
                    var element = SessionController.Instance.ContentController.GetContent(libraryId);
                    var dict = new Message();
                    Dictionary<string, object> metadata;

                    metadata = new Dictionary<string, object>();
                    metadata["node_creation_date"] = DateTime.Now;
                    metadata["node_type"] = elementType + "Node";

                    dict = new Message();
                    dict["title"] = element?.Title + " element";
                    dict["width"] = size.Width.ToString();
                    dict["height"] = size.Height.ToString();
                    dict["type"] = elementType.ToString();
                    dict["x"] = pos.X;
                    dict["y"] = pos.Y;
                    dict["contentId"] = libraryId;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                    dict["metadata"] = metadata;
                    dict["autoCreate"] = true;
                    dict["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
                    var request = new NewElementRequest(dict);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                }
                else
                {
                    await
                        StaticServerCalls.PutCollectionInstanceOnMainCollection(pos.X, pos.Y, libraryId, size.Width,
                            size.Height);
                }
            });

            // TOOD: refresh library
        }
        

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
                vm.Controller.SetSize(resizeX, resizeY);
                xPropertiesList.Height = resizeY - ListBoxHeightOffset;
                xPieChart.Height = resizeY - 175;
                xPieChart.Width = resizeX;

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

        public void SetSize(double x, double y)
        {
            (DataContext as BasicToolViewModel).Controller.SetSize(x, y);
            xPropertiesList.Height = y - ListBoxHeightOffset;
            xPieChart.Height = y - 175;
            xPieChart.Width = x;
        }
        


        private void DataPointSeries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as PieSeries).SelectedItem != null)
            {
                
            }
        }

        private void XPieChartButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentViewMode == ViewMode.List)
            {
                Binding b = new Binding();
                b.Path = new PropertyPath("PropertiesToDisplayUnique");
                xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);

                if ((DataContext as BasicToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
                {
                    xPropertiesList.SelectedItem = (DataContext as BasicToolViewModel).Selection;
                }

                xPieChart.Visibility = Visibility.Visible;
                xPropertiesList.Visibility = Visibility.Collapsed;
                _currentViewMode = ViewMode.PieChart;
                SetSize(400, this.Height);
            }
        }

        private void XListViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentViewMode == ViewMode.PieChart)
            {
                Binding b = new Binding();
                b.Path = new PropertyPath("PropertiesToDisplayUnique");
                xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);

                if ((DataContext as BasicToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
                {
                    xPropertiesList.SelectedItem = (DataContext as BasicToolViewModel).Selection;
                }

                xPieChart.Visibility = Visibility.Collapsed;
                xPropertiesList.Visibility = Visibility.Visible;
                _currentViewMode = ViewMode.List;
            }
        }


        private void XPieSeries_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var selected = (sender as PieSeries).SelectedItem is KeyValuePair<string, int> ? (KeyValuePair<string, int>)(sender as PieSeries).SelectedItem : new KeyValuePair<string, int>();
            (DataContext as BasicToolViewModel).Selection = selected.Key;
            xPieSeries.ReleasePointerCapture(e.Pointer);
        }


        private void xListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if ((DataContext as ToolViewModel).Controller.Model.Selected && (DataContext as BasicToolViewModel).Selection.Equals(((sender as Grid).Children[0] as TextBlock).Text))
            {
                (DataContext as ToolViewModel).Controller.UnSelect();
            }
            else
            {
                if (xPropertiesList.SelectedItems.Count == 1)
                {
                    (DataContext as BasicToolViewModel).Selection = (((string)(xPropertiesList.SelectedItems[0])));
                }
            }
        }
    }

}