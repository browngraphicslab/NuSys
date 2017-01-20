using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using NusysIntermediate;

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

        /// <summary>
        /// Button used to add or remove direction capabilities of the link
        /// </summary>
        private ButtonUIElement _toggleDirectionButton;

        /// <summary>
        /// Button used to reverse the direction of the link
        /// </summary>
        private ButtonUIElement _reverseDirectionButton;


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
                BorderColor = Colors.DarkSlateGray,
                Text = _controller.ContentDataController.ContentDataModel.Data
            };
            AddChild(_linkAnnotationsInputBox);

            _toggleDirectionButton = new ButtonUIElement(this, resourceCreator)
            {
                Height = 50,
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                Background = Colors.Azure,
                BorderWidth = 3,
                BorderColor = Colors.SlateGray
            };
            AddChild(_toggleDirectionButton);

            _reverseDirectionButton = new ButtonUIElement(this, resourceCreator)
            {
                Height = 50,
                ButtonText = "Reverse Direction",
                ButtonTextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                ButtonTextVerticalAlignment = CanvasVerticalAlignment.Center,
                Background = Colors.Azure,
                BorderWidth = 3,
                BorderColor = Colors.SlateGray
            };
            AddChild(_reverseDirectionButton);

            // event for when the controllers text changes or the user changes the text
            _controller.ContentDataController.ContentDataUpdated += ContentDataController_ContentDataUpdated;
            _linkAnnotationsInputBox.TextChanged += _linkAnnotationsInputBox_TextChanged;
            _reverseDirectionButton.Tapped += _reverseDirectionButton_Tapped;
            _toggleDirectionButton.Tapped += _toggleDirectionButton_Tapped;
        }

        /// <summary>
        /// Toggles the direction of the link from on to off
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _toggleDirectionButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_controller.LinkLibraryElementModel.Direction == NusysConstants.LinkDirection.None)
            {
                _controller.SetLinkDirection(NusysConstants.LinkDirection.Forward);
            }
            else
            {
                _controller.SetLinkDirection(NusysConstants.LinkDirection.None);
            }
        }

        private void _reverseDirectionButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_controller.LinkLibraryElementModel.Direction == NusysConstants.LinkDirection.Forward)
            {
                _controller.SetLinkDirection(NusysConstants.LinkDirection.Backward);
            }
            else
            {
                _controller.SetLinkDirection(NusysConstants.LinkDirection.Forward);
            }
        }

        public override void Dispose()
        {
            _controller.ContentDataController.ContentDataUpdated -= ContentDataController_ContentDataUpdated;
            _linkAnnotationsInputBox.TextChanged -= _linkAnnotationsInputBox_TextChanged;
            _reverseDirectionButton.Tapped -= _reverseDirectionButton_Tapped;
            _toggleDirectionButton.Tapped -= _toggleDirectionButton_Tapped;
            base.Dispose();
        }

        /// <summary>
        /// Called whenever the content data controllers data is updated, changes the text in the text input box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ContentDataController_ContentDataUpdated(object sender, string e)
        {
            _linkAnnotationsInputBox.TextChanged -= _linkAnnotationsInputBox_TextChanged;
            _linkAnnotationsInputBox.Text = e;
            _linkAnnotationsInputBox.TextChanged += _linkAnnotationsInputBox_TextChanged;
        }

        /// <summary>
        /// Called whenever the user inputs text into the annotations box, changes the data on the server
        /// </summary>
        /// <param name="item"></param>
        /// <param name="text"></param>
        private void _linkAnnotationsInputBox_TextChanged(InteractiveBaseRenderItem item, string text)
        {
            _controller.ContentDataController.ContentDataUpdated -= ContentDataController_ContentDataUpdated;
            _controller.ContentDataController.SetData(text);
            _controller.ContentDataController.ContentDataUpdated += ContentDataController_ContentDataUpdated;
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

            _inLinkedElementTextbox.Transform.LocalPosition = new Vector2(Width / 2 - _inLinkedElementTextbox.Width / 2, vertical_spacing);

            vertical_spacing += _inLinkedElementTextbox.Height + 20;

            _linkIconDisplayElement.Transform.LocalPosition = new Vector2(Width / 2 - _linkIconDisplayElement.Width / 2, vertical_spacing);

            vertical_spacing += _linkIconDisplayElement.Height + 20;

            _outLinkedElementTextbox.Transform.LocalPosition = new Vector2(Width / 2 - _outLinkedElementTextbox.Width / 2, vertical_spacing);

            vertical_spacing += _outLinkedElementTextbox.Height + 20;
            var linkDirectionVerticalSpace = _toggleDirectionButton.Height + 20;

            _linkAnnotationsInputBox.Width = Width - 2 * horizontal_spacing;
            _linkAnnotationsInputBox.Height = Height - vertical_spacing - 20 - linkDirectionVerticalSpace;
            _linkAnnotationsInputBox.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            vertical_spacing += _linkAnnotationsInputBox.Height + 20;
            _toggleDirectionButton.Width = (Width - 3 * horizontal_spacing) / 2;
            _toggleDirectionButton.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            _reverseDirectionButton.Width = (Width - 3 * horizontal_spacing) / 2;
            _reverseDirectionButton.Transform.LocalPosition = new Vector2(horizontal_spacing * 2 + _toggleDirectionButton.Width, vertical_spacing);

            _reverseDirectionButton.IsVisible = _controller.LinkLibraryElementModel.Direction !=
                                                NusysConstants.LinkDirection.None;
            _toggleDirectionButton.ButtonText = _controller.LinkLibraryElementModel.Direction == NusysConstants.LinkDirection.None ? "Add Direction" : "Remove Direction";

            base.Update(parentLocalToScreenTransform);
        }
    }
}
