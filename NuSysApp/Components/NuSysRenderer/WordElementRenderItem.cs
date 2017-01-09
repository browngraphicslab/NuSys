using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// the class used for Word Nodes.  
    /// Essentially just pdf node
    /// </summary>
    public class WordElementRenderItem : PdfElementRenderItem
    {
        private RectangleUIElement _wordUIcon;

        /// <summary>
        /// constructor takes in the usual parameters but enforces to take in a wordnode view model
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public WordElementRenderItem(WordNodeViewModel vm, CollectionRenderItem parent,
            ICanvasResourceCreatorWithDpi resourceCreator) : base(vm, parent, resourceCreator)
        {
            _wordUIcon = new RectangleUIElement(this, resourceCreator);
            AddChild(_wordUIcon);
        }

        /// <summary>
        /// this should simply load the image to the image rectangle
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            _wordUIcon.Image = await CanvasBitmap.LoadAsync(ResourceCreator, new Uri("ms-appx:///Assets/new icons/tools red.png"));
            await base.Load();
        }


        /// <summary>
        /// this override should only set the location of the word icon
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            
            base.Update(parentLocalToScreenTransform);
        }
    }
}
