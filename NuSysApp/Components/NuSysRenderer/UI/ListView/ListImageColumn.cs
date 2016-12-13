using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using Windows.UI;

namespace NuSysApp
{


    /// <summary>
    /// This function takes in a generic item, and returns the string to be displayed in the list.
    /// </summary>


    public class ListImageColumn<T> : ListColumn<T>
    {

        public Func<T, string> ColumnFunction { private get; set; }

        public override RectangleUIElement GetColumnCellFromItem(T itemSource, ListViewRowUIElement<T> listViewRowUIElement, ICanvasResourceCreatorWithDpi resourceCreator, float rowHeight, float sumOfAllColumnRelativeWidths)
        {
            var cell = new RectangleUIElement(listViewRowUIElement, resourceCreator);
            cell.Width = (RelativeWidth / sumOfAllColumnRelativeWidths) * listViewRowUIElement.Width;
            cell.BorderWidth = BorderWidth;
            cell.Bordercolor = BorderColor;
            cell.Height = rowHeight;
            cell.Background = Colors.White;
            cell.ImageBounds = new Windows.Foundation.Rect(0, 0, cell.Width, cell.Height);
            SetCellImageFromItem(itemSource, cell);
            return cell;
        }

        public async Task SetCellImageFromItem(T itemSource, RectangleUIElement cell)
        {
            var url = ColumnFunction(itemSource) as string;

            if (String.IsNullOrEmpty(url))
            {
                return;
            }

            //cell.Image?.Dispose();
            //cell.Image = await CanvasBitmap.LoadAsync(cell.ResourceCreator, new Uri("ms-appx:///Assets/add from file dark.png"));
            cell.ImageBounds = new Windows.Foundation.Rect(0, 0, cell.Width, cell.Height);
            try
            {
                LoadImage(itemSource, cell, url);
                //cell.Image = await CanvasBitmap.LoadAsync(cell.ResourceCreator, new Uri(url));
            }

            catch (Exception e)
            {
                LoadImage(itemSource, cell, "ms-appx:///Assets/add from file dark.png");
                //cell.Image = await CanvasBitmap.LoadAsync(cell.ResourceCreator, new Uri("ms-appx:///Assets/add from file dark.png"));
            }

        }

        public async Task LoadImage(T itemSource, RectangleUIElement cell, string url)
        {
            cell.Image =  await CanvasBitmap.LoadAsync(cell.ResourceCreator, new Uri(url));

        }

        public override void UpdateColumnCellFromItem(T item, RectangleUIElement cell)
        {
            //SetCellImageFromItem(item, cell);
        }

    }
}
