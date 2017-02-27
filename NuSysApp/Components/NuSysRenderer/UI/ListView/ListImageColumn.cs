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
            var thumbnail = new RectangleUIElement(listViewRowUIElement, resourceCreator);
            thumbnail.Width = (RelativeWidth / sumOfAllColumnRelativeWidths) * listViewRowUIElement.Width;
            thumbnail.BorderWidth = BorderWidth;
            thumbnail.BorderColor = BorderColor;
            thumbnail.Height = rowHeight;
            thumbnail.Background = Colors.LightBlue;
            LoadCellImageAsync(thumbnail, itemSource);
            return thumbnail;

        }

        public virtual async void LoadCellImageAsync(RectangleUIElement thumbnail, T itemSource)
        {

            try
            {

                if (_dict.Keys.Contains(itemSource))
                {
                    thumbnail.Image = _dict[itemSource];
                }
                else
                {
                    thumbnail.Image = _image;
                    thumbnail.Image = await MediaUtil.LoadCanvasBitmapAsync(thumbnail.ResourceCreator, ColumnFunction(itemSource));
                    _dict[itemSource] = thumbnail.Image;

                }
                var imgBounds = thumbnail?.Image?.GetBounds(thumbnail.ResourceCreator);


                if (imgBounds == null)
                {
                    return;
                }

                var cellWidth = thumbnail.Width;
                var cellHeight = thumbnail.Height;

                var imgWidth = thumbnail.RegionBounds != null ? thumbnail.RegionBounds.Value.Width: thumbnail.Image.GetBounds(thumbnail.ResourceCreator).Width;
                var imgHeight = thumbnail.RegionBounds != null ? thumbnail.RegionBounds.Value.Height: thumbnail.Image.GetBounds(thumbnail.ResourceCreator).Height;

                if (imgWidth < 0 || imgHeight < 0)
                {
                    return;
                }

                var newWidth = imgWidth / imgHeight * cellHeight / cellWidth;
                var newHeight = 1;

                thumbnail.ImageBounds = new Rect(0.5 - newWidth / 2, 0, newWidth, newHeight);

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