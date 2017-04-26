using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class ListImageColumn<T> : ListColumn<T>, IDisposable
    {
        /// <summary>
        /// This function takes in a generic item, and returns the image to be displayed in the list.
        /// </summary>
        public Func<T, Uri> ColumnFunction { get; set; }


        public Dictionary<T, ICanvasImage> ImageDict => _dict;
        private Dictionary<T, ICanvasImage> _dict;


        public ICanvasImage DefaultImage => _image;
        private ICanvasImage _image;
        public ListImageColumn(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            _dict = new Dictionary<T, ICanvasImage>();
            LoadDefaultImageAsync(resourceCreator);
        }

        private async void LoadDefaultImageAsync(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            try
            {
                _image = await MediaUtil.LoadCanvasBitmapAsync(resourceCreator, new Uri("ms-appx:///Assets/icon_image.png"));

            }
            catch (Exception e)
            {

            }
        }

        public override RectangleUIElement GetColumnCellFromItem(T itemSource, ListViewRowUIElement<T> listViewRowUIElement, ICanvasResourceCreatorWithDpi resourceCreator, float rowHeight, float sumOfAllColumnRelativeWidths)
        {
            var cell = new RectangleUIElement(listViewRowUIElement, resourceCreator);
            cell.Width = (RelativeWidth / sumOfAllColumnRelativeWidths) * listViewRowUIElement.Width;
            cell.BorderWidth = BorderWidth;
            cell.BorderColor = BorderColor;
            cell.Height = rowHeight;
            cell.Background = Colors.Transparent;
            LoadCellImageAsync(cell, itemSource);
            return cell;

        }

        public virtual async void LoadCellImageAsync(RectangleUIElement cell, T itemSource)
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
                    cell.Image = await MediaUtil.LoadCanvasBitmapAsync(cell.ResourceCreator, ColumnFunction(itemSource));
                    _dict[itemSource] = cell.Image;

                    cell.Image = _image;
                    _dict[itemSource] = cell.Image;
                    _dict[itemSource] = await MediaUtil.LoadCanvasBitmapAsync(cell.ResourceCreator, ColumnFunction(itemSource));
                }

                var cellWidth = cell.Width;
                var cellHeight = cell.Height;

                var imgWidth = cell.Image.GetBounds(cell.ResourceCreator).Width;
                var imgHeight = cell.Image.GetBounds(cell.ResourceCreator).Height;

                if (imgWidth < 0 || imgHeight < 0)
                {
                    return;
                }

                var newWidth = imgWidth / imgHeight * cellHeight / cellWidth;
                var newHeight = 1;

                cell.ImageBounds = new Rect(0.5 - newWidth / 2, 0, newWidth, newHeight);

            }
            catch (Exception e)
            {

            }

        }


        public override async void UpdateColumnCellFromItem(T item, RectangleUIElement rectangleUIElement, bool isSelected)
        {
            LoadCellImageAsync(rectangleUIElement, item);
        }

        public override void Dispose()
        {
            _dict.Clear();
        }
    }
}