﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
using Windows.UI.Xaml.Navigation;
using NuSysApp.Tools;
using WinRTXamlToolkit.Controls.DataVisualization.Charting;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp2
{
    public sealed partial class PieChartToolView : AnimatableUserControl, ToolViewable
    {
        private Image _dragItem;

        private BaseToolView _baseTool;
        private double _x;
        private double _y;
        public Dictionary<string, int> PieChartDictionary { get; set; }

        public PieChartToolView(BaseToolView baseTool)
        {
            this.InitializeComponent();
            
            _baseTool = baseTool;
            _dragItem = baseTool.Vm.InitializeDragFilterImage();
        }


        public void SetProperties(List<string> propertiesList)
        {
            PieChartDictionary = new Dictionary<string, int>();
            foreach (var item in propertiesList)
            {
                if (item != null && !item.Equals(""))
                {
                    if (!PieChartDictionary.ContainsKey(item))
                    {
                        PieChartDictionary.Add(item, 1);
                    }
                    else
                    {
                        PieChartDictionary[item] = PieChartDictionary[item] + 1;
                    }
                }
            }
            xPieSeries.ItemsSource = PieChartDictionary;
        }

        public void Dispose()
        {
            
        }

        public void SetViewSelection(HashSet<string> selection)
        {
            var transparent = new SolidColorBrush(Colors.Transparent);
            if (selection == null)
            {
                foreach (LegendItem item in xPieChart.LegendItems)
                {
                    item.Background = transparent;
                }
                //xPieSeries.SelectedItem = null;
                return;
            }
            foreach (KeyValuePair<string, int> item in xPieSeries.ItemsSource)
            {
                //if (item.Key != null && selection.Contains(item.Key))
                //{
                //    xPieSeries.SelectedItem = item;
                //}
            }
            foreach (LegendItem item in xPieChart.LegendItems)
            {
                if (selection.Contains(item.Content))
                {
                    item.Background = new SolidColorBrush(Colors.LightBlue);
                }
                else
                {
                    item.Background = transparent;
                }
            }

        }


        private async void PieSlice_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (_baseTool.getCanvas().Children.Contains(_dragItem))
                _baseTool.getCanvas().Children.Remove(_dragItem);
            _baseTool.getCanvas().Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(_baseTool.getCanvas()).TransformPoint(e.Position);
            t.TranslateX = sp.X - _dragItem.ActualWidth/2;
            t.TranslateY = sp.Y - _dragItem.ActualWidth / 2;
        }

        private void PieSlice_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if ((_dragItem.RenderTransform as CompositeTransform) != null)
            {

                var t = (CompositeTransform)_dragItem.RenderTransform;
                var zoom = SessionController.Instance.ActiveFreeFormViewer.CompositeTransform.ScaleX;

                var p = e.Position;
                t.TranslateX += e.Delta.Translation.X / zoom;
                t.TranslateY += e.Delta.Translation.Y / zoom;
            }
        }

        private async void PieSlice_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _baseTool.getCanvas().Children.Remove(_dragItem);

            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
            var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(sp.X, sp.Y, 300, 300));

            var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);
            if (hitsStart.Contains(this))
            {
                return;
            }
            var selected = (KeyValuePair<string, int>)(sender as FrameworkElement).DataContext;
            if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
            {
                _baseTool.Vm.Selection.Add(selected.Key);
                _baseTool.Vm.Selection = _baseTool.Vm.Selection;
            }
            else
            {
                _baseTool.Vm.Selection = new HashSet<string>() { selected.Key};
            }
            _baseTool.Vm.FilterIconDropped(hitsStart, wvm, r.X, r.Y);


        }

        private void Slice_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void Slice_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var selected = (KeyValuePair<string, int>)(sender as FrameworkElement).DataContext;
            if (_baseTool.Vm.Selection != null && _baseTool.Vm.Controller.Model.Selected && _baseTool.Vm.Selection.Contains(selected.Key))
            {
                if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                {
                    _baseTool.Vm.Selection.Remove(selected.Key);
                    _baseTool.Vm.Selection = _baseTool.Vm.Selection;
                }
                else
                {
                    _baseTool.Vm.Selection.Clear();
                    _baseTool.Vm.Controller.UnSelect();

                }
            }
            else
            {
                if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
                {
                    if (_baseTool.Vm.Selection != null)
                    {
                        _baseTool.Vm.Selection.Add(selected.Key);
                        _baseTool.Vm.Selection = _baseTool.Vm.Selection;
                    }
                    else
                    {
                        _baseTool.Vm.Selection = new HashSet<string> { selected.Key };
                    }
                }
                else
                {
                    _baseTool.Vm.Selection = new HashSet<string> { selected.Key };
                }
            }
        }

        private void Slice_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var selected = (KeyValuePair<string, int>)(sender as FrameworkElement).DataContext;
            if (!_baseTool.Vm.Selection.Contains(selected.Key) &&  _baseTool.Vm.Selection.Count == 0 || _baseTool.Vm.Controller.Model.Selected == false)
            {
                _baseTool.Vm.Selection = new HashSet<string>() { selected.Key};
            }
            if (_baseTool.Vm.Selection.Count == 1 &&
                _baseTool.Vm.Selection.First().Equals(selected.Key))
            {
                _baseTool.Vm.OpenDetailView();
            }
        }
    }
}
