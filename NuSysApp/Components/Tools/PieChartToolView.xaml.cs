using System;
using System;
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

namespace NuSysApp
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

        /// <summary>
        ///Creates a new dictionary from properties list and sets it as the pie series item source
        /// </summary>
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

        /// <summary>
        ///Sets the visual selection of the pie legend
        /// </summary>
        public void SetVisualSelection(HashSet<string> selection)
        {
            var transparent = new SolidColorBrush(Colors.Transparent);
            if (selection == null)
            {
                foreach (LegendItem item in xPieChart.LegendItems)
                {
                    item.Background = transparent;
                }
                return;
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

        /// <summary>
        ///Set up drag item
        /// </summary>
        private async void PieSlice_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _baseTool.Item_ManipulationStarted(sender);
        }

        /// <summary>
        ///Move drag item accordingly
        /// </summary>
        private void PieSlice_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _baseTool.Item_ManipulationDelta(sender as FrameworkElement, e);
        }

        /// <summary>
        ///If the point is located outside the tool, logically set the selection based on selection type (Multi/Single) and either create new tool or add to existing tool
        /// </summary>
        private async void PieSlice_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var selected = ((KeyValuePair<string, int>)(sender as FrameworkElement).DataContext).Key;
            _baseTool.Item_ManipulationCompleted(sender, selected, e);
        }

        /// <summary>
        ///This is to make sure that pie chart isn't visually selected once you click on it, because visual selection will always be based on the logcial selection in the model.
        /// </summary>
        private void Slice_OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _baseTool.Item_PointerPressed(e);
            //e.Handled = true;
        }

        /// <summary>
        ///When the slice is tapped, set the logical selection based on the type of selection (multi/single).
        /// </summary>
        private void Slice_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var selected = ((KeyValuePair<string, int>)(sender as FrameworkElement).DataContext).Key;
            _baseTool.Item_OnTapped(selected, e.PointerDeviceType);
        }

        /// <summary>
        ///If the item that was double tapped is the only selected item, attempt to open the detail view.
        /// </summary>
        private void Slice_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var selected = ((KeyValuePair<string, int>)(sender as FrameworkElement).DataContext).Key;
            _baseTool.Item_OnDoubleTapped(selected);
        }

        /// <summary>
        /// When the list loads, set the visual selection based on the tool models logical selection
        /// </summary>
        private void XPieChart_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetVisualSelection(_baseTool.Vm.Selection);
        }
    }
}
