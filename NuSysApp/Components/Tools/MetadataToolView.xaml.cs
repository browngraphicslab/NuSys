﻿using System;
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

        private double _x;

        private double _y;

        public MetadataToolView(MetadataToolViewModel vm, double x, double y)
        {
            this.InitializeComponent();
            _dragItem = vm.InitializeDragFilterImage();
            vm.Controller.SetLocation(x, y);
            this.DataContext = vm;
            SetSize(400,500);
            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;
            //xMetadataKeysList.ItemsSource = (DataContext as MetadataToolViewModel).AllMetadataDictionary.Keys;
            xMetadataKeysList.ItemsSource = (DataContext as MetadataToolViewModel)?.AllMetadataDictionary.Keys;

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

        }

        private void XMetadataKeysList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as MetadataToolViewModel;
            Debug.Assert(vm != null);
            if (xMetadataKeysList.SelectedItems.Count == 1)
            {
                var x = xMetadataKeysList.SelectedItems[0];
                xMetadataValuesList.ItemsSource =
                    vm.AllMetadataDictionary[(string)xMetadataKeysList.SelectedItems[0]];
                vm.Selection = new Tuple<string, string>((string)xMetadataKeysList.SelectedItems[0], null);
            }

        }

        private void XMetadataValuesList_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var vm = DataContext as MetadataToolViewModel;
            Debug.Assert(vm != null);
            if (xMetadataValuesList.SelectedItems.Count == 1 && xMetadataKeysList.SelectedItems.Count == 1)
            {
                vm.Selection = new Tuple<string, string>(vm.Selection.Item1, (string)xMetadataValuesList.SelectedItems[0]);
            }
        }

        private void Vm_PropertiesToDisplayChanged()
        {
            var vm = DataContext as MetadataToolViewModel;
            Debug.Assert(vm != null);
            xMetadataKeysList.ItemsSource = vm.AllMetadataDictionary.Keys;
            if (vm.Selection != null &&
                (vm.Controller as MetadataToolController).Model.Selected &&
                vm.Selection.Item1 != null)
            {
                xMetadataKeysList.SelectionChanged -= XMetadataKeysList_OnSelectionChanged;
                xMetadataKeysList.SelectedItem = vm.Selection.Item1;
                xMetadataKeysList.SelectionChanged += XMetadataKeysList_OnSelectionChanged;
                if (vm.Selection.Item2 != null)
                {
                    xMetadataValuesList.SelectionChanged -= XMetadataValuesList_OnSelectionChanged;
                    xMetadataValuesList.SelectedItem = vm.Selection.Item2;
                    xMetadataValuesList.SelectionChanged += XMetadataValuesList_OnSelectionChanged;
                }
                else
                {
                    xMetadataValuesList.ItemsSource = vm.AllMetadataDictionary[vm.Selection.Item1];
                }
            }
            else
            {
                xMetadataValuesList.ItemsSource = new List<string>();
            }
            xMetadataKeysList.ScrollIntoView(xMetadataKeysList.SelectedItem);
            xMetadataValuesList.ScrollIntoView(xMetadataValuesList.SelectedItem);


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
            (DataContext as MetadataToolViewModel).Controller.SetSize(width, height);
            xMetadataKeysList.Height = height - ListBoxHeightOffset;
            xMetadataValuesList.Height = height - ListBoxHeightOffset;
            xMetadataKeysList.Width = width/2;
            xMetadataValuesList.Width =width/2;

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

        private void xList_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _x = e.GetCurrentPoint(xCanvas).Position.X - 25;
            _y = e.GetCurrentPoint(xCanvas).Position.Y - 25;
        }

        private async void xListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (xCanvas.Children.Contains(_dragItem))
                xCanvas.Children.Remove(_dragItem);
            //if (_currentDragMode == DragMode.Key)
            //{
            //    //xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
            //    xMetadataKeysList.SelectionChanged -= XMetadataKeysList_OnSelectionChanged;
            //    xMetadataKeysList.SelectedItem = ((sender as Grid).Children[0] as TextBlock).Text;
            //    xMetadataKeysList.SelectionChanged += XMetadataKeysList_OnSelectionChanged;
            //}
            //else if (_currentDragMode == DragMode.Value)
            //{
            //    //xPropertiesList.SelectionChanged -= XPropertiesList_OnSelectionChanged;
            //    xMetadataValuesList.SelectionChanged -= XMetadataValuesList_OnSelectionChanged;
            //    xMetadataValuesList.SelectedItem = ((sender as Grid).Children[0] as TextBlock).Text;
            //    xMetadataValuesList.SelectionChanged += XMetadataValuesList_OnSelectionChanged;
            //}



            //_currentDragMode = DragMode.Filter;
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
                if (_currentDragMode == DragMode.Key)
                {
                    (DataContext as MetadataToolViewModel).Selection = new Tuple<string, string>((((Grid)sender).Children[0] as TextBlock).Text, null);
                }
                else if (_currentDragMode == DragMode.Value)
                {
                    (DataContext as MetadataToolViewModel).Selection = new Tuple<string, string>((DataContext as MetadataToolViewModel).Selection.Item1, (((Grid)sender).Children[0] as TextBlock).Text);
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

        //private void KeyListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        //{
        //    if (xMetadataKeysList.SelectedItem != null && (xMetadataKeysList.SelectedItem as string).Equals(((sender as Grid).Children[0] as TextBlock).Text))
        //    {
        //        //(DataContext as ToolViewModel).Controller.UnSelect();
        //    }
        //}

        //private void ValueListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        //{
        //    if (xMetadataValuesList.SelectedItem != null && (xMetadataValuesList.SelectedItem as string).Equals(((sender as Grid).Children[0] as TextBlock).Text))
        //    {
        //        ((DataContext as ToolViewModel).Controller as MetadataToolController).SetSelection(new Tuple<string, string>((DataContext as MetadataToolViewModel).Selection.Item1, null));
        //    }
        //}
    }
}
