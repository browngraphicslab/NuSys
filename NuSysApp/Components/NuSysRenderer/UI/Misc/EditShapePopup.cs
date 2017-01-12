using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System.Numerics;
using Windows.Foundation;
using MyToolkit.Composition;
using Windows.Storage;
using Windows.Storage.FileProperties;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Popup for second part of Collection Settings Popup - allows user to edit the shape by either picking an image as the backgroudn or 
    /// choosing a color for the shape
    /// </summary>
    public class EditShapePopup : PopupUIElement
    {
        /// <summary>
        /// button to choose color of shape
        /// </summary>
        private RectangleButtonUIElement _colorButton;

        /// <summary>
        /// button to choose image of shape, if wanted
        /// </summary>
        private RectangleButtonUIElement _imageButton;

        /// <summary>
        /// title box of this pop up 
        /// </summary>
        private TextboxUIElement _titleBox;

        /// <summary>
        /// variable stores collection controller so we can act on the current element in the detail view
        /// </summary>
        private CollectionLibraryElementController _collectionController;

        /// <summary>
        /// button to delete the shape. only should be added as a child if there is a shape.
        /// </summary>
        private RectangleButtonUIElement _deleteButton;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        public EditShapePopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            _collectionController = controller;

            Background = Colors.White;
            BorderWidth = 2;
            Bordercolor = Constants.DARK_BLUE;
            Width = 600;
            Height = 500;

            _colorButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, "Color");
            AddChild(_colorButton);

            _imageButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, "Image");
            AddChild(_imageButton);

            _titleBox = new TextboxUIElement(this, resourceCreator)
            {
                Text = "Edit Shape",
                FontSize = 20,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center
            };
            AddChild(_titleBox);

            if (_collectionController.CollectionContentDataController.CollectionModel.Shape != null)
            {
                _deleteButton = new RectangleButtonUIElement(this, resourceCreator, UIDefaults.PrimaryStyle, "Delete");
                AddChild(_deleteButton);

                _deleteButton.Tapped += DeleteButton_Tapped;
            }

            _colorButton.Tapped += ColorButton_Tapped;
            _imageButton.Tapped += ImageButton_Tapped;
        }

        private void DeleteButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            _collectionController.CollectionContentDataController.ClearShape();
            DismissPopup();
        }

        private void ImageButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            SelectAndSaveImage();
        }

        /// <summary>
        /// Method to call when you want to select and safve a url for the collection
        /// </summary>
        private async Task SelectAndSaveImage()
        {
            var storageFiles = await FileManager.PromptUserForFiles(Constants.ImageFileTypes, singleFileOnly: true);
            var file = new List<StorageFile>(storageFiles).First();
            var bytes = await MediaUtil.StorageFileToByteArray(file);

            var thumb = await file.GetThumbnailAsync(ThumbnailMode.SingleItem, 300);
            var aspectRatio = thumb.OriginalWidth / (double)thumb.OriginalHeight;

            var args = new UploadCollectionBackgroundImageServerRequestArgs()
            {
                FileExtension = file.FileType,
                ImageBytes = bytes
            };
            var request = new UploadCollectionBackgroundImageRequest(args);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            Debug.Assert(request.WasSuccessful() == true);
            var url = request.GetReturnedImageUrl();
            _collectionController.CollectionContentDataController.SetShapeUrl(url, aspectRatio);
        }


        private void ColorButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            var popup = new ColorPopup(Parent, Canvas, _collectionController);
            Parent.AddChild(popup);
            popup.Height = Height;
            popup.Width = Width;
            popup.Transform.LocalPosition = Transform.LocalPosition;
            DismissPopup();
        }

        /// <summary>
        /// update sets positions and widths of the popup
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_deleteButton != null)
            {
                _titleBox.Width = Width;
                _titleBox.Height = Height / 8;
                _titleBox.Transform.LocalPosition = new Vector2(Width / 2 - _titleBox.Width / 2, Height / 10);
                _colorButton.Width = Width/2;
                _imageButton.Width = Width/2;
                _colorButton.Transform.LocalPosition = new Vector2(Width/2 - _colorButton.Width/2, Height*2/7);
                _deleteButton.Width = Width/2;
                _deleteButton.Transform.LocalPosition = new Vector2(_colorButton.Transform.LocalX,
                    _colorButton.Transform.LocalY + _colorButton.Height + 10);
                _imageButton.Transform.LocalPosition = new Vector2(Width / 2 - _imageButton.Width / 2,
                    _deleteButton.Transform.LocalY + _deleteButton.Height + 10);
            }
            else
            {
                _colorButton.Width = Width/2;
                _imageButton.Width = Width/2;
                _colorButton.Transform.LocalPosition = new Vector2(Width/2 - _colorButton.Width/2, Height*2/5);
                _imageButton.Transform.LocalPosition = new Vector2(Width/2 - _imageButton.Width/2,
                    _colorButton.Transform.LocalY + _colorButton.Height + 10);
                _titleBox.Width = Width;
                _titleBox.Height = Height / 5;
                _titleBox.Transform.LocalPosition = new Vector2(Width / 2 - _titleBox.Width / 2, Height / 8);
            }

            base.Update(parentLocalToScreenTransform);
        }

        public override void Dispose()
        {
            if (_deleteButton != null)
            {
                _deleteButton.Tapped -= DeleteButton_Tapped;
            }
            _colorButton.Tapped -= ColorButton_Tapped;
            _imageButton.Tapped -= ImageButton_Tapped;
            base.Dispose();
        }

    }
}
