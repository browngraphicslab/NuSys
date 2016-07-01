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


        private enum DragMode { Filter, Collection };
        private DragMode _currentDragMode = DragMode.Filter;


        private const int MinWidth = 250;
        private const int MinHeight = 300;
        private const int  ListBoxHeightOffset = 175;

        private double _x;
        private double _y;

        public TemporaryToolView(BasicToolViewModel vm, double x, double y)
        {
            _dragItem = new Image();
            this.InitializeComponent();
            vm.Controller.SetLocation(x, y);
            this.DataContext = vm;
            xFilterElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xFilterElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            xTitle.Text = vm.Filter.ToString();
            xPropertiesList.Height = vm.Height - 175;
            PieChart.Height = vm.Height - 175;
            PieChart.Width = vm.Width;


            Binding b = new Binding();
            b.Path = new PropertyPath("PropertiesToDisplayUnique");
            xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);
            xUniqueButton.IsChecked = true;
            (PieChart.Series[0] as PieSeries).ItemsSource =
                    (DataContext as BasicToolViewModel).PieChartDictionary;

        }

        private void Vm_PropertiesToDisplayChanged()
        {
            if ((DataContext as BasicToolViewModel).Selection != null && ((DataContext as BasicToolViewModel).Controller as BasicToolController).Model.Selected && xPropertiesList.SelectedItems.Count == 0)
            {
                xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
                xPropertiesList.SelectedItem = (DataContext as BasicToolViewModel).Selection;
                xPropertiesList.SelectionChanged += XPropertiesList_OnSelectionChanged;
            }

            (PieChart.Series[0] as PieSeries).ItemsSource = (DataContext as BasicToolViewModel).PieChartDictionary;

        }

        public void Dispose()
        {
            xFilterElement.PointerPressed -= BtnAddOnManipulationStarting;
            xFilterElement.PointerReleased -= BtnAddOnManipulationCompleted;
            xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
            xUniqueButton.Checked -= XUniqueButton_OnChecked;
            (DataContext as BasicToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
            xResizer.ManipulationDelta -= Resizer_OnManipulationDelta;
        }



        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;

            if (_currentDragMode == DragMode.Filter)
            {
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                var hitsStartList = hitsStart.Where(uiElem => (uiElem is TemporaryToolView)).ToList();
                if (!hitsStartList.Any())
                {
                    hitsStartList = hitsStart.Where(uiElem => (uiElem is MetadataToolView)).ToList();
                }
                if (hitsStartList.Any())
                {

                    ToolViewModel toolViewModel;
                    if ((hitsStartList.First() as TemporaryToolView) != null)
                    {
                        toolViewModel = (hitsStartList.First() as TemporaryToolView).DataContext as ToolViewModel;
                    }
                    else
                    {
                        toolViewModel = (hitsStartList.First() as MetadataToolView).DataContext as ToolViewModel;
                    }
                    if (true) //(first != this)
                    {
                        bool createsLoop = false;
                        //checks if tools will create a loop, and prevent that from happening
                        var controllers = new List<ToolController>((DataContext as ToolViewModel).Controller.Model.ParentIds.Select(item => ToolController.ToolControllers.ContainsKey(item) ? ToolController.ToolControllers[item] : null));

                        while (controllers != null && controllers.Count != 0)
                        {
                            if (controllers.Contains(toolViewModel.Controller))
                            {
                                createsLoop = true;
                                break;
                            }
                            var tempControllers = new List<ToolController>();
                            foreach (var controller in controllers)
                            {
                                tempControllers = new List<ToolController>(tempControllers.Union(new List<ToolController>(
                                        controller.Model.ParentIds.Select(
                                            item =>
                                                ToolController.ToolControllers.ContainsKey(item)
                                                    ? ToolController.ToolControllers[item]
                                                    : null))));
                            }
                            controllers = tempControllers;
                        }
                        if (createsLoop == false)
                        {
                            var linkviewmodel = new ToolLinkViewModel(this.DataContext as ToolViewModel, toolViewModel);
                            var link = new ToolLinkView(linkviewmodel);
                            Canvas.SetZIndex(link, Canvas.GetZIndex(this) - 1);
                            wvm.AtomViewList.Add(link);
                            toolViewModel.Controller.AddParent((DataContext as ToolViewModel).Controller);
                        }

                    }

                }
                else
                {
                    var toolFilter = new ToolFilterView(r.X, r.Y, DataContext as ToolViewModel);
                    var toolFilterLinkViewModel = new ToolFilterLinkViewModel(DataContext as ToolViewModel, toolFilter);
                    var toolFilterLink = new ToolFilterLinkView(toolFilterLinkViewModel);
                    Canvas.SetZIndex(toolFilterLink, Canvas.GetZIndex(this) - 1);

                    toolFilter.AddLink(toolFilterLink);
                    wvm.AtomViewList.Add(toolFilter);
                    wvm.AtomViewList.Add(toolFilterLink);
                }


            }
            else if (_currentDragMode == DragMode.Collection)
            {
                var vm = DataContext as MetadataToolViewModel;
                if (vm != null)
                {
                    await Task.Run(async delegate
                    {
                        var collectionID = SessionController.Instance.GenerateId();
                        var request = new CreateNewLibraryElementRequest(collectionID, "", ElementType.Collection,
                            "Tool-Generated Collection");
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
                        var m = new Message();
                        m["width"] = "300";
                        m["height"] = "300";
                        m["nodeType"] = ElementType.Collection.ToString();
                        m["x"] = r.X;
                        m["y"] = r.Y;
                        m["contentId"] = collectionID;
                        m["autoCreate"] = true;
                        m["creator"] = SessionController.Instance.ActiveFreeFormViewer.Model.LibraryId;
                        var collRequest = new NewElementRequest(m);
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(collRequest);
                        foreach (var id in vm.Controller.Model.LibraryIds)
                        {
                            var lem = SessionController.Instance.ContentController.GetContent(id);
                            if (lem == null)
                            {
                                continue;
                            }
                            var dict = new Message();
                            dict["title"] = lem.Title;
                            dict["width"] = "300";
                            dict["height"] = "300";
                            dict["nodeType"] = lem.Type.ToString();
                            dict["x"] = "50000";
                            dict["y"] = "50000";
                            dict["contentId"] = lem.LibraryElementId;
                            dict["autoCreate"] = true;
                            dict["creator"] = collectionID;
                            var elementRequest = new NewElementRequest(dict);
                            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(elementRequest);
                        }

                    });
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

            if (sender == xFilterElement)
            {
                _currentDragMode = DragMode.Filter;
            }
            else if (sender == xCollectionElement)
            {
                _currentDragMode = DragMode.Collection;
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




        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            wvm.AtomViewList.Remove(this);
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

        private void xList_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
        //    //don't think this is necessary
        //    var view = SessionController.Instance.SessionView;
        //    _x = e.GetCurrentPoint(view).Position.X - 25;
        //    _y = e.GetCurrentPoint(view).Position.Y - 25;
        //    e.Handled = true;
        //    CapturePointer(e.Pointer);

        }

        private void xList_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {

            //LibraryElementModel element = SessionController.Instance.ContentController.GetContent(xPropertiesList.SelectedItem as string);
            //if ((SessionController.Instance.ActiveFreeFormViewer.ContentId == element.LibraryElementId) || (element.Type == ElementType.Link))
            //{
            //    e.Handled = true;
            //    return;
            //}


            //var view = SessionController.Instance.SessionView;
            //view.LibraryDraggingRectangle.SwitchType(element.Type);
            //view.LibraryDraggingRectangle.Show();
            //var rect = view.LibraryDraggingRectangle;
            //Canvas.SetZIndex(rect, 3);
            //rect.RenderTransform = new CompositeTransform();
            //var t = (CompositeTransform)rect.RenderTransform;


            //t.TranslateX += _x;
            //t.TranslateY += _y;

            //if (!SessionController.Instance.ContentController.ContainsAndLoaded(element.LibraryElementId))
            //{
            //    Task.Run(async delegate
            //    {
            //        SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(element.LibraryElementId);
            //    });
            //}

            e.Handled = true;
        }

        private void xList_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            //LibraryElementModel element = SessionController.Instance.ContentController.GetContent(xPropertiesList.SelectedItem as string);
            //if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == ElementType.Link))
            //{
            //    e.Handled = true;
            //    return;
            //}

            //var el = (FrameworkElement)sender;
            //var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);

            //var itemsBelow = VisualTreeHelper.FindElementsInHostCoordinates(sp, null).Where(i => i is LibraryView);
            //if (itemsBelow.Any())
            //{
            //    SessionController.Instance.SessionView.LibraryDraggingRectangle.Hide();
            //}
            //else
            //{
            //    SessionController.Instance.SessionView.LibraryDraggingRectangle.Show();

            //}
            //var view = SessionController.Instance.SessionView;
            //var rect = view.LibraryDraggingRectangle;
            //var t = (CompositeTransform)rect.RenderTransform;

            //t.TranslateX += e.Delta.Translation.X;
            //t.TranslateY += e.Delta.Translation.Y;

            //_x += e.Delta.Translation.X;
            //_y += e.Delta.Translation.Y;


            e.Handled = true;
        }

        private async void xList_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            //LibraryElementModel element = SessionController.Instance.ContentController.GetContent(xPropertiesList.SelectedItem as string);
            //if ((WaitingRoomView.InitialWorkspaceId == element.LibraryElementId) || (element.Type == ElementType.Link))
            //{
            //    e.Handled = true;
            //    return;
            //}

            //var rect = SessionController.Instance.SessionView.LibraryDraggingRectangle;


            //if (rect.Visibility == Visibility.Collapsed)
            //    return;

            //rect.Hide();
            //var r = SessionController.Instance.SessionView.MainCanvas.TransformToVisual(SessionController.Instance.SessionView.FreeFormViewer.AtomCanvas).TransformPoint(new Point(_x, _y));
            //await this.AddNode(new Point(r.X, r.Y), new Size(300, 300), element.Type, element.LibraryElementId);

            e.Handled = true;
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
                    dict["nodeType"] = elementType.ToString();
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

        private void XPropertiesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xPropertiesList.SelectedItems.Count == 1)
            {
                (DataContext as BasicToolViewModel).Selection  = ((string)(xPropertiesList.SelectedItems[0]));
                
            }
        }

        private void XUniqueButton_OnChecked(object sender, RoutedEventArgs e)
        {
            //PropertiesToDisplay = (DataContext as BasicToolViewModel).PropertiesToDisplay;
            Binding b = new Binding();
            b.Path = new PropertyPath("PropertiesToDisplayUnique");
            xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);
            
            if ((DataContext as BasicToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            {
                xPropertiesList.SelectedItem = (DataContext as BasicToolViewModel).Selection;
            }

            PieChart.Visibility = Visibility.Collapsed;
            xPropertiesList.Visibility = Visibility.Visible;
        }

        private void XUniqueButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            Binding b = new Binding();
            b.Path = new PropertyPath("PropertiesToDisplayUnique");
            xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);


            if ((DataContext as BasicToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            {
                xPropertiesList.SelectedItem = (DataContext as BasicToolViewModel).Selection;
            }

            PieChart.Visibility = Visibility.Visible;
            xPropertiesList.Visibility = Visibility.Collapsed;
        }
        


        private void Resizer_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (SessionController.Instance.SessionView.IsPenMode)
                return;

            var vm = (BasicToolViewModel) this.DataContext;

            var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;
            var resizeX = vm.Width + e.Delta.Translation.X/zoom;
            var resizeY = vm.Height + e.Delta.Translation.Y/zoom;

            if (resizeX > MinWidth && resizeY > MinHeight)
            {
                vm.Controller.SetSize(resizeX, resizeY);
                xPropertiesList.Height = resizeY - ListBoxHeightOffset;
                PieChart.Height = resizeY - 175;
                PieChart.Width = resizeX;

            }
            else if (resizeX > MinWidth)
            {
                vm.Controller.SetSize(resizeX, vm.Height);
            }
            else if (resizeY > MinHeight)
            {
                vm.Controller.SetSize(vm.Width, resizeY);
                xPropertiesList.Height = resizeY - ListBoxHeightOffset;
                PieChart.Height = resizeY - 175;
                PieChart.Width = resizeX;
            }
        }

        private void xFilterList_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            //keep this method.
            e.Handled = true;
        }

        private void xFilterList_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void xFilterList_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {

            e.Handled = true;
        }

        private void DataPointSeries_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((sender as PieSeries).SelectedItem != null)
            {
                var selected = (sender as PieSeries).SelectedItem is KeyValuePair<string, int> ? (KeyValuePair<string, int>) (sender as PieSeries).SelectedItem : new KeyValuePair<string, int>();
                (DataContext as BasicToolViewModel).Selection = selected.Key;
            }
        }
    }
    
}
