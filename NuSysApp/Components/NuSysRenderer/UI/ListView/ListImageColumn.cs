using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Priority_Queue;
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


        private HashSet<T> _imagesBeingLoaded;

        private const int MAX_IMAGES_LOADING = 20;

        private bool _canLoadMoreImages => _imagesBeingLoaded.Count < MAX_IMAGES_LOADING && _priorityQueue.Count >=  MAX_IMAGES_LOADING - _imagesBeingLoaded.Count; 
        

        private SimplePriorityQueue<T> _priorityQueue;

        public ICanvasResourceCreatorWithDpi ResourceCreator => _resourceCreator;
        private ICanvasResourceCreatorWithDpi _resourceCreator;
        public ListImageColumn(ICanvasResourceCreatorWithDpi resourceCreator)
        {
            _resourceCreator = resourceCreator;
            _dict = new Dictionary<T, ICanvasImage>();
            _priorityQueue = new SimplePriorityQueue<T>();
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
            /*
            try
            {
                UpdateLoadingImages();

                if (_dict.Keys.Contains(itemSource))
                {
                    cell.Image = _dict[itemSource];
                }
                else
                {
                    //set cell image to default icon
                    cell.Image = _image;
                    //enque image to the priority queue
                    EnqueueImageToBeLoaded(itemSource);
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
            */
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

        private void EnqueueImageToBeLoaded(T item)
        {
            if (_priorityQueue.Contains(item))
            {
                _priorityQueue.Enqueue(item, 1);
            }
        }

        private async void UpdateLoadingImages()
        {
            while (_canLoadMoreImages) { 
                
                var item = _priorityQueue.Dequeue();
                var uri = ColumnFunction(item);
                _dict[item] = await MediaUtil.LoadCanvasBitmapAsync(ResourceCreator, ColumnFunction(item));
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