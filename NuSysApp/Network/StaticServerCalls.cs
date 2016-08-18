using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using Windows.Foundation;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class StaticServerCalls
    {
        public static async Task<bool> CreateSnapshot()//returns true if successful
        {
            string libraryId = SessionController.Instance.ActiveFreeFormViewer.Controller.Model.LibraryId;
            return false;
            //return await SessionController.Instance.NuSysNetworkSession.DuplicateLibraryElement(libraryId) != null;
            var collectionLibraryController = SessionController.Instance.ContentController.GetLibraryElementController(libraryId) as CollectionLibraryElementController;
            if (collectionLibraryController == null)
            {
                return false;
            }

            var snapshotId = SessionController.Instance.GenerateId();

            var m = new Message();
            m["id"] = snapshotId;
            m["type"] = NusysConstants.ElementType.Collection.ToString();
            m["inklines"] = collectionLibraryController.InkLines;
            m["favorited"] = true;
            m["title"] = collectionLibraryController.LibraryElementModel.Title + " SNAPSHOT "+DateTime.Now;

            var libraryElementRequest = new CreateNewLibraryElementRequest(m);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(libraryElementRequest);

            var children = SessionController.Instance.IdToControllers.Where(item => item.Value.Model.ParentCollectionId == libraryId).ToArray();

            foreach (var child in children)
            {
                var dict = await child.Value.Model.Pack();
                dict["creator"] = snapshotId;
                dict["id"] = SessionController.Instance.GenerateId();
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(new NewElementRequest(new Message(dict)));
            }

            return true; 
        }

        /// <summary>
        /// this static method will add a collection element to your current collection.  
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="collectionLibraryElementId"></param>
        /// <param name="finite"></param>
        /// <param name="shapepoints"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="id"></param>
        /// <param name="collectionView"></param>
        /// <returns></returns>
        public static async Task<ElementCollectionController> PutCollectionInstanceOnMainCollection(double x, double y, string collectionLibraryElementId, bool finite, List<Point> shapepoints, double width = 400, double height = 400, string id = null, CollectionElementModel.CollectionViewType collectionView = CollectionElementModel.CollectionViewType.List)
        {
            return await Task.Run(async delegate
            {
                //make sure the collection id isn't bogus
                var collectionController = SessionController.Instance.ContentController.GetLibraryElementModel(collectionLibraryElementId) as CollectionLibraryElementModel;
                Debug.Assert(collectionController != null);

                var newId = id ?? SessionController.Instance.GenerateId();
                
                //TODO override the args class and make a CreateNewCollectionElementArgs and apply the settings of collections there
                //create the element args
                var args  = new NewElementRequestArgs();
                args.LibraryElementId = collectionLibraryElementId;
                args.Height = height;
                args.Width = width;
                args.ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId;
                args.X = x;
                args.Y = y;
                args.Id = newId;

                //create the request for the new element
                var elementRequest = new NewElementRequest(args);

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

                //add the collection element after the sucessful request
                await elementRequest.AddReturnedElementToSession();

                //if we haven't loaded this collection before
                if (!SessionController.Instance.ContentController.ContainsContentDataModel( collectionController.ContentDataModelId))
                {
                    //create a request to get the elements on the new collection
                    var getWorkspaceRequest = new GetEntireWorkspaceRequest(collectionLibraryElementId);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(getWorkspaceRequest);

                    var contentDataModels = getWorkspaceRequest.GetReturnedContentDataModels();
                    var elements = getWorkspaceRequest.GetReturnedElementModels();

                    //for each contentDataModel, add it to the contentController if it doesn't exist
                    foreach (var content in contentDataModels)
                    {
                        if (!SessionController.Instance.ContentController.ContainsContentDataModel(content.ContentId))
                        {
                            SessionController.Instance.ContentController.AddContentDataModel(content);
                        }
                    }

                    //make the collection
                    await SessionController.Instance.SessionView.MakeCollection(
                        elements.Select(element => new KeyValuePair<string, ElementModel>(element.Id, element))
                            .ToDictionary(k => k.Key, v => v.Value));

                }
                //get and return the element collection controller
                if (SessionController.Instance.IdToControllers.ContainsKey(newId))
                {
                    return (ElementCollectionController)SessionController.Instance.IdToControllers[newId];
                }
                return null;
            });
        }
    }
}
