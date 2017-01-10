using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using NusysIntermediate;
using NuSysApp.Components.NuSysRenderer.UI;

namespace NuSysApp
{
    public class ImageElementRenderItem : ElementRenderItem
    {
        private InkableImageDetailRenderItemUIElement _inkableImage;

        public ImageElementRenderItem(ImageElementViewModel vm, CollectionRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) :base(vm, parent, resourceCreator)
        {
            _inkableImage = new InkableImageDetailRenderItemUIElement(vm.Controller.LibraryElementController.ContentDataController, vm, this, resourceCreator);
            AddChild(_inkableImage);
        }
    }
}
