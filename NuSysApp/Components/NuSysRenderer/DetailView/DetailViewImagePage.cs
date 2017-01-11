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
            _inkable.BorderWidth = 2.0f;
            _inkable.Bordercolor = Colors.Green;
            AddChild(_inkable);
            _inkable.Transform.SetParent(_content.Transform);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _inkable.Width = (float)_content.CroppedImageTarget.Width;
            _inkable.Height = (float)_content.CroppedImageTarget.Height;
            _inkable.Transform.LocalPosition = _content.Transform.LocalPosition;
            base.Update(parentLocalToScreenTransform);
        }
    }
}
