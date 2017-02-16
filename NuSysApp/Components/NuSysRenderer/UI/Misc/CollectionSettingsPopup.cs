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
using Windows.UI;
using Microsoft.Graphics.Canvas.Text;
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
        /// title of popup for clarification :)
        /// </summary>
        private TextboxUIElement _titleBox;

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

            Background = Colors.White;
            BorderWidth = 2;
            BorderColor = Constants.DARK_BLUE;
            Width = 600;
            Height = 500;

            _titleBox = new TextboxUIElement(this, resourceCreator)
            {
                Text = "Collection Settings",
                FontSize = 20,
                TextHorizontalAlignment = CanvasHorizontalAlignment.Center,
                TextVerticalAlignment = CanvasVerticalAlignment.Center
            };
            AddChild(_titleBox);

            _boundedButton = new RectangleButtonUIElement(this, resourceCreator);
            _boundedButton.Width = Width/2;
            AddChild(_boundedButton);

            _editShapeButton = new RectangleButtonUIElement(this, resourceCreator, text: "Edit Shape");
            _editShapeButton.Width = Width/2;
            AddChild(_editShapeButton);

            _collectionController.FiniteBoolChanged += CollectionControllerOnFiniteBoolChanged;
            _collectionContentController.ContentDataUpdated += CollectionContentControllerOnContentDataUpdated;

            _boundedButton.Tapped += BoundedButton_Tapped;
            _editShapeButton.Tapped += EditShapeButton_Tapped;

            UpdateText();
        }

        private void EditShapeButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {         
            EditShapePopup popup = new EditShapePopup(Parent, Canvas, _collectionController);
            Parent.AddChild(popup);
            popup.Height = Height;
            if (_collectionController.CollectionContentDataController.CollectionModel.Shape != null)
            {
                popup.Height = Height + 70;
            }
            popup.Width = Width;
            popup.Transform.LocalPosition = Transform.LocalPosition;
            DismissPopup();            
        }

        private void BoundedButton_Tapped(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            if (_collectionController.CollectionModel.IsFinite)
            {
                _collectionController.SetFiniteBoolean(false);
            }
            else
            {
                _collectionController.SetFiniteBoolean(true);
            }

            Debug.WriteLine(_collectionController.CollectionModel.IsFinite);
            
            DismissPopup();
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
            _editShapeButton.Width = Width/2;
            _boundedButton.Width = Width/2;
            _editShapeButton.Transform.LocalPosition = new Vector2(Width/2 - _editShapeButton.Width/2, Height*2/5);
            _boundedButton.Transform.LocalPosition = new Vector2(Width/2 - _boundedButton.Width/2, Height - Height*2/5 + 10);
            _titleBox.Width = Width;
            _titleBox.Height = Height/5;
            _titleBox.Transform.LocalPosition = new Vector2(Width/2 - _titleBox.Width/2, Height/8);
            base.Update(parentLocalToScreenTransform);
        }


        /// <summary>
        /// dispose method shold mainly remove the vent handlers added
        /// </summary>
        public override void Dispose()
        {
            _collectionController.FiniteBoolChanged -= CollectionControllerOnFiniteBoolChanged;
            _collectionContentController.ContentDataUpdated -= CollectionContentControllerOnContentDataUpdated;
            _boundedButton.Tapped -= BoundedButton_Tapped;
            _editShapeButton.Tapped -= EditShapeButton_Tapped;
            base.Dispose();
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
            if(_collectionContentController.CollectionModel.Shape?.ImageUrl != null)
            {
                _collectionContentController.ClearShape();
                _collectionContentController.SetShapePoints(new List<PointModel>
                {
                    new PointModel(50000,50000),
                    new PointModel(50000,51000),
                    new PointModel(50000,52000),
                    new PointModel(52000,52000),
                    new PointModel(52000,51000),
                    new PointModel(52000,50000)
                });
            }
        }
    }
}
