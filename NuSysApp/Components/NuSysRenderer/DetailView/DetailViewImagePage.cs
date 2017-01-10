using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class DetailViewImagePage : DetailViewPage
    {
        private DetailViewImageRegionContent _content;
        private InkableUIElement _inkable;

        public DetailViewImagePage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            ImageLibraryElementController controller, bool showImageAnalysis, bool showRegions)
            : base(parent, resourceCreator, controller, showImageAnalysis, showRegions)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewImageRegionContent(this, Canvas, controller, showRegions);

            SetContent(_content);

            _inkable = new InkableUIElement(controller.ContentDataController, this, resourceCreator);
            _inkable.Background = Colors.Transparent;
            AddChild(_inkable);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            base.Update(parentLocalToScreenTransform);
            _inkable.Transform.LocalPosition = _content.Transform.LocalPosition;
            _inkable.Width = _content.Width;
            _inkable.Height = _content.Height;
        }
    }
}
