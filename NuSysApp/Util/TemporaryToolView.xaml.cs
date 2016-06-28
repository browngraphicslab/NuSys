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

        private double _x;
        private double _y;
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
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(p, null);
                hitsStart = hitsStart.Where(uiElem => (uiElem is TemporaryToolView)).ToList();
                if (hitsStart.Any())
                {
                    var first = hitsStart.First() as TemporaryToolView;
                    var linkviewmodel = new ToolLinkViewModel(this, first);
                    var link = new ToolLinkView(linkviewmodel);
                    Canvas.SetZIndex(link, Canvas.GetZIndex(this) - 1);
                    wvm.AtomViewList.Add(link);
                    (first.DataContext as ToolViewModel).Controller.AddParent((DataContext as ToolViewModel).Controller);
                    //TODO: set parent and stuff
                }
                else
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

            var vm = DataContext as ToolViewModel;
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

        private void XFilterList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
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

            xGrid.Children.Remove(xFilterList);
            xTitle.Text = selection.ToString();
            xUniqueButton.Visibility = Visibility.Visible;
            xUniqueText.Visibility = Visibility.Visible;
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
    }
    
}
