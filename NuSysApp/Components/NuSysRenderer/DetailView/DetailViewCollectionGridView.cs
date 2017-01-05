using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    public class DetailViewCollectionGridView : RectangleUIElement
    {
        private CollectionLibraryElementController _controller;

        public DetailViewCollectionGridView(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;
            Background = Colors.Red;
        }
    }
}
