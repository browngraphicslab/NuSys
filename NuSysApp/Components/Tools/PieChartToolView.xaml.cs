using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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

namespace NuSysApp.Components.Tools
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
            //Binding bb = new Binding();
            //bb.Path = new PropertyPath("PieChartDictionary");
            //xPieSeries.SetBinding(PieSeries.ItemsSourceProperty, bb);
            //RefreshColorPallete();
            //(xPieChart.Series[0] as PieSeries).ItemsSource = PieChartDictionary;
        }
        private void XPieSeries_OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var selected = (sender as PieSeries).SelectedItem is KeyValuePair<string, int> ? (KeyValuePair<string, int>)(sender as PieSeries).SelectedItem : new KeyValuePair<string, int>();

            if (_baseTool.Vm.Selection != null && _baseTool.Vm.Controller.Model.Selected && _baseTool.Vm.Selection.Equals(selected.Key))
            {
                _baseTool.Vm.Controller.UnSelect();
            }
            else
            {
                if (xPieSeries.SelectedItem != null)
                {
                    _baseTool.Vm.Selection = selected.Key;
                }
            }
            xPieSeries.ReleasePointerCapture(e.Pointer);
        }

        public void SetSize(double width, double height)
        {
            this.Height = height - 175;
            this.Width = width;
            xPieChart.Height = height - 175;
            xPieChart.Width = width;
        }

        //public void RefreshColorPallete()
        //{
        //    ResourceDictionaryCollection pieSeriesPalette = new ResourceDictionaryCollection();

        //    Brush currentBrush = new SolidColorBrush(Colors.Red);

        //    ResourceDictionary pieDataPointStyles = new ResourceDictionary();
        //    Style stylePie = new Style(typeof(PieDataPoint));
        //    stylePie.Setters.Add(new Setter(PieDataPoint.BackgroundProperty, currentBrush));
        //    pieDataPointStyles.Add("DataPointStyle", stylePie);
        //    pieSeriesPalette.Add(pieDataPointStyles);

        //    currentBrush = new SolidColorBrush(Colors.Blue);

        //    pieDataPointStyles = new ResourceDictionary();
        //    stylePie = new Style(typeof(PieDataPoint));
        //    stylePie.Setters.Add(new Setter(PieDataPoint.BackgroundProperty, currentBrush));
        //    pieDataPointStyles.Add("DataPointStyle", stylePie);
        //    pieSeriesPalette.Add(pieDataPointStyles);

        //    currentBrush = new SolidColorBrush(Colors.Green);

        //    pieDataPointStyles = new ResourceDictionary();
        //    stylePie = new Style(typeof(PieDataPoint));
        //    stylePie.Setters.Add(new Setter(PieDataPoint.BackgroundProperty, currentBrush));
        //    pieDataPointStyles.Add("DataPointStyle", stylePie);
        //    pieSeriesPalette.Add(pieDataPointStyles);

        //    xPieChart.Palette = pieSeriesPalette;
        //}

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

        public void SetViewSelection(string selection)
        {
            var transparent = new SolidColorBrush(Colors.Transparent);
            if (selection == null)
            {
                foreach (LegendItem item in xPieChart.LegendItems)
                {
                    item.Background = transparent;
                }
                xPieSeries.SelectedItem = null;
                return;
            }
            foreach (KeyValuePair<string, int> item in xPieSeries.ItemsSource)
            {
                if (item.Key != null && item.Key.Equals(selection))
                {
                    xPieSeries.SelectedItem = item;
                    break;
                }
            }
            foreach (LegendItem item in xPieChart.LegendItems)
            {
                if (item.Content.Equals(selection))
                {
                    item.Background = new SolidColorBrush(Colors.Blue);
                }
                else
                {
                    item.Background = transparent;
                }
            }

        }


        private async void xListItem_ManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            if (_baseTool.getCanvas().Children.Contains(_dragItem))
                _baseTool.getCanvas().Children.Remove(_dragItem);
            //_dragItem = (DataContext as ToolViewModel).InitializeDragFilterImage();
            _baseTool.getCanvas().Children.Add(_dragItem);
            _dragItem.RenderTransform = new CompositeTransform();
            var t = (CompositeTransform)_dragItem.RenderTransform;
            var el = (FrameworkElement)sender;
            var sp = el.TransformToVisual(_baseTool.getCanvas()).TransformPoint(e.Position);
            t.TranslateX = sp.X - _dragItem.ActualWidth/2;
            t.TranslateY = sp.Y - _dragItem.ActualWidth / 2;
        }

        private void xListItem_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
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



        private async void xListItem_ManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
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
            _baseTool.Vm.FilterIconDropped(hitsStart, wvm, r.X, r.Y);


        }
    }
}
