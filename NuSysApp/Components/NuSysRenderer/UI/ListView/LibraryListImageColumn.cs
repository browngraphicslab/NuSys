using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            var linkIcon = await MediaUtil.LoadCanvasBitmapAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/link.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Link] = linkIcon;
        }

        private async void LoadTextIcon()
        {
            var textIcon = await MediaUtil.LoadCanvasBitmapAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/text.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Text] = textIcon;

        }

        private async void LoadCollectionIcon()
        {
            var collectionIcon = await MediaUtil.LoadCanvasBitmapAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/collection_1.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Collection] = collectionIcon;

        }

        private async void LoadWordIcon()
        {
            var wordIcon = await MediaUtil.LoadCanvasBitmapAsync(_resourceCreator, new Uri("ms-appx:///Assets/library_thumbnails/word.png"));
            _defaultIconDictionary[NusysConstants.ElementType.Word] = wordIcon;

        }
        public override async void LoadCellImageAsync(RectangleUIElement cell, T itemSource)
        {
            try
            {
                var thumbnail = cell as ThumbnailUIElement;
                Debug.Assert(thumbnail != null);
                var model = itemSource as LibraryElementModel;

                if (_defaultIconDictionary.ContainsKey(model.Type))
                {
                    thumbnail.Image = _defaultIconDictionary[model.Type];
                    base.ImageDict[itemSource] = thumbnail.Image;
                }

                thumbnail.RegionBounds = GetRegionBounds(model);


                base.LoadCellImageAsync(thumbnail, itemSource);

            }
            catch (Exception e)
            {
                
            }


        }

        private Rect? GetRegionBounds(LibraryElementModel model)
        {
            switch (model.Type)
            {
                case NusysConstants.ElementType.Image:
                    var imageModel = (ImageLibraryElementModel)model;
                    return new Rect(imageModel.NormalizedX, imageModel.NormalizedY, imageModel.NormalizedWidth, imageModel.NormalizedHeight);
                case NusysConstants.ElementType.PDF:
                    var pdfModel = (PdfLibraryElementModel)model;
                    return new Rect(pdfModel.NormalizedX, pdfModel.NormalizedY, pdfModel.NormalizedWidth, pdfModel.NormalizedHeight);
                default:
                    return null;
            }
        }

        public override void Dispose()
        {
            _defaultIconDictionary.Clear();

            base.Dispose();
        }
    }
   
}
