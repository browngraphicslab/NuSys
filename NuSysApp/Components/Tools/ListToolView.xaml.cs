using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236
namespace NuSysApp.Tools
{

    /// <summary>
    /// Temporary class for a tool that can be dragged and dropped onto the collection
    /// </summary>
    public sealed partial class ListToolView : AnimatableUserControl, ToolViewable
    {
        //public ObservableCollection<string> PropertiesToDisplay { get; set; }

        private Image _dragItem;


        private enum DragMode { Filter, Scroll };

        private DragMode _currentDragMode = DragMode.Filter;


        private const int MinWidth = 250;
        private const int MinHeight = 300;
        private const int ListBoxHeightOffset = 175;
        public ObservableCollection<string> PropertiesToDisplayUnique { get; set; } 


        // for dragging
        private double _x;
        private double _y;
        private BaseToolView _baseTool;
        public ListToolView(BaseToolView baseTool)
        {
            PropertiesToDisplayUnique = new ObservableCollection<string>();
            this.InitializeComponent();
            _dragItem = baseTool.Vm.InitializeDragFilterImage();
            _baseTool = baseTool;
        }

        /// <summary>
        ///Clears then individually adds each of the new properties to the PropertiesToDisplayUnique observable collection which is bound to the list view.
        /// </summary>
        public void SetProperties(List<string> propertiesList)
        {
            HashSet<string> set = new HashSet<string>();
            PropertiesToDisplayUnique.Clear();
            foreach (var item in propertiesList)
            {
                if (item != null && !item.Equals(""))
                {
                    if (!set.Contains(item))
                    {
                        PropertiesToDisplayUnique.Add(item);
                        set.Add(item);
                    }
                }
            }
        }

        public void Dispose()
        {

        }

        /// <summary>
        ///Sets the visual selection of the list.
        /// </summary>
        public void SetVisualSelection(HashSet<string> selections)
        {
            xPropertiesList.SelectedItems.Clear();
            foreach (var selection in selections ?? new HashSet<string>())
            {
                xPropertiesList.SelectedItems.Add(selection);
            }
            if (selections != null && selections.Count > 0)
            {
                xPropertiesList.ScrollIntoView(xPropertiesList.SelectedItems.Last());
            }
        }

        /// <summary>
        ///Sets that starting point for dragging. This is also to make sure that list isn't visually selected once you click on it, because visual selection will always be based on the logcial selection in the model.
        /// </summary>
        private void xListItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _baseTool.Item_PointerPressed(e);
        }

        /// <summary>
        ///When the list item is tapped, set the logical selection based on the type of selection (multi/single).
        /// </summary>
        private void xListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            var selection = ((sender as Grid).Children[0] as TextBlock).Text;
            _baseTool.Item_OnTapped(selection, e.PointerDeviceType);
        }

        /// <summary>
        ///If the item that was double tapped is the only selected item, attempt to open the detail view.
        /// </summary>
        private void xListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            var selection = ((sender as Grid).Children[0] as TextBlock).Text;
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
        ///Either scroll or drag depending on the location of the point.
        /// </summary>
        private void xListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            _baseTool.Item_ManipulationDelta(sender as FrameworkElement, e, xPropertiesList);
        }


        /// <summary>
        ///If the point is located outside the tool, logically set the selection based on selection type (Multi/Single) and either create new tool or add to existing tool
        /// </summary>
        private async void xListItem_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            var selection = (((Grid) sender).Children[0] as TextBlock).Text;
            _baseTool.Item_ManipulationCompleted(sender, selection, e);
        }

        /// <summary>
        /// When the list loads, set the visual selection based on the tool models logical selection
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void XPropertiesList_OnLoaded(object sender, RoutedEventArgs e)
        {
            SetVisualSelection(_baseTool.Vm.Selection);

        }
    }

}