using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
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


        private ICanvasImage _image;
        public ListImageColumn(ICanvasResourceCreatorWithDpi resourceCreator )
        {
            _dict = new Dictionary<T, ICanvasImage>();
            LoadDefaultImageAsync(resourceCreator);
        }

        private async void LoadDefaultImageAsync(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            _image = await CanvasBitmap.LoadAsync(resourceCreator, new Uri("ms-appx:///Assets/node icons/icon_image.png"));
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
            try
            {

                if (_dict.Keys.Contains(itemSource))
                {
                    cell.Image = _dict[itemSource];
                }
                else
                {
                    cell.Image = _image;
                    cell.Image = await CanvasBitmap.LoadAsync(cell.ResourceCreator, ColumnFunction(itemSource));
                    _dict[itemSource] = cell.Image;
                }

                var width = cell.Width;
                var height = cell.Height;
                var imgWidth = cell.Image.GetBounds(cell.ResourceCreator).Width;
                var imgHeight = cell.Image.GetBounds(cell.ResourceCreator).Height;

                if (imgWidth < 0 || imgHeight < 0)
                {
                    return;
                }
                if (imgWidth > imgHeight)
                {
                    cell.ImageBounds = new Rect(0, 0, height*imgWidth/imgHeight, height);

                }
                else
                {
                    cell.ImageBounds = new Rect(0, 0, width, width*imgHeight/imgWidth);
                }
            }
            catch(Exception e)
            {
                
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
