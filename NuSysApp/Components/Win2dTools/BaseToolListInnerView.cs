﻿using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Newtonsoft.Json.Converters;
using NusysIntermediate;

namespace NuSysApp
{
    public class BaseToolListInnerView : BaseToolInnerView
    {
        private ListViewUIElementContainer<string> _listView;
        public BaseToolListInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {
            Background = Colors.Green;
            _listView = new ListViewUIElementContainer<string>(this, ResourceCreator);
            _listView.ShowHeader = false;
            _listView.RowBorderThickness = 1;
            _listView.DisableSelectionByClick = true;
            var listColumn = new ListTextColumn<string>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model;
            
            _listView.AddColumns(new List<ListColumn<string>>() { listColumn });
            _listView.RowTapped += _listView_RowTapped;

            _listView.AddItems(new List<string>() {"1", "2", "3", "4", "5", "6", "7", "9", "10", });
            AddChild(_listView);
        }

        private void _listView_RowTapped(string item, string columnName)
        {
            _listView.SelectItem(item);
        }
        

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _listView.Width = this.Width;
            _listView.Height = this.Height;
            base.Update(parentLocalToScreenTransform);
            
        }

        /// <summary>
        /// This makes the list display the properties passed in
        /// </summary>
        /// <param name="propertiesList"></param>
        public override void SetProperties(List<string> propertiesList)
        {
            _listView.ClearItems();
            if (propertiesList.Count > 0)
            {
                _listView.AddItems(propertiesList);
            }
        }

        public override void Dispose()
        {
        }

        /// <summary>
        /// This visually selects rows in the list based on each of the items passed in
        /// </summary>
        /// <param name="itemsToSelect"></param>
        public override void SetVisualSelection(HashSet<string> itemsToSelect)
        {
            _listView.DeselectAllItems();
            foreach (var selection in itemsToSelect ?? new HashSet<string>())
            {
                _listView.SelectItem(selection);
            }
            if (itemsToSelect != null && itemsToSelect.Count > 0)
            {
                _listView.ScrollTo(itemsToSelect.Last());
            }
        }
    }
}