using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Microsoft.Graphics.Canvas.Geometry;

namespace NuSysApp
{
    /// <summary>
    /// the class used for Word Nodes.  
    /// Essentially just pdf node
    /// </summary>
    public class WordElementRenderItem : PdfElementRenderItem
    {
        private ICanvasImage _wordUIcon;

        /// <summary>
        /// constructor takes in the usual parameters but enforces to take in a wordnode view model
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public WordElementRenderItem(WordNodeViewModel vm, CollectionRenderItem parent,
            ICanvasResourceCreatorWithDpi resourceCreator) : base(vm, parent, resourceCreator)
        {
        }

        /// <summary>
        /// this should simply load the image to the image rectangle
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            _wordUIcon = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri("ms-appx:///Assets/new icons/tools red.png"));
            await base.Load();
        }


        /// <summary>
        /// this draw override will just draw the word icon.
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            base.Draw(ds);
            if (_wordUIcon != null)
            {
                var orgTransform = ds.Transform;
                ds.Transform = Transform.LocalToScreenMatrix;

                using (ds.CreateLayer(1, CanvasGeometry.CreateRectangle(Canvas, new Rect(0, 0, Width, Height))))
                {
                    ds.DrawImage(_wordUIcon, new Rect(0,0,Constants.DefaultNodeSize * .05, Constants.DefaultNodeSize * .05), _wordUIcon.GetBounds(Canvas));
                }

                ds.Transform = orgTransform;
            }
        }
    }
}
