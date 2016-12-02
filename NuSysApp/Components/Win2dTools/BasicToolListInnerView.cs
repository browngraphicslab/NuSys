using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Newtonsoft.Json.Converters;
using NusysIntermediate;

namespace NuSysApp
{
    public class BasicToolListInnerView : BasicToolInnerView
    {
        private ListViewUIElementContainer<string> _listView;
        public BasicToolListInnerView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, BasicToolViewModel vm) : base(parent, resourceCreator, vm)
        {
            Background = Colors.White;
            SetUpList();
        }

        /// <summary>
        /// Sets up the list that displays all the data and adds it to child
        /// </summary>
        private void SetUpList()
        {
            _listView = new ListViewUIElementContainer<string>(this, ResourceCreator);
            _listView.ShowHeader = false;
            _listView.RowBorderThickness = 0;
            _listView.DisableSelectionByClick = true;
            var listColumn = new ListTextColumn<string>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model;

            _listView.AddColumns(new List<ListColumn<string>>() { listColumn });
            _listView.RowTapped += _listView_RowTapped;
            _listView.RowDragged += _listView_RowDragged;
            _listView.RowDragCompleted += _listView_RowDragCompleted;
            _listView.RowDoubleTapped += _listView_RowDoubleTapped;

            _listView.AddItems(new List<string>() { "1", "2", "3", "4", "5", "6", "7", "9", "10", });
            AddChild(_listView);
        }

        private void _listView_RowDoubleTapped(string item, string columnName, CanvasPointer pointer)
        {
            Item_OnDoubleTapped(item);
        }

        private void _listView_RowDragCompleted(string item, string columnName, CanvasPointer pointer)
        {
            Item_DragCompleted(item, pointer);
        }

        private void _listView_RowDragged(string item, string columnName, CanvasPointer pointer)
        {
            Item_Dragging(pointer);
        }

        private void _listView_RowTapped(string item, string columnName, CanvasPointer pointer)
        {
            Item_OnTapped(item, pointer);
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
            HashSet<string> set = new HashSet<string>(propertiesList);
            if (propertiesList.Count > 0)
            {
                
                _listView.AddItems(set.ToList());
            }
        }

        public override void Dispose()
        {
            _listView.RowTapped -= _listView_RowTapped;
            _listView.RowDragged -= _listView_RowDragged;
            _listView.RowDragCompleted -= _listView_RowDragCompleted;
            _listView.RowDoubleTapped -= _listView_RowDoubleTapped;
            _listView.Dispose();
            base.Dispose();
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