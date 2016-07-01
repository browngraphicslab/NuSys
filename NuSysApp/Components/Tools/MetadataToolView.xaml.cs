using System;
using System.Collections.Generic;
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
        private enum DragMode { Filter, Collection };

        private DragMode _currentDragMode = DragMode.Filter;

        private const int ListBoxHeightOffset = 175;

        public MetadataToolView(MetadataToolViewModel vm, double x, double y)
        {
            this.InitializeComponent();
            _dragItem = new Image();
            vm.Controller.SetLocation(x, y);
            this.DataContext = vm;
            xTitle.Text = vm.Filter.ToString();
            xFilterElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xFilterElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            xMetadataKeysList.ItemsSource = (DataContext as MetadataToolViewModel).AllMetadataDictionary.Keys;


        }

        private void XMetadataKeysList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xMetadataKeysList.SelectedItems.Count == 1)
            {
                string test = (string)xMetadataKeysList.SelectedItems[0];
                xMetadataValuesList.ItemsSource = (DataContext as MetadataToolViewModel).AllMetadataDictionary[test];
                (DataContext as MetadataToolViewModel).Selection = new Tuple<string, string>((string)xMetadataKeysList.SelectedItems[0], null);
            }

        }

        private void XMetadataValuesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (xMetadataValuesList.SelectedItems.Count == 1 && xMetadataKeysList.SelectedItems.Count == 1)
            {
                (DataContext as MetadataToolViewModel).Selection = new Tuple<string, string>((string)xMetadataKeysList.SelectedItems[0], (string)xMetadataValuesList.SelectedItems[0]);

            }
        }

        private void Vm_PropertiesToDisplayChanged()
        {
            xMetadataKeysList.ItemsSource = (DataContext as MetadataToolViewModel).AllMetadataDictionary.Keys;
            if ((DataContext as MetadataToolViewModel).Selection != null &&
                ((DataContext as MetadataToolViewModel).Controller as MetadataToolController).Model.Selected &&
                (DataContext as MetadataToolViewModel).Selection.Item1 != null)
            {
                xMetadataKeysList.SelectionChanged -= XMetadataKeysList_OnSelectionChanged;
                xMetadataKeysList.SelectedItem = (DataContext as MetadataToolViewModel).Selection.Item1;
                xMetadataKeysList.SelectionChanged += XMetadataKeysList_OnSelectionChanged;
                if ((DataContext as MetadataToolViewModel).Selection.Item2 != null)
                {
                    xMetadataValuesList.SelectionChanged -= XMetadataValuesList_OnSelectionChanged;
                    xMetadataValuesList.SelectedItem = (DataContext as MetadataToolViewModel).Selection.Item2;
                    xMetadataValuesList.SelectionChanged += XMetadataValuesList_OnSelectionChanged;
                }
                else
                {
                    xMetadataValuesList.ItemsSource = (DataContext as MetadataToolViewModel).AllMetadataDictionary[(DataContext as MetadataToolViewModel).Selection.Item1];

                }
            }
            else
            {
                xMetadataValuesList.ItemsSource = new List<string>();
            }
            



        }

        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;

            if (_currentDragMode == MetadataToolView.DragMode.Filter)
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
                    if(true) //(first != this)
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
            else if (_currentDragMode == MetadataToolView.DragMode.Collection)
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

        private void Tool_OnManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {


            e.Handled = true;


        }

        private void XFilterElement_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
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
                vm.Controller.SetSize(resizeX, resizeY);
                xMetadataKeysList.Height = resizeY - ListBoxHeightOffset;
                xMetadataValuesList.Height = resizeY - ListBoxHeightOffset;

            }
            else if (resizeX > MinWidth)
            {
                vm.Controller.SetSize(resizeX, vm.Height);
            }
            else if (resizeY > MinHeight)
            {
                vm.Controller.SetSize(vm.Width, resizeY);
                xMetadataKeysList.Height = resizeY - ListBoxHeightOffset;
                xMetadataValuesList.Height = resizeY - ListBoxHeightOffset;
            }
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


        private async void BtnAddOnManipulationStarting(object sender, PointerRoutedEventArgs args)
        {

            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);

            var button = (Button)sender;
            button.Focus(FocusState.Pointer);

            CapturePointer(args.Pointer);

            if (sender == xFilterElement)
            {
                _currentDragMode = MetadataToolView.DragMode.Filter;
            }
            else if (sender == xCollectionElement)
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

        private void XUniqueButton_OnChecked(object sender, RoutedEventArgs e)
        {
            //Binding b = new Binding();
            //b.Path = new PropertyPath("PropertiesToDisplayUnique");
            //xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);

            //if ((DataContext as BasicToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            //{
            //    xPropertiesList.SelectedItem = (DataContext as BasicToolViewModel).Selection;
            //}

            //xPropertiesList.Visibility = Visibility.Visible;
        }

        private void XUniqueButton_OnUnchecked(object sender, RoutedEventArgs e)
        {
            //Binding b = new Binding();
            //b.Path = new PropertyPath("PropertiesToDisplayUnique");
            //xPropertiesList.SetBinding(ListBox.ItemsSourceProperty, b);


            //if ((DataContext as BasicToolViewModel).Selection != null && xPropertiesList.SelectedItems.Count == 0)
            //{
            //    xPropertiesList.SelectedItem = (DataContext as BasicToolViewModel).Selection;
            //}

            //PieChart.Visibility = Visibility.Visible;
            //xPropertiesList.Visibility = Visibility.Collapsed;
        }


    }
}
