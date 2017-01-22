using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            LoadDefaultIconDictionary();
            //TODO: make this a public property in the superclass
            _resourceCreator = resourceCreator;
        }

        private async void LoadDefaultIconDictionary()
        {
            _defaultIconDictionary = new BiDictionary<NusysConstants.ElementType, ICanvasImage>();

            var textIcon = await CanvasBitmap.LoadAsync(_resourceCreator, );

        }

        public  override async void LoadCellImageAsync(RectangleUIElement cell, T itemSource)
        {
            //base.LoadCellImageAsync(cell, itemSource);

            if (base.ImageDict.Keys.Contains(itemSource))
            {
                cell.Image = base.ImageDict[itemSource];
            }
            else if ()
            {
                
            }
            else
            {
                cell.Image = base.DefaultImage;
                cell.Image = await CanvasBitmap.LoadAsync(cell.ResourceCreator, base.ColumnFunction(itemSource));
                base.ImageDict[itemSource] = cell.Image;
            }

            var width = cell.Width;
            var height = cell.Height;

            var imgWidth = cell.Image.GetBounds(cell.ResourceCreator).Width;
            var imgHeight = cell.Image.GetBounds(cell.ResourceCreator).Height;

            if (imgWidth < 0 || imgHeight < 0)
            {
                return;
            }

            var newWidth = height*imgWidth/imgHeight;
            var newHeight = height;
            //cell.ImageHorizontalAlignment = HorizontalAlignment.Center;
            //cell.ImageBounds = new Rect(0, 0, height * imgWidth / imgHeight, height);


    }
}
