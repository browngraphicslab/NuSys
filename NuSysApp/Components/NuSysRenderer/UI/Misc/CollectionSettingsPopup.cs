using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using System.Diagnostics;
using System.Numerics;
using Windows.Storage;
using Windows.Storage.FileProperties;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Class to set the collection settings of a given collection.
    /// </summary>
    public class CollectionSettingsPopup : PopupUIElement
    {
        /// <summary>
        /// the collection controller that we are modifying in this class
        /// </summary>
        private CollectionLibraryElementController _collectionController;

        /// <summary>
        /// the collection content data controller for the given library element controller.
        /// This getter will debug.assert that the returned object isn't null;
        /// </summary>
        private CollectionContentDataController _collectionContentController
        {
            get
            {
                Debug.Assert(_collectionController?.CollectionContentDataController != null);
                return _collectionController?.CollectionContentDataController;
            }
        }

        /// <summary>
        /// The  button that will be used to change the bounded boolean of the collection
        /// </summary>
        private ButtonUIElement _boundedButton;

        /// <summary>
        /// the button used to edit the shape of the collection
        /// </summary>
        private ButtonUIElement _editShapeButton;

        /// <summary>
        /// This popup will be used to set the settings of a collection content.
        /// Pass in the usual render item parameters as well as the CollectionLibraryElementController you wish to edit.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="resourceCreator"></param>
        /// <param name="controller"></param>
        public CollectionSettingsPopup(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, CollectionLibraryElementController controller) : base(parent, resourceCreator)
        {
            Debug.Assert(controller != null);
            _collectionController = controller;

            _boundedButton = new RectangleButtonUIElement(this,resourceCreator);
            AddChild(_boundedButton);

            _editShapeButton = new RectangleButtonUIElement(this, resourceCreator, text:"Edit Shape");
            AddChild(_editShapeButton);

            Background = Constants.LIGHT_BLUE;
            Width = 500;
            Height = 550;
            Transform.LocalX = 150;

            _collectionController.FiniteBoolChanged += CollectionControllerOnFiniteBoolChanged;
            _collectionContentController.ContentDataUpdated += CollectionContentControllerOnContentDataUpdated;

            UpdateText();
        }

        /// <summary>
        /// Event handler fired whenever the conentDataModel for the collection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="s"></param>
        private void CollectionContentControllerOnContentDataUpdated(object sender, string s)
        {
            UpdateText();
        }

        /// <summary>
        /// event handler for when the finit bool of the collection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="b"></param>
        private void CollectionControllerOnFiniteBoolChanged(object sender, bool b)
        {
            UpdateText();
        }

        /// <summary>
        /// private method used to make sure the buttons and such are correctly updated with the latest information
        /// </summary>
        private void UpdateText()
        {
            _boundedButton.ButtonText = "Make "+(_collectionController.CollectionModel.IsFinite ? "Not " : "")+"Bounded";
        }

        /// <summary>
        /// Overriding the update method to update the locations of the buttons
        /// </summary>
        /// <param name="parentLocalToScreenTransform"></param>
        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _editShapeButton.Width = Width - BorderWidth * 2;
            _boundedButton.Width = Width - BorderWidth * 2;
            _editShapeButton.Transform.LocalPosition = new Vector2(BorderWidth,_editShapeButton.Height + _boundedButton.Height * 2);
            _boundedButton.Transform.LocalPosition = new Vector2(BorderWidth,_boundedButton.Height / 2);
            Height = (float)(_boundedButton.Height*1.5 + _boundedButton.Transform.LocalY);
            base.Update(parentLocalToScreenTransform);
        }


        /// <summary>
        /// dispose method shold mainly remove the vent handlers added
        /// </summary>
        public override void Dispose()
        {
            _collectionController.FiniteBoolChanged -= CollectionControllerOnFiniteBoolChanged;
            _collectionContentController.ContentDataUpdated -= CollectionContentControllerOnContentDataUpdated;
            base.Dispose();
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
            _collectionContentController.SetShapeUrl(url,aspectRatio);
        }

        /// <summary>
        /// Method to call to set the color of the collection background.
        /// If you want to remove a pre-existing background image as well, set removeBackgroundImage to true;
        /// </summary>
        /// <param name="color"></param>
        /// <param name="removeAnyBackgroundImage"></param>
        private void SetShapeColor(ColorModel color, bool removeBackgroundImage = true)
        {
            if (removeBackgroundImage)
            {
                ClearCollectionImage();
            }
            _collectionContentController.SetShapeColor(color);
        }

        /// <summary>
        /// This method will remove the image of the collection if it exists already.
        /// </summary>
        private void ClearCollectionImage()
        {
            if(_collectionContentController.CollectionModel.Shape.ImageUrl != null)
            {
                _collectionContentController.ClearShape();
                _collectionContentController.SetShapePoints(new List<PointModel>()
                {
                    new PointModel(50000,50000),
                    new PointModel(50000,51000),
                    new PointModel(50000,52000),
                    new PointModel(52000,52000),
                    new PointModel(52000,51000),
                    new PointModel(52000,50000),
                });
            }
        }
    }
}
