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

    public class CollectionGridViewUIElement : RectangleUIElement
    {
        /// <summary>
        /// The library element controller associated with this collection grid view ui element
        /// </summary>
        private LibraryElementController _controller;

        /// <summary>
        /// the image rect used to display an icon of the element on the grid view ui element
        /// </summary>
        private RectangleUIElement _imageRect;

        /// <summary>
        /// The label used to metadata about the element in the grid view
        /// </summary>
        private TextboxUIElement _metadataLabel;

        /// <summary>
        /// private helper variable for the DisplayMetadata property
        /// </summary>
        private DetailViewCollectionGridView.GridSortOption _displayMetadata { get; set; }

        /// <summary>
        /// Use this to set the type of metadata to display on the ui element label
        /// </summary>
        public DetailViewCollectionGridView.GridSortOption DisplayMetadata {
            get { return _displayMetadata; }
            set
            {
                _displayMetadata = value;
                DisplayMetadataLabel(DisplayMetadata);
            }
        }

        public CollectionGridViewUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) : base(parent, resourceCreator)
        {
            _controller = controller;

            // UI Defaults
            DisplayMetadata = DetailViewCollectionGridView.GridSortOption.Title; // automatically display titles
            BorderWidth = 2;
            BorderColor = Colors.Black;

            _imageRect = new RectangleUIElement(this, resourceCreator)
            {
                IsHitTestVisible = false
            };
            AddChild(_imageRect);

            _metadataLabel = new TextboxUIElement(this, resourceCreator)
            {
                Text = _controller?.LibraryElementModel?.Title,
                IsHitTestVisible = false,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center
            };
            AddChild(_metadataLabel);

            _controller.TitleChanged += _controller_TitleChanged;
            DoubleTapped += CollectionGridViewUIElement_DoubleTapped;
        }


        private void CollectionGridViewUIElement_DoubleTapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SessionController.Instance.NuSessionView.ShowDetailView(_controller);
        }

        public override void Dispose()
        {
            _controller.TitleChanged -= _controller_TitleChanged;
            DoubleTapped -= CollectionGridViewUIElement_DoubleTapped;

            base.Dispose();
        }

        private void _controller_TitleChanged(object sender, string title)
        {
            if (DisplayMetadata == DetailViewCollectionGridView.GridSortOption.Title)
            {
                _metadataLabel.Text = title;
            }
        }

        /// <summary>
        /// Displays the proper metadata label on the GridViewUIElement
        /// </summary>
        /// <param name="metaDataToDisplay"></param>
        private void DisplayMetadataLabel(DetailViewCollectionGridView.GridSortOption metaDataToDisplay)
        {
            // don't do anything if the metadata label is null
            if (_metadataLabel == null)
            {
                return;
            }

            switch (metaDataToDisplay)
            {
                case DetailViewCollectionGridView.GridSortOption.Title:
                    _metadataLabel.Text = _controller.Title;
                    break;
                case DetailViewCollectionGridView.GridSortOption.Date:
                    _metadataLabel.Text = _controller.GetCreationDate().ToString("MM/DD/YY");
                    break;
                case DetailViewCollectionGridView.GridSortOption.Creator:
                    _metadataLabel.Text =
                        SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(
                            _controller.LibraryElementModel.Creator);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(metaDataToDisplay), metaDataToDisplay, null);
            }
        }

        public override async Task Load()
        {
            Task.Run(async delegate {
                _imageRect.Image = await CanvasBitmap.LoadAsync(Canvas, _controller.SmallIconUri);
            });
            base.Load();
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {

            // layout the metadata label on the bottom of the ui element
            var borderSpacing = 5;
            _metadataLabel.Height = 20;
            _metadataLabel.Width = Width - 2*BorderWidth - 2*borderSpacing;
            _metadataLabel.Transform.LocalPosition = new Vector2(BorderWidth + borderSpacing, Height - borderSpacing - BorderWidth - _metadataLabel.Height);

            // layout the imageRect above the metadata label
            _imageRect.Height = Height - 3*borderSpacing - 2*BorderWidth - _metadataLabel.Height;
            _imageRect.Width = Width - 2 * BorderWidth - 2 * borderSpacing;
            _imageRect.Transform.LocalPosition = new Vector2(BorderWidth + borderSpacing);

            base.Update(parentLocalToScreenTransform);
        }
    }
}
