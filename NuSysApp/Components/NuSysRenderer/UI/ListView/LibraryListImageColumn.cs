using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class LibraryListImageColumn<T> : ListImageColumn<T>
    {
        private Dictionary<NusysConstants.ElementType, ICanvasImage> _defaultIconDictionary;
        private ICanvasResourceCreatorWithDpi _resourceCreator;

        public LibraryListImageColumn(ICanvasResourceCreatorWithDpi resourceCreator) : base(resourceCreator)
        {
            //TODO: make this a public property in the superclass
            _resourceCreator = resourceCreator;
            LoadDefaultIconDictionary();

        }

        private async void LoadDefaultIconDictionary()
        {
            _defaultIconDictionary = new BiDictionary<NusysConstants.ElementType, ICanvasImage>();

            LoadTextIcon();
            LoadLinkIcon();
            LoadCollectionIcon();
            LoadWordIcon();
            

        }

        private async void LoadLinkIcon()
        {
            var linkIcon = await CanvasBitmap.LoadAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/link.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Link] = linkIcon;
        }

        private async void LoadTextIcon()
        {
            var textIcon = await CanvasBitmap.LoadAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/text.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Text] = textIcon;

        }

        private async void LoadCollectionIcon()
        {
            var collectionIcon = await CanvasBitmap.LoadAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Collection] = collectionIcon;

        }

        private async void LoadWordIcon()
        {
            var wordIcon = await CanvasBitmap.LoadAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/word.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Word] = wordIcon;

        }
        public override async void LoadCellImageAsync(RectangleUIElement cell, T itemSource)
        {
            try
            {
                var model = itemSource as LibraryElementModel;


                if (base.ImageDict.Keys.Contains(itemSource))
                {
                    cell.Image = base.ImageDict[itemSource];
                }
                else if (_defaultIconDictionary.ContainsKey(model.Type))
                {
                    cell.Image = _defaultIconDictionary[model.Type];
                    base.ImageDict[itemSource] = cell.Image;
                }
                else 
                {
                    cell.Image = base.DefaultImage;
                    base.ImageDict[itemSource] = cell.Image;
                    base.ImageDict[itemSource] = await CanvasBitmap.LoadAsync(cell.ResourceCreator, base.ColumnFunction(itemSource));

                }

                var cellWidth = cell.Width;
                var cellHeight = cell.Height;

                var imgWidth = cell.Image.GetBounds(_resourceCreator).Width;
                var imgHeight = cell.Image.GetBounds(_resourceCreator).Height;

                if (imgWidth < 0 || imgHeight < 0)
                {
                    return;
                }

                var newWidth = imgWidth/imgHeight*cellHeight/cellWidth;
                var newHeight = 1;

                cell.ImageBounds = new Rect(0.5 - newWidth/2, 0, newWidth, newHeight);
            }
            catch (Exception e)
            {
                
            }

        }

        public override void Dispose()
        {
            _defaultIconDictionary.Clear();

            base.Dispose();
        }
    }
   
}
