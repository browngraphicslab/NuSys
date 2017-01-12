using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    class DetailViewPdfPage : DetailViewPage
    {

        private DetailViewPdfRegionContent _content;
        private InkableUIElement _inkable;

        private PdfLibraryElementController _controller;

        public DetailViewPdfPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, PdfLibraryElementController controller, bool showImageAnalysis, bool showRegions) : base(parent, resourceCreator, controller, showImageAnalysis, showRegions)
        {
            _controller = controller;
            _content = new DetailViewPdfRegionContent(this, resourceCreator, controller, showRegions);
            SetContent(_content);

            _inkable = new InkableUIElement(controller.ContentDataController, this, resourceCreator);
            _inkable.Background = Colors.Transparent;
            _content.AddChild(_inkable);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_content.PdfContent.CroppedImageTarget.Width > 0.0)
            {
                _inkable.Width = (float)_content.PdfContent.CroppedImageTarget.Width;
            }
            if (_content.PdfContent.CroppedImageTarget.Height > 0.0)
            {
                _inkable.Height = (float)_content.PdfContent.CroppedImageTarget.Height;
            }
            _inkable.Transform.LocalPosition = _content.PdfContent.Transform.LocalPosition;
            base.Update(parentLocalToScreenTransform);
        }

        protected override void ExpandButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var model = (PdfContentDataModel) _controller.ContentDataController.ContentDataModel;
            var pageUrl = model.PageUrls[_content.CurrentPage];
            SessionController.Instance.SessionView.FreeFormViewer.ShowFullScreenImage(new Uri(pageUrl));
        }
    }
}
