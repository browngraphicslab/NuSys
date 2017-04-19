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
    class DetailViewCollectionContent : RectangleUIElement
    {

        /// <summary>
        /// The library element controller for the collection represented by this page
        /// </summary>
        private CollectionLibraryElementController _controller;

        private DetailViewCollectionGridView _collectionGridView;


        /// <summary>
        /// The button used to enter the collection
        /// </summary>
        private ButtonUIElement _enterCollectionButton;

        public DetailViewCollectionContent(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            _collectionGridView = new DetailViewCollectionGridView(this, resourceCreator, _controller);
            AddChild(_collectionGridView);

            _enterCollectionButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.SecondaryStyle, "Enter Collection");
            _enterCollectionButton.Width = 150;
            _enterCollectionButton.Height = 40;
            AddChild(_enterCollectionButton);


            // add events
            _enterCollectionButton.Tapped += _enterCollectionButton_Tapped;
        }

        public override void Dispose()
        {
            _enterCollectionButton.Tapped -= _enterCollectionButton_Tapped;

            base.Dispose();
        }

        /// <summary>
        /// Enter the collection displayed in the detail viewer when the enter collection button is tapped
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void _enterCollectionButton_Tapped(ButtonUIElement sender)
        {
            if (SessionController.Instance.CurrentCollectionLibraryElementModel != _controller.LibraryElementModel)
            {
                SessionController.Instance.EnterCollection(_controller.LibraryElementModel.LibraryElementId);
            }
        }

    public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            float vertical_spacing = 20;
            float horizontal_spacing = 20;

            // layout the collection grid view
            _collectionGridView.Width = Width - 2*horizontal_spacing;
            _collectionGridView.Height = Height - vertical_spacing - _enterCollectionButton.Height;
            _collectionGridView.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);

            vertical_spacing += _collectionGridView.Height + 20;

            _enterCollectionButton.Transform.LocalPosition = new Vector2(Width/2 - _enterCollectionButton.Width/2, vertical_spacing);



            base.Update(parentLocalToScreenTransform);
        }
    }
}
