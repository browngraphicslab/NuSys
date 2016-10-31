using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.Graphics.Canvas;
using NusysIntermediate;

namespace NuSysApp
{
    internal class DetailViewAliasesPage : RectangleUIElement
    {
        private ListViewUIElementContainer<ElementModel> _listView;
        private readonly BaseRenderItem _parent;
        private readonly ICanvasResourceCreatorWithDpi _resourceCreator;
        private readonly LibraryElementController _controller;

        private List<ElementModel> _aliasList;
        private bool _isLoading;

        public DetailViewAliasesPage(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator, LibraryElementController controller) :
            base(parent, resourceCreator)
        {
            _isLoading = true;
            _parent = parent;
            _resourceCreator = resourceCreator;
            _controller = controller;
        }
        
        public override async Task Load()
        {
            GetAliasesOfLibraryElementRequest req = new GetAliasesOfLibraryElementRequest(_controller.LibraryElementModel.LibraryElementId);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(req);
            _aliasList = req.GetReturnedElementModels();
            CreateAliasList(_parent, _resourceCreator);
            base.Load();
        }

        private void CreateAliasList(BaseRenderItem parent, ICanvasResourceCreatorWithDpi resourceCreator)
        {
            _listView = new ListViewUIElementContainer<ElementModel>(parent, resourceCreator);
            _listView.Height = Height;
            _listView.Width = Width;
            _listView.Transform.LocalPosition = new Vector2(0, 0);
            _listView.AddItems(_aliasList);
            _listView.Background = Colors.CadetBlue;

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
            creator.ColumnFunction = delegate (ElementModel el)
            {
                string CollectionId = el.ParentCollectionId;

                var collectionController =
                    SessionController.Instance.ContentController.GetLibraryElementController(CollectionId);
                return
                   SessionController.Instance.NuSysNetworkSession.GetDisplayNameFromUserId(el?.CreatorId);
            };

            ListTextColumn<ElementModel> lastEdited = new ListTextColumn<ElementModel>();
            lastEdited.Title = "Last Edited";
            lastEdited.RelativeWidth = 1;
            lastEdited.ColumnFunction = delegate (ElementModel el)
            {
                string CollectionId = el.ParentCollectionId;

                var collectionController =
                    SessionController.Instance.ContentController.GetLibraryElementController(CollectionId);
                return collectionController.LibraryElementModel.LastEditedTimestamp;
            };

            List<ListColumn<ElementModel>> cols = new List<ListColumn<ElementModel>>();
            cols.Add(title);
            cols.Add(creator);
            cols.Add(lastEdited);
            _listView.AddColumns(cols);

            AddChild(_listView);
        }

        public override void Update(Matrix3x2 parentLocalToScreenTransform)
        {
            _listView.Height = Height;
            _listView.Width = Width;
            base.Update(parentLocalToScreenTransform);
        }
    }
}