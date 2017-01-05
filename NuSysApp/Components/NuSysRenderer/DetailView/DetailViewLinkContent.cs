using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;

namespace NuSysApp
{
    class DetailViewLinkContent : RectangleUIElement
    {
        /// <summary>
        /// the controller for thelink represented by this page of the detail view
        /// </summary>
        private LinkLibraryElementController _controller;

        private TextboxUIElement _inLinkedElementTextbox;

        private RectangleUIElement _linkIconDisplayElement;

        private TextboxUIElement _outLinkedElementTextbox;

        private ScrollableTextboxUIElement _linkAnnotationsInputBox;


        public DetailViewLinkContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LinkLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            // create the in inLinkedElement textbox and set its text to the title of the InAtomId
            _inLinkedElementTextbox = new TextboxUIElement(this, resourceCreator)
            {
                Text = SessionController.Instance.ContentController.GetLibraryElementModel(_controller.LinkLibraryElementModel.InAtomId).Title,
                Height = 50,
                Width = 200,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center
            };
            AddChild(_inLinkedElementTextbox);

            // create the link icon display element
            _linkIconDisplayElement = new RectangleUIElement(this, resourceCreator)
            {
                Height = 50,
                Width = 50,
            };
            AddChild(_linkIconDisplayElement);

            // create the in outLinkedElement textbox and set its text to the title of the OutAtomId
            _outLinkedElementTextbox = new TextboxUIElement(this, resourceCreator)
            {
                Text = SessionController.Instance.ContentController.GetLibraryElementModel(_controller.LinkLibraryElementModel.OutAtomId).Title,
                Height = 50, 
                Width = 200,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center
            };
            AddChild(_outLinkedElementTextbox);

            // create the link annotations textbox
            _linkAnnotationsInputBox = new ScrollableTextboxUIElement(this, resourceCreator, true, true)
            {
                PlaceHolderText = "Link annotations...",
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray
            };
            AddChild(_linkAnnotationsInputBox);
        }

        public override async Task Load()
        {
            _linkIconDisplayElement.Image = await CanvasBitmap.LoadAsync(Canvas, new Uri("ms-appx:///Assets/node icons/icon_link.png"));
            base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            float vertical_spacing = 20;
            float horizontal_spacing = 20;

            _inLinkedElementTextbox.Transform.LocalPosition = new Vector2(Width/2 - _inLinkedElementTextbox.Width/2, vertical_spacing);

            vertical_spacing += _inLinkedElementTextbox.Height + 20;

            _linkIconDisplayElement.Transform.LocalPosition = new Vector2(Width / 2 - _linkIconDisplayElement.Width / 2, vertical_spacing);

            vertical_spacing += _linkIconDisplayElement.Height + 20;

            _outLinkedElementTextbox.Transform.LocalPosition = new Vector2(Width / 2 - _outLinkedElementTextbox.Width / 2, vertical_spacing);

            vertical_spacing += _outLinkedElementTextbox.Height + 20;

            _linkAnnotationsInputBox.Width = Width - 2*horizontal_spacing;
            _linkAnnotationsInputBox.Height = Height - vertical_spacing - 20;
            _linkAnnotationsInputBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            base.Update(parentLocalToScreenTransform);
        }
    }
}
