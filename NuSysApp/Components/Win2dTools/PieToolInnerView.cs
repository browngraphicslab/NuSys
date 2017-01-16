using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Newtonsoft.Json.Converters;
using NusysIntermediate;



namespace NuSysApp
{
    public class PieToolInnerView : BasicToolInnerView
    {

        private PieChartUIElement _pieChart;

        public PieToolInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BasicToolViewModel vm) : base(parent, resourceCreator, vm)
        {
            Background = Colors.White;
            BorderWidth = 1;
            BorderColor = Constants.LIGHT_BLUE;
            SetUpPieChart();
        }

        public void SetUpPieChart()
        {
            _pieChart = new PieChartUIElement(this, ResourceCreator);
            _pieChart.DisableSelectionByClick = true;

            _pieChart.ElementTapped += PieChart_ElementTapped;
            _pieChart.ElementDragged += PieChart_ElementDragged;
            _pieChart.ElementDragCompleted += PieChart_ElementDragCompleted;
            _pieChart.ElementDoubleTapped += PieChart_ElementDoubleTapped;
            AddChild(_pieChart);
        }

        private void PieChart_ElementDoubleTapped(object source, PieChartElement<string> element, CanvasPointer pointer)
        {
            Item_OnDoubleTapped(element.Item);

        }

        private void PieChart_ElementDragCompleted(object source, PieChartElement<string> element, CanvasPointer pointer)
        {
            Item_DragCompleted(element.Item, pointer);

        }

        private void PieChart_ElementDragged(object source, PieChartElement<string> element, CanvasPointer pointer)
        {
            Item_Dragging(pointer);

        }

        private void PieChart_ElementTapped(object source, PieChartElement<string> element, CanvasPointer pointer)
        {
            Item_OnTapped(element.Item, pointer);

        }

        public override void Dispose()
        {
            _pieChart.ElementTapped -= PieChart_ElementTapped;
            _pieChart.ElementDragged -= PieChart_ElementDragged;
            _pieChart.ElementDragCompleted -= PieChart_ElementDragCompleted;
            _pieChart.ElementDoubleTapped -= PieChart_ElementDoubleTapped;
            _pieChart.Dispose();
            base.Dispose();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _pieChart.Width = this.Width;
            _pieChart.Height = this.Height;
            base.Update(parentLocalToScreenTransform);

        }
        public override void SetProperties(List<string> propertiesList)
        {
            _pieChart.ClearElements();
            if (propertiesList.Count > 0)
            {
                _pieChart.AddItems(propertiesList);
            }

        }

        public override void SetVisualSelection(HashSet<string> itemsToSelect)
        {
            _pieChart.DeselectAllElements();
            foreach (var selection in itemsToSelect ?? new HashSet<string>())
            {
                _pieChart.SelectElement(selection);
            }
            if (itemsToSelect != null && itemsToSelect.Count > 0)
            {
                //_listView.ScrollTo(itemsToSelect.Last());
            }
        }
    }
}