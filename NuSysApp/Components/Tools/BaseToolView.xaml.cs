﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using NuSysApp.Tools;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp.Components.Tools
{
    public sealed partial class BaseToolView : AnimatableUserControl
    {
        public BasicToolViewModel Vm;
        private Image _dragItem;
        private ToolViewable _toolView;
        private enum ViewMode { PieChart, List }
        private ViewMode _currentViewMode;

        public BaseToolView(BasicToolViewModel vm, double x, double y)
        {
            this.InitializeComponent();
            vm.Controller.SetLocation(x, y);
            this.DataContext = vm;
            Vm = vm;
            xTitle.Text = vm.Filter.ToString();
            vm.ReloadPropertiesToDisplay();
            _toolView = new TemporaryToolView(this);
            _toolView.SetProperties(Vm.PropertiesToDisplay);
            xViewTypeGrid.Children.Add((UIElement)_toolView);
            _currentViewMode = ViewMode.List;
            SetSize(400,600);
            (vm.Controller as BasicToolController).SelectionChanged += OnSelectionChanged;
            vm.PropertiesToDisplayChanged += Vm_PropertiesToDisplayChanged;

            xCollectionElement.AddHandler(PointerPressedEvent, new PointerEventHandler(BtnAddOnManipulationStarting), true);
            xCollectionElement.AddHandler(PointerReleasedEvent, new PointerEventHandler(BtnAddOnManipulationCompleted), true);

        }

        private void Vm_PropertiesToDisplayChanged()
        {
            _toolView.SetProperties(Vm.PropertiesToDisplay);
        }


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

        private async void BtnAddOnManipulationCompleted(object sender, PointerRoutedEventArgs args)
        {
            xCanvas.Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var p = args.GetCurrentPoint(SessionController.Instance.SessionView.MainCanvas).Position;
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(p.X, p.Y, 300, 300));
            var send = (FrameworkElement)sender;

            var vm = DataContext as ToolViewModel;
            if (vm != null)
            {
                vm.CreateCollection(r.X, r.Y);
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
        public void Dispose()
        {
            (DataContext as BasicToolViewModel).PropertiesToDisplayChanged -= Vm_PropertiesToDisplayChanged;
            ((DataContext as BasicToolViewModel).Controller as BasicToolController).SelectionChanged -= OnSelectionChanged;
        }

        public Canvas getCanvas()
        {
            return xCanvas;
        }
        private void OnSelectionChanged(object sender)
        {
            if (Vm.Selection != null && (Vm.Controller as BasicToolController).Model.Selected)
            {
                _toolView.SetViewSelection(Vm.Selection);
            }
            else
            {
                _toolView.SetViewSelection(null);
            }
        }
        private void XFilterElement_OnManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            e.Handled = true;
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
        public void SetSize(double width, double height)
        {
            (DataContext as BasicToolViewModel).Controller.SetSize(width, height);
            _toolView.SetSize(width, height);
            //xPropertiesList.Height = y - ListBoxHeightOffset;
            //xPieChart.Height = y - 175;
            //xPieChart.Width = x;
        }
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

        private void XListViewButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentViewMode == ViewMode.PieChart)
            {
                xViewTypeGrid.Children.Remove((UIElement)_toolView);
                _toolView = new TemporaryToolView(this);
                _toolView.SetProperties(Vm.PropertiesToDisplay);
                xViewTypeGrid.Children.Add((UIElement)_toolView);
                _currentViewMode = ViewMode.List;
                SetSize(this.Width, this.Height);
            }
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
    }

}
