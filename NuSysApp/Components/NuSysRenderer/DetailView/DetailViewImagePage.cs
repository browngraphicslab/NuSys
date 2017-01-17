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
        private ImageLibraryElementController _controller;

        public DetailViewImagePage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            ImageLibraryElementController controller, bool showImageAnalysis, bool showRegions)
            : base(parent, resourceCreator, controller, showImageAnalysis, showRegions)
        {
            _controller = controller;

            // initialize the image rectangle and the _imageLayoutManager
            _content = new DetailViewImageRegionContent(this, Canvas, controller, showRegions);
            _content.ImageUrl = controller.Data;
            SetContent(_content);

            _inkable = new InkableUIElement(controller, this, resourceCreator);
            _inkable.Background = Colors.Transparent;
            AddChild(_inkable);
            _inkable.Transform.SetParent(_content.Transform);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_content.CroppedImageTarget.Width > 0.0)
            {
                _inkable.Width = (float) _content.CroppedImageTarget.Width;
            }
            if (_content.CroppedImageTarget.Height > 0.0)
            {
                _inkable.Height = (float)_content.CroppedImageTarget.Height;
            }
            _inkable.Transform.LocalPosition = _content.Transform.LocalPosition;
            base.Update(parentLocalToScreenTransform);
        }

        protected override void ExpandButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.SessionView.FreeFormViewer.ShowFullScreenImage(new Uri(_controller.ContentDataController.ContentDataModel.Data));
        }
    }
}
