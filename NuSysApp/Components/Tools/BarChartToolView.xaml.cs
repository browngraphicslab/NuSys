﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Input.Inking;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using NuSysApp.Components.Tools;
using NuSysApp.Tools;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace NuSysApp
{
    public sealed partial class BarChartToolView : ToolViewable
    {

        // Properties for displaying the barchart
        private Dictionary<string, int> BarChartDictionary { get; set; }
        private BaseToolView _baseTool;
        private Dictionary<string, BarChartItem> _barChartItemDictionary;
        private double _maxValue;


        // the data context for the list view
        public ObservableCollection<BarChartItemViewModel> BarChartLegendItems;


        // dragging variables
        private double _x;
        private double _y;
        private Image _dragItem;
        private enum DragMode { Filter, Scroll };
        private BarChartToolView.DragMode _currentDragMode = BarChartToolView.DragMode.Filter;


        public BarChartToolView(BaseToolView baseTool)
        {
            this.InitializeComponent();
            _baseTool = baseTool;
            _barChartItemDictionary = new Dictionary<string, BarChartItem>();
            BarChartLegendItems = new ObservableCollection<BarChartItemViewModel>();

            _dragItem = baseTool.Vm.InitializeDragFilterImage();
            xInkCanvas.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen;
            xInkCanvas.InkPresenter.InputProcessingConfiguration.RightDragAction = InkInputRightDragAction.LeaveUnprocessed;
            xInkCanvas.InkPresenter.StrokeInput.StrokeEnded += StrokeInput_StrokeEnded;
            xInkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var el = xInkCanvas;
            foreach (InkStroke stroke in args.Strokes)
            {
                var points = stroke.GetInkPoints();
                foreach (InkPoint point in points)
                {
                    var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(point.Position);
                    var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);
                    var y =
                        hitsStart.Where(
                            uiElem =>
                                (uiElem is FrameworkElement) &&
                                (uiElem as FrameworkElement).DataContext is BarChartItemViewModel).ToList();
                    if (y.Count() > 0)
                    {
                        var selection = ((y.First() as FrameworkElement)?.DataContext as BarChartItemViewModel)?.Title;
                        _baseTool.Vm.Selection.Add(selection);
                    }
                }
            }
            xInkCanvas.InkPresenter.StrokeContainer.Clear();
            _baseTool.Vm.Selection = _baseTool.Vm.Selection;

        }

        private void StrokeInput_StrokeEnded(InkStrokeInput sender, PointerEventArgs args)
        {
            var strokes = xInkCanvas.InkPresenter.StrokeContainer.GetStrokes();
        }
        
        // pass in a list of all the properties to show in the graph
        public void SetProperties(List<string> propertiesList)
        {
            BarChartDictionary = new Dictionary<string, int>();
            foreach (var item in propertiesList)
            {
                if (item != null && !item.Equals(""))
                {
                    if (!BarChartDictionary.ContainsKey(item))
                    {
                        BarChartDictionary.Add(item, 1);
                    }
                    else
                    {
                        BarChartDictionary[item] = BarChartDictionary[item] + 1;
                    }
                }
            }

            // populates all the visual elements in the bar chart
            SetData();
        }

        /// <summary>
        /// Populates all the visual elements in the barchart
        /// </summary>
        private void SetData()
        {

            // reset the visual elements to zero
            _maxValue = 0;
            _barChartItemDictionary.Clear();
            BarChartLegendItems.Clear();
            xBarChart.Children.Clear();
            xBarChart.ColumnDefinitions.Clear();

            // populate all the visual elements from the data in BarChartDictionary
            int i = 0;
            foreach (var kvp in BarChartDictionary)
            {

                // add all the columns to the barchart and find the height of the maximum column
                var columnDefinition = new ColumnDefinition();
                columnDefinition.Width = new GridLength(1, GridUnitType.Star);
                xBarChart.ColumnDefinitions.Add(columnDefinition);
                _maxValue = Math.Max(_maxValue, kvp.Value);

                // ad the barchart items to the bar chart
                var vm = new BarChartItemViewModel(kvp, GetColor(kvp.Key));
                var item = new BarChartItem(vm);
                item.Tapped += xBarChartItem_OnTapped;
                item.PointerPressed += xListItem_PointerPressed;
                //item.ManipulationMode = ManipulationModes.All;
                item.ManipulationStarted += xListItem_ManipulationStarted;
                item.ManipulationDelta += xListItem_ManipulationDelta;
                item.ManipulationCompleted += xListItem_ManipulationCompleted;
                Grid.SetColumn(item, i);
                xBarChart.Children.Add(item);

                // tae care of mappings
                _barChartItemDictionary.Add(kvp.Key, item);
                BarChartLegendItems.Add(vm);
                i++;
            }

            // create a space between the tallest column and the top
            _maxValue *= 1.1;

            // set the height of the bars in the bar chart
            SetBarChartBarHeights();
        }
        
        /// <summary>
        /// Returns the color based on the hash of the passed in string
        /// </summary>
        private Color GetColor(string text)
        {
            Color color;
            try
            {
                var idHash = WaitingRoomView.Encrypt(text);
                long number = Math.Abs(BitConverter.ToInt64(idHash, 0));
                long r1 = BitConverter.ToInt64(idHash, 1);
                long r2 = BitConverter.ToInt64(idHash, 2); ;

                var mod = 250;

                int r = (int)Math.Abs(((int)number % mod));
                int b = (int)Math.Abs((r1 * number) % mod);
                int g = (int)Math.Abs((r2 * number) % mod);
                color = Color.FromArgb((byte)200, (byte)r, (byte)g, (byte)b);
                /*
                var number = Int64.Parse(ID.Replace(@".", ""));
                var start = 2*(Int64.Parse(IP[IP.Length - 1].ToString()) + 1);

                number += start*2*number; 

                var mod = 250 - start;

                int r = (int)Math.Abs(start + ((int) number%mod));
                int b = (int)Math.Abs(start + ((int) (number*Int64.Parse(IP[IP.Length - 1].ToString())% mod)));
                int g = (int)Math.Abs(start + ((int) ((start*number*r )% mod)));
                _color = Color.FromArgb((byte) 200, (byte) r, (byte) g, (byte) b);*/
            }
            catch (Exception e)
            {
                color = Colors.Black;
            }
            return color;
        }

        public void Dispose()
        {
            
        }

        /// <summary>
        /// used to set the visual selection
        /// </summary>
        public void SetVisualSelection(HashSet<string> selection)
        {
            foreach (var item in _barChartItemDictionary)
            {
                var dataContext = item.Value.DataContext as BarChartItemViewModel;
                Debug.Assert(dataContext != null);
                dataContext.IsSelected = false;
            }

            foreach (var key in selection)
            {
                var dataContext = _barChartItemDictionary[key].DataContext as BarChartItemViewModel;
                Debug.Assert(dataContext != null);
                dataContext.IsSelected = true;
            }
        }

        /// <summary>
        /// Used to set the heights of elemnts in the bar chart when the bar chart changes size
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XBarChart_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            SetBarChartBarHeights();
        }

        /// <summary>
        /// Sets the heights of the bars in the bar chart
        /// </summary>
        private void SetBarChartBarHeights()
        {
            int i = 0;
            foreach (var uiElement in xBarChart.Children)
            {
                var item = uiElement as BarChartItem;
                var itemDataContext = item?.DataContext as BarChartItemViewModel;
                Debug.Assert(itemDataContext != null);
                itemDataContext.Height = (itemDataContext.Count / _maxValue) * xBarChart.ActualHeight;
                i++;
            }
        }

        /// <summary>
        ///Sets that starting point for dragging. This is also to make sure that list isn't visually selected once you click on it, because visual selection will always be based on the logcial selection in the model.
        /// </summary>
        private void xListItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _baseTool.Item_PointerPressed(e);
        }

        private void xBarChartItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var selection = ((sender as FrameworkElement)?.DataContext as BarChartItemViewModel)?.Title;
            _baseTool.Item_OnTapped(selection, e.PointerDeviceType);
            //if (_baseTool.Vm.Selection != null && _baseTool.Vm.Controller.Model.Selected && _baseTool.Vm.Selection.Contains(selection))
            //{
            //    if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
            //    {
            //        _baseTool.Vm.Selection.Remove(selection);
            //        _baseTool.Vm.Selection = _baseTool.Vm.Selection;
            //    }
            //    else
            //    {
            //        _baseTool.Vm.Controller.UnSelect();
            //    }
            //}
            //else
            //{
            //    if (e.PointerDeviceType == PointerDeviceType.Pen || CoreWindow.GetForCurrentThread().GetAsyncKeyState(VirtualKey.Shift) == CoreVirtualKeyStates.Down)
            //    {
            //        if (_baseTool.Vm.Selection != null)
            //        {
            //            _baseTool.Vm.Selection.Add(selection);
            //            _baseTool.Vm.Selection = _baseTool.Vm.Selection;
            //        }
            //        else
            //        {
            //            _baseTool.Vm.Selection = new HashSet<string> { selection };
            //        }
            //    }
            //    else
            //    {
            //        _baseTool.Vm.Selection = new HashSet<string> { selection };
            //    }
            //}
        }
        
        /// <summary>
        ///If the item that was double tapped is the only selected item, attempt to open the detail view.
        /// </summary>
        private void xListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var selection = ((sender as Grid)?.DataContext as BarChartItemViewModel)?.Title;
            _baseTool.Item_OnDoubleTapped(selection);
        }

        /// <summary>
        ///Set up drag item
        /// </summary>
        private async void xListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _baseTool.Item_ManipulationStarted();
        }

        /// <summary>
        /// If the sender is the bar chart item, there is no scrolling adjustment to worry about. If the sender 
        /// is the grid in the list view, then adjust the scrolling.
        /// </summary>
        private void xListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            if (sender is BarChartItem)
            {
                _baseTool.Item_ManipulationDelta((FrameworkElement) sender, e);
            }
            else
            {
                _baseTool.Item_ManipulationDelta((FrameworkElement)sender, e, xBarChartLegend);
            }
        }

        /// <summary>
        ///If the point is located outside the tool, logically set the selection based on selection type (Multi/Single) and either create new tool or add to existing tool
        /// </summary>
        private async void xListItem_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var selection = ((sender as FrameworkElement)?.DataContext as BarChartItemViewModel)?.Title;
            _baseTool.Item_ManipulationCompleted(sender, selection, e);
        }

    }

}