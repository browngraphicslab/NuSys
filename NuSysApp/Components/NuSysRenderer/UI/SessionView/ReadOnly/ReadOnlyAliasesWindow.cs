using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;
using System.Numerics;

namespace NuSysApp
{
    public class ReadOnlyAliasesWindow : ReadOnlyModeWindow
    {
       
        /// <summary>
        /// list view for the aliases
        /// </summary>
        private ListViewUIElementContainer<ElementModel> _listView;

        /// <summary>
        /// The controller for the element currently being displayed in this window.
        /// </summary>
        private LibraryElementController _controller;

        /// <summary>
        /// The list of aliases, requests from the server async
        /// </summary>
        private List<ElementModel> _aliasList;

        public ReadOnlyAliasesWindow(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator) : base(parent, resourceCreator)
        {

        }

        public void UpdateList(LibraryElementController controller)
        {
            if (_controller != null)
            {
                _controller.AliasAdded += OnAliasAdded;
                _controller.AliasRemoved += OnAliasRemoved;
            }
            
            _controller = controller;

            // add events for the controller so that aliases are automatically added and removed from the list
            _controller.AliasAdded += OnAliasAdded;
            _controller.AliasRemoved += OnAliasRemoved;
        }

        private void ListView_OnRowDoubleTapped(ElementModel item, string columnName, CanvasPointer pointer)
        {
            // get collection library element controller for the parent of the alias
            var itemParentCollectionController =
                SessionController.Instance.ContentController.GetLibraryElementController(item.ParentCollectionId) as
                    CollectionLibraryElementController;
            Debug.Assert(itemParentCollectionController != null);

            // get the element controller of the elementModel we clicked on
            var elementController = itemParentCollectionController.CollectionModel.Children.Where(id => SessionController.Instance.ElementModelIdToElementController.ContainsKey(id) && id == item.Id).Select(id => SessionController.Instance.ElementModelIdToElementController[id]).FirstOrDefault();
            Debug.Assert(elementController != null);


            // if the item is in the current collection
            if (itemParentCollectionController.LibraryElementModel == SessionController.Instance.CurrentCollectionLibraryElementModel)
            {
                UITask.Run(() =>
                {
                    // move the camera to the element the crumb represents
                    SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.CenterCameraOnElement(
                        elementController.Id);
                });
            }
            else
            {
                // otherwise enter the collection and try to zoom in on the element model that the crumb represents
                UITask.Run(() =>
                {
                    SessionController.Instance.EnterCollection(itemParentCollectionController.LibraryElementModel.LibraryElementId, elementController?.Id);
                });
            }
        }

        private void OnAliasRemoved(object sender, ElementModel e)
        {
            _listView.RemoveItems(new List<ElementModel> { e });
        }

        private void OnAliasAdded(object sender, ElementModel e)
        {
            _listView.AddItems(new List<ElementModel> { e });
        }

        public override void Dispose()
        {
            _controller.AliasAdded -= OnAliasAdded;
            _controller.AliasRemoved -= OnAliasRemoved;
            _listView.RowDoubleTapped -= ListView_OnRowDoubleTapped;


            base.Dispose();
        }

        /// <summary>
        /// Get the list of aliases async
        /// </summary>
        /// <returns></returns>
        public override async Task Load()
        {
            if (_controller == null)
            {
                return;
            }
            var req = new GetAliasesOfLibraryElementRequest(_controller.LibraryElementModel.LibraryElementId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(req);
            _aliasList = req.GetReturnedElementModels();
            CreateAliasList();
            base.Load();
        }

        private void CreateAliasList()
        {
            _listView = new ListViewUIElementContainer<ElementModel>(this, ResourceCreator);
            _listView.AddItems(_aliasList);

            ListTextColumn<ElementModel> title = new ListTextColumn<ElementModel>();
            title.Title = "Collection Title";
            title.RelativeWidth = 1;
            title.ColumnFunction = delegate (ElementModel el)
            {
                string CollectionId = el.ParentCollectionId;
                var collectionController =
                    SessionController.Instance.ContentController.GetLibraryElementController(CollectionId);
                return collectionController.Title;
            };

            ListTextColumn<ElementModel> creator = new ListTextColumn<ElementModel>();
            creator.Title = "Creator";
            creator.RelativeWidth = 1;
            creator.ColumnFunction = el => SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(el?.CreatorId);

            ListTextColumn<ElementModel> lastEdited = new ListTextColumn<ElementModel>();
            lastEdited.Title = "Last Edited";
            lastEdited.RelativeWidth = 1;
            lastEdited.ColumnFunction = delegate (ElementModel el)
            {
                var collectionController =
                    SessionController.Instance.ContentController.GetLibraryElementController(el.ParentCollectionId);
                return collectionController.LibraryElementModel.LastEditedTimestamp;
            };

            List<ListColumn<ElementModel>> cols = new List<ListColumn<ElementModel>>();
            cols.Add(title);
            cols.Add(creator);
            cols.Add(lastEdited);
            _listView.AddColumns(cols);

            AddChild(_listView);

            _listView.RowDoubleTapped += ListView_OnRowDoubleTapped;

        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            if (_listView == null)
            {
                return;
            }
            var vertical_spacing = 20;
            var horizontal_spacing = 20;

            _listView.Height = Height - 2 * vertical_spacing;
            _listView.Width = Width - 2 * horizontal_spacing;
            _listView.Transform.LocalPosition = new Vector2(horizontal_spacing, vertical_spacing);
            base.Update(parentLocalToScreenTransform);
        }
    }
}

