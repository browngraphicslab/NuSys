using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Microsoft.Graphics.Canvas;

namespace NuSysApp
{
    class DetailViewImageRegionPage : RectangleUIElement
    {
        /// <summary>
        /// Rectangle holding the content of the image
        /// </summary>
        private RectangleImageUIElement _imageRect;

        /// <summary>
        /// the stack layout manager managing the layout of the image on the window
        /// </summary>
        private StackLayoutManager _imageLayoutManager;

        /// <summary>
        /// The layout manager for the add region button
        /// </summary>
        private StackLayoutManager _addRegionButtonLayoutManager;

        /// <summary>
        /// The add region button
        /// </summary>
        private ButtonUIElement _addRegionButton;

        /// <summary>
        /// The width of the add region button
        /// </summary>
        private float _addRegionButtonWidth = 25;

        /// <summary>
        /// The margin on the left and right of the add region button
        /// </summary>
        private float _addRegionButtonLeftRightMargin = 10;

        public DetailViewImageRegionPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator,
            ImageLibraryElementController controller) : base(parent, resourceCreator)
        {
            // initialize the image rectangle and the _imageLayoutManager
            _imageRect = new RectangleImageUIElement(this, Canvas, controller);
            _imageLayoutManager = new StackLayoutManager();
            _imageLayoutManager.AddElement(_imageRect);
            AddChild(_imageRect);

            // initialize the add region button and the _addRegionButtonLayoutManager
            _addRegionButton = new ButtonUIElement(this, resourceCreator, new RectangleUIElement(this, resourceCreator))
            {
                Background = Colors.Azure,
                ButtonText = "+",
                BorderWidth = 3,
                Bordercolor = Colors.DarkSlateGray
            };
            _addRegionButtonLayoutManager = new StackLayoutManager();
            AddChild(_addRegionButton);
            _addRegionButtonLayoutManager.AddElement(_addRegionButton);

            _addRegionButton.Tapped += AddRegionButton_Tapped;
        }

        /// <summary>
        /// Called whenever the add region button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void AddRegionButton_Tapped(ButtonUIElement item, CanvasPointer pointer)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The dispose method, remove events here, dispose of objects here
        /// </summary>
        public override void Dispose()
        {
            if (IsDisposed)
                return;

            _imageLayoutManager.Dispose();
            base.Dispose();
        }

        /// <summary>
        /// Draws the image and stuff onto the 
        /// </summary>
        /// <param name="ds"></param>
        public override void Draw(CanvasDrawingSession ds)
        {
            if (IsDisposed)
            {
                return;
            }

            var orgTransform = ds.Transform;

            ds.Transform = Transform.LocalToScreenMatrix;

            ds.Transform = orgTransform;
            base.Draw(ds);
        }

        /// <summary>
        /// The update method, manage the layout here, update the transform here, called before draw
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _addRegionButtonLayoutManager.SetSize(_addRegionButtonLeftRightMargin * 2 + _addRegionButtonWidth,Height);
            _addRegionButtonLayoutManager.VerticalAlignment = VerticalAlignment.Center;
            _addRegionButtonLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _addRegionButtonLayoutManager.ItemWidth = _addRegionButtonWidth;
            _addRegionButtonLayoutManager.ItemHeight = _addRegionButtonWidth;
            _addRegionButtonLayoutManager.ArrangeItems();

            _imageLayoutManager.SetSize(Width - _addRegionButtonLayoutManager.Width, Height);
            _imageLayoutManager.VerticalAlignment = VerticalAlignment.Center;
            _imageLayoutManager.HorizontalAlignment = HorizontalAlignment.Center;
            _imageLayoutManager.ItemWidth = Width - _addRegionButtonLayoutManager.Width - 20;
            _imageLayoutManager.ItemHeight = Height - 20;
            _imageLayoutManager.ArrangeItems(new Vector2(_addRegionButtonLayoutManager.Width, 0));

            base.Update(parentLocalToScreenTransform);
        }


    }
}
