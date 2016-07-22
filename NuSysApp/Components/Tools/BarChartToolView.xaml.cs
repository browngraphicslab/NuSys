using System;
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
using WinRTXamlToolkit.Controls.Extensions;

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
            xInkCanvas.InkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
        }

        /// <summary>
        /// Given a list of ink points, returns the maximum and minimum x coordinates as a tuple where minx is item 1 and maxX is item 2
        /// </summary>
        /// <param name="inkPoints"></param>
        /// <returns></returns>
        private Tuple<double, double> GetMinMaxXValues(IEnumerable<InkPoint> inkPoints)
        {
            var minX = xBarChart.ActualWidth;
            var maxX = 0.0;
            foreach (InkPoint point in inkPoints)
            {
                if (point.Position.X < minX)
                {
                    minX = point.Position.X;
                }
                if (point.Position.X > maxX)
                {
                    maxX = point.Position.X;
                }
            }
            return new Tuple<double, double>(minX, maxX);
        } 

        private void InkPresenter_StrokesCollected(InkPresenter sender, InkStrokesCollectedEventArgs args)
        {
            var wvm = SessionController.Instance.ActiveFreeFormViewer;
            var el = xInkCanvas;
            var listOfBarsHit = new List<BarChartItemViewModel>();
            bool allSelected = true;
            foreach (InkStroke stroke in args.Strokes)
            {
                var minMaxXTuple = GetMinMaxXValues(stroke.GetInkPoints());
                var currentPointToCheck = new Point(minMaxXTuple.Item1, this.ActualHeight - 1);
                var columnWidth = xBarChart.ColumnDefinitions[0].ActualWidth;
                while (currentPointToCheck.X < minMaxXTuple.Item2)
                {
                    var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(currentPointToCheck);
                    var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);
                    var y =
                        hitsStart.Where(
                            uiElem =>
                                (uiElem is FrameworkElement) &&
                                (uiElem as FrameworkElement).DataContext is BarChartItemViewModel).ToList();
                    if (y.Any())
                    {
                        var barVm = ((y.First() as FrameworkElement)?.DataContext as BarChartItemViewModel);
                        listOfBarsHit.Add(barVm);
                        var selectionString = barVm?.Title;

                        //If any of the bars hit was unselected, just select the unselected bars. 
                        //If all bars were already selected, then allSelected will remain true which will cause
                        //all the bars that were hit to be deselected.
                        if (!_baseTool.Vm.Selection.Contains(selectionString))
                        {
                            allSelected = false;
                            _baseTool.Vm.Selection.Add(selectionString);
                        }
                    }
                    currentPointToCheck = new Point(currentPointToCheck.X + columnWidth, currentPointToCheck.Y);
                }
                //Checks if each point intersects with a bar chart item
                //foreach (InkPoint point in points)
                //{
                //    //Creates a new point using the x of the stroke point and the bottom of the bar chart
                //    //so you can draw ontop of a bar and still select that bar.
                //    Point bottomPoint = new Point(point.Position.X, this.ActualHeight - 1);
                //    var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(bottomPoint);
                //    var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);
                //    var y =
                //        hitsStart.Where(
                //            uiElem =>
                //                (uiElem is FrameworkElement) &&
                //                (uiElem as FrameworkElement).DataContext is BarChartItemViewModel).ToList();
                //    if (y.Any())
                //    {
                //        var barVm = ((y.First() as FrameworkElement)?.DataContext as BarChartItemViewModel);
                //        listOfBarsHit.Add(barVm);
                //        var selectionString = barVm?.Title;

                //        //If any of the bars hit was unselected, just select the unselected bars. 
                //        //If all bars were already selected, then allSelected will remain true which will cause
                //        //all the bars that were hit to be deselected.
                //        if (!_baseTool.Vm.Selection.Contains(selectionString))
                //        {
                //            allSelected = false;
                //            _baseTool.Vm.Selection.Add(selectionString);
                //        }
                //    }
                //}
            }
            //deselect all the bars that were hit
            if (allSelected == true)
            {
                foreach (var bar in listOfBarsHit)
                {
                    var selectionString = bar?.Title;
                    _baseTool.Vm.Selection.Remove(selectionString);
                }
            }
            //refresh the selection so the selection changed event fires
            _baseTool.Vm.Selection = _baseTool.Vm.Selection;
            //clear all the strokes in the ink container
            xInkCanvas.InkPresenter.StrokeContainer.Clear();

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
                SetUpBarChartItemHandlers(item);
                Grid.SetColumn(item, i);
                xBarChart.Children.Add(item);

                // take care of mappings
                _barChartItemDictionary.Add(kvp.Key, item);
                BarChartLegendItems.Add(vm);
                i++;
            }

            // create a space between the tallest column and the top
            _maxValue *= 1.1;

            // set the height of the bars in the bar chart
            SetBarChartBarHeights();
        }

        public void SetUpBarChartItemHandlers(BarChartItem item)
        {
            item.Tapped += xBarChartItem_OnTapped;
            item.PointerPressed += xListItem_PointerPressed;
            item.ManipulationStarted += xListItem_ManipulationStarted;
            item.ManipulationDelta += xListItem_ManipulationDelta;
            item.ManipulationCompleted += xListItem_ManipulationCompleted;
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
            _baseTool.Item_ManipulationStarted(sender);
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