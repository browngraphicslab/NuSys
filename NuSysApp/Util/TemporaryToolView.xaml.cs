using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Core;
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
namespace NuSysApp
{

    /// <summary>
    /// Temporary class for a tool that can be dragged and dropped onto the collection
    /// </summary>
    public sealed partial class TemporaryToolView : AnimatableUserControl, ToolViewable
    {
        //public ObservableCollection<string> PropertiesToDisplay { get; set; }

        private Image _dragItem;


        private enum DragMode { Filter, Scroll };

        private DragMode _currentDragMode = DragMode.Filter;


        private const int MinWidth = 250;
        private const int MinHeight = 300;
        private const int ListBoxHeightOffset = 175;
        public ObservableCollection<string> PropertiesToDisplayUnique { get; set; } 

        private double _x;
        private double _y;
        private BaseToolView _baseTool;
        public TemporaryToolView(BaseToolView baseTool)
        {
            PropertiesToDisplayUnique = new ObservableCollection<string>();
            this.InitializeComponent();
            _dragItem = baseTool.Vm.InitializeDragFilterImage();
            //xPropertiesList.Height = baseTool.Vm.Height - 175;
            _baseTool = baseTool;
        }

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

        public void SetViewSelection(List<string> selections)
        {
            xPropertiesList.SelectedItems.Clear();
            foreach (var selection in selections ?? new List<string>())
            {
                xPropertiesList.SelectedItems.Add(selection);
            }
            if (selections != null && selections.Count  > 0)
            {
                xPropertiesList.ScrollIntoView(xPropertiesList.SelectedItem);
            }
        }
        
        
        
        private void xListItem_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _x = e.GetCurrentPoint(_baseTool.getCanvas()).Position.X - 25;
            _y = e.GetCurrentPoint(_baseTool.getCanvas()).Position.Y - 25;
            e.Handled = true;
        }

        private void xListItem_OnTapped(object sender, TappedRoutedEventArgs e)
        {
            if (_baseTool.Vm.Selection != null && _baseTool.Vm.Controller.Model.Selected && _baseTool.Vm.Selection.Equals(((sender as Grid).Children[0] as TextBlock).Text))
            {
                _baseTool.Vm.Controller.UnSelect();
            }
            else
            {
                if (e.PointerDeviceType == PointerDeviceType.Pen)
                {
                    if (_baseTool.Vm.Selection != null)
                    {
                        var selection = ((sender as Grid).Children[0] as TextBlock).Text;
                        if (!_baseTool.Vm.Selection.Contains(selection))
                        {
                            _baseTool.Vm.Selection.Add(selection);
                        }
                        _baseTool.Vm.Selection = _baseTool.Vm.Selection;
                    }
                    else
                    {
                        _baseTool.Vm.Selection = new List<string> { (((sender as Grid).Children[0] as TextBlock).Text) };
                    }
                }
                else
                {
                    _baseTool.Vm.Selection = new List<string> {(((sender as Grid).Children[0] as TextBlock).Text)};
                }
            }
        }


        private void xListItem_OnDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            if (!_baseTool.Vm.Selection.Contains(((sender as Grid).Children[0] as TextBlock).Text) || _baseTool.Vm.Controller.Model.Selected == false)
            {
                _baseTool.Vm.Selection = new List<string> { (((sender as Grid).Children[0] as TextBlock).Text)};
            }
            _baseTool.Vm.OpenDetailView();
        }

        private async void xListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (_baseTool.getCanvas().Children.Contains(_dragItem))
                _baseTool.getCanvas().Children.Remove(_dragItem);
            _currentDragMode = DragMode.Filter;
            _baseTool.getCanvas().Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)_dragItem.RenderTransform;
            t.TranslateX = _x;
            t.TranslateY = _y;
        }

        private void xListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(xPropertiesList).TransformPoint(e.Position);
            if (sp.X < Width && sp.X > 0 && sp.Y > 0 && sp.Y < _baseTool.getCanvas().ActualHeight)
            {
                Border border = (Border) VisualTreeHelper.GetChild(xPropertiesList, 0);
                ScrollViewer scrollViewer = VisualTreeHelper.GetChild(border, 0) as ScrollViewer;
                if (scrollViewer != null)
                {
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta.Translation.Y);
                }
                if (_currentDragMode == DragMode.Filter)
                {
                    _dragItem.Visibility = Visibility.Collapsed;
                    _currentDragMode = DragMode.Scroll;
                }
            }
            else if(_currentDragMode == DragMode.Scroll)
            {
                _dragItem.Visibility = Visibility.Visible;
                _currentDragMode = DragMode.Filter;
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
            _baseTool.getCanvas().Children.Remove(_dragItem);
            if (_currentDragMode == DragMode.Filter)
            {

                _baseTool.Vm.Selection = new List<string>() {(((Grid) sender).Children[0] as TextBlock).Text};

                var wvm = SessionController.Instance.ActiveFreeFormViewer;
                var el = (FrameworkElement)sender;
                var sp = el.TransformToVisual(SessionController.Instance.SessionView).TransformPoint(e.Position);
                var r = wvm.CompositeTransform.Inverse.TransformBounds(new Rect(sp.X, sp.Y, 300, 300));
                var hitsStart = VisualTreeHelper.FindElementsInHostCoordinates(sp, null);

                _baseTool.Vm.FilterIconDropped(hitsStart, wvm, r.X, r.Y);
            }
        }

    }

}