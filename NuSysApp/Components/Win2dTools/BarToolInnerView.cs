﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Windows.Devices.Input;

namespace NuSysApp
{
    public class BarToolInnerView : BasicToolInnerView
    {
        private BarChartUIElement _barChart;
        public BarToolInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BasicToolViewModel viewModel) : base(parent, resourceCreator, viewModel)
        {
            Background = Colors.White;
            BorderWidth = 1;
            BorderColor = Constants.LIGHT_BLUE;
            SetUpBarChart();

        }

        private void SetUpBarChart()
        {
            _barChart = new BarChartUIElement(this, ResourceCreator);
            _barChart.DisableSelectionByClick = true;

            _barChart.BarTapped += BarChart_BarTapped;

            //_barChart.BarDragged += BarChart_BarDragged;

            var dragRecognizer = new DragGestureRecognizer();
            _barChart.GestureRecognizers.Add(dragRecognizer);
            dragRecognizer.OnDragged += DragRecognizer_OnDragged;

            _barChart.BarDragCompleted += BarChart_BarDragCompleted;
      
            AddChild(_barChart);

        }

        private void BarChart_BarDoubleTapped(object source, BarChartElement bar)
        {
            Item_OnDoubleTapped(bar.Item);
        }

        private void BarChart_BarTapped(object source, BarChartElement bar, PointerDeviceType type)
        {
            Item_OnTapped(bar.Item, type);
        }

        private void BarChart_BarDragCompleted(object source, BarChartElement bar, DragEventArgs args)
        {
            Item_DragCompleted(bar.Item, args);
        }

        private void DragRecognizer_OnDragged(DragGestureRecognizer sender, DragEventArgs args)
        {
            Item_Dragging(args);
        }



        public override void SetProperties(List<string> propertiesList)
        {
            _barChart.ClearItems();
            if (propertiesList.Count > 0)
            {
                _barChart.AddItems(propertiesList);
            }
        }

        public override void SetVisualSelection(HashSet<string> itemsToSelect)
        {
            _barChart.DeselectAllItems();
            foreach (var selection in itemsToSelect ?? new HashSet<string>())
            {
                _barChart.SelectElement(selection);
            }
            if (itemsToSelect != null && itemsToSelect.Count > 0)
            {
                //_listView.ScrollTo(itemsToSelect.Last());
            }
        }

        public override void Update(System.Numerics.Matrix3x2 parentLocalToScreenTransform)
        {
            _barChart.Width = this.Width;
            _barChart.Height = this.Height;
            base.Update(parentLocalToScreenTransform);

        }
    }


}