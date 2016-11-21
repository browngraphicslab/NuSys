﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    public class LibraryListUIElement : ResizeableWindowUIElement
    {
        public ListViewUIElementContainer<LibraryElementModel> libraryListView;

        private List<RectangleUIElement> _libraryDragElements;

        private bool _isDragVisible;

        private float _itemDropOffset = 10;

        public LibraryListUIElement(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
            : base(parent, resourceCreator)
        {

            InitializeLibraryList();
            AddChild(libraryListView);

            _libraryDragElements = new List<RectangleUIElement>();

            libraryListView.RowDragged += LibraryListView_RowDragged;
            libraryListView.RowDragCompleted += LibraryListView_RowDragCompleted;

            // events so that the library list view adds and removes elements dynamically
            SessionController.Instance.ContentController.OnNewLibraryElement += UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete += UpdateLibraryListToRemoveElement;
        }

        private void LibraryListView_RowDragCompleted(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {

            foreach (var rect in _libraryDragElements)
            {
                rect.Dispose();
                RemoveChild(rect);
            }
            _isDragVisible = false;

            // convert the current point of the drag event to a point on the collection
            var collectionPoint = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(
                                                            pointer.CurrentPoint, SessionController.Instance.SessionView.FreeFormViewer.InitialCollection);

            foreach (var lem in libraryListView.GetSelectedItems())
            {
                //Before we add the node, we need to check if the access settings for the library element and the workspace are incompatible
                // If they are different we simply return 
                var currWorkSpaceAccessType =
                    SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;

                // if the item is private and the workspace is public then continue
                if (item.AccessType == NusysConstants.AccessType.Private &&
                    currWorkSpaceAccessType == NusysConstants.AccessType.Public)
                {
                    continue;
                }

                // otherwise add the item to the workspace at the current point
                var libraryElementController =
                    SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId);
                libraryElementController.AddElementAtPosition(collectionPoint.X, collectionPoint.Y);

                // increment the collectionPoint by itemDropOffset
                collectionPoint += new Vector2(_itemDropOffset, _itemDropOffset);
              
            }
            


        }

        private async void LibraryListView_RowDragged(LibraryElementModel item, string columnName, CanvasPointer pointer)
        {
            if (_isDragVisible)
            {
                var position = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
                foreach (var element in _libraryDragElements)
                {
                    element.Transform.LocalPosition = position;
                    position += new Vector2(_itemDropOffset, _itemDropOffset);

                }

            }
            else
            {
                var position = Vector2.Transform(pointer.CurrentPoint, Transform.ScreenToLocalMatrix);
                _isDragVisible = true;
                foreach (var lem in libraryListView.GetSelectedItems())
                {
                    var controller= SessionController.Instance.ContentController.GetLibraryElementController(lem.LibraryElementId);
                    var rect = new RectangleUIElement(this, ResourceCreator);
                    rect.Image = await CanvasBitmap.LoadAsync(Canvas, controller.SmallIconUri);
                    rect.Transform.LocalPosition = position;
                    _libraryDragElements.Add(rect);
                    position += new Vector2(_itemDropOffset, _itemDropOffset);
                    AddChild(rect);

                }
            }
        }

        public override void Dispose()
        {

            SessionController.Instance.ContentController.OnNewLibraryElement -= UpdateLibraryListWithNewElement;
            SessionController.Instance.ContentController.OnLibraryElementDelete -= UpdateLibraryListToRemoveElement;
            base.Dispose();
        }

        public void InitializeLibraryList()
        {
            libraryListView = new ListViewUIElementContainer<LibraryElementModel>(this, Canvas)
            {
                MultipleSelections = true
            };

            var listColumn = new ListTextColumn<LibraryElementModel>();
            listColumn.Title = "Title";
            listColumn.RelativeWidth = 1;
            listColumn.ColumnFunction = model => model.Title;

            var listColumn2 = new ListTextColumn<LibraryElementModel>();
            listColumn2.Title = "Creator";
            listColumn2.RelativeWidth = 2;
            listColumn2.ColumnFunction =
                model => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(model.Creator);

            var listColumn3 = new ListTextColumn<LibraryElementModel>();
            listColumn3.Title = "Last Edited Timestamp";
            listColumn3.RelativeWidth = 3;
            listColumn3.ColumnFunction = model => model.LastEditedTimestamp;

            libraryListView.AddColumns(new List<ListColumn<LibraryElementModel>> { listColumn, listColumn2, listColumn3 });


            libraryListView.AddItems(
                           SessionController.Instance.ContentController.ContentValues.ToList());
           
        }

        private void UpdateLibraryListToRemoveElement(LibraryElementModel element)
        {
            libraryListView.RemoveItems(new List<LibraryElementModel> {element});
        }

        private void UpdateLibraryListWithNewElement(LibraryElementModel libraryElement)
        {
            libraryListView.AddItems(new List<LibraryElementModel> {libraryElement});
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            libraryListView.Width = Width - 2 * BorderWidth;
            libraryListView.Height = Height - TopBarHeight - BorderWidth;
            libraryListView.Transform.LocalPosition = new Vector2(BorderWidth, TopBarHeight);
            base.Update(parentLocalToScreenTransform);
        }
    }
}