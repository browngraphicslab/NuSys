using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary>
        /// constructor takes in the usual parameters but enforces to take in a wordnode view model
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public WordElementRenderItem(WordNodeViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(vm, parent, resourceCreator){}


        /// <summary>
        /// Draw function's only voerride functionality should be to add a word-node icon to the wordnode
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            //TODO MIRANDA add in a word node icon to the drawing of this 'pdf'
            base.Draw(ds);
        }
    }
}
