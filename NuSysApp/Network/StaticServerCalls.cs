using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class StaticServerCalls
    {
        public static async Task<bool> CreateSnapshot()//returns true if successful
        {
            string libraryId = SessionController.Instance.ActiveFreeFormViewer.Controller.Model.LibraryId;
            return await SessionController.Instance.NuSysNetworkSession.DuplicateLibraryElement(libraryId) != null;
            var collectionLibraryModel = SessionController.Instance.ContentController.GetContent(libraryId) as CollectionLibraryElementModel;
            if (collectionLibraryModel == null)
            {
                return false;
            }

            var snapshotId = SessionController.Instance.GenerateId();

            var m = new Message();
            m["id"] = snapshotId;
            m["type"] = ElementType.Collection.ToString();
            m["inklines"] = collectionLibraryModel.InkLines;
            m["favorited"] = true;
            m["title"] = collectionLibraryModel.Title + " SNAPSHOT "+DateTime.Now;

            var libraryElementRequest = new CreateNewLibraryElementRequest(m);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(libraryElementRequest);

            var children = SessionController.Instance.IdToControllers.Where(item => item.Value.Model.ParentCollectionId == libraryId).ToArray();

            foreach (var child in children)
            {
                var dict = await child.Value.Model.Pack();
                dict["creator"] = snapshotId;
                dict["id"] = SessionController.Instance.GenerateId();
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(new Message(dict)));
            }

            return true; 
        }
        public static async Task<ElementCollectionController> PutCollectionInstanceOnMainCollection(double x, double y, string contentID, bool finite, PointCollection shapepoints, double width = 400, double height = 400, string id = null, CollectionElementModel.CollectionViewType collectionView = CollectionElementModel.CollectionViewType.List)
        {
            return await Task.Run(async delegate
            {
                var newId = id ?? SessionController.Instance.GenerateId();

                Message message = new Message();
                message["contentId"] = contentID;
                message["x"] = x;
                message["y"] = y;
                message["width"] = width;
                message["height"] = height;
                message["type"] = ElementType.Collection;
                message["collectionview"] = collectionView;
                message["creator"] = SessionController.Instance.ActiveFreeFormViewer.ContentId;
                message["id"] = newId;
                message["finite"] = finite;
                message["points"] = shapepoints;

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewElementRequest(message));

                List<Message> messages = new List<Message>();

                await Task.Run(async delegate
                {
                    messages = await SessionController.Instance.NuSysNetworkSession.GetCollectionAsElementMessages(contentID);
                });

                foreach (var m in messages)
                {
                    if (m.ContainsKey("contentId"))
                    {
                        await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewElementRequest(m));
                        var newNodeContentId = m.GetString("contentId");
                        if (SessionController.Instance.ContentController.GetContent(newNodeContentId) == null)
                        {
                            Task.Run(async delegate
                            {
                                SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(newNodeContentId);
                            });
                        }
                    }

                }
                if (SessionController.Instance.IdToControllers.ContainsKey(newId))
                {
                    return (ElementCollectionController)SessionController.Instance.IdToControllers[newId];
                }
                return null;
            });
        }
    }
}
