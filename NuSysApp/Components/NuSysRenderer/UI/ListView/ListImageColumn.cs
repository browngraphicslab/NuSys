using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class ListImageColumn<T> : ListColumn<T>, IDisposable
    {
        /// <summary>
        /// This function takes in a generic item, and returns the image to be displayed in the list.
        /// </summary>
        public Func<T, Uri> ColumnFunction { private get; set; }


        private Dictionary<T, ICanvasImage> _dict;

        public ListImageColumn()
        {
            _dict = new Dictionary<T, ICanvasImage>();
        }
        public override RectangleUIElement GetColumnCellFromItem(T itemSource, ListViewRowUIElement<T> listViewRowUIElement, ICanvasResourceCreatorWithDpi resourceCreator, float rowHeight, float sumOfAllColumnRelativeWidths)
        {
            var cell = new RectangleUIElement(listViewRowUIElement, resourceCreator);
            cell.Width = (RelativeWidth / sumOfAllColumnRelativeWidths) * listViewRowUIElement.Width;
            cell.BorderWidth = BorderWidth;
            cell.Bordercolor = BorderColor;
            cell.Height = rowHeight;
            cell.Background = Colors.Transparent;
            LoadCellImageAsync(cell, itemSource);
            return cell;

        }

        private async void LoadCellImageAsync(RectangleUIElement cell, T itemSource)
        {
            if (_dict.Keys.Contains(itemSource))
            {
                cell.Image = _dict[itemSource];
            }
            else
            {
                cell.Image = await CanvasBitmap.LoadAsync(cell.ResourceCreator, ColumnFunction(itemSource));
                _dict[itemSource] = cell.Image;
            }

        }

        public override async void UpdateColumnCellFromItem(T item, RectangleUIElement rectangleUIElement, bool isSelected)
        {
            LoadCellImageAsync(rectangleUIElement, item);
        }

        public void Dispose()
        {
            _dict.Clear();
            _dict = null;
        }
    }
}
