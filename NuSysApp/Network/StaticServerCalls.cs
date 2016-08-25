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
        /// This takes in a NewElementRequestArgs class. 
        /// The args will automatically be set to put an element on the current collection, so don't worry about setting it. 
        /// The library element Id for this new collction element must be set to a valid, existing library element.
        /// The m
        /// </summary>
        /// <param name="requestArgs"></param>
        /// <returns></returns>

        public static async Task<ElementCollectionController> PutCollectionInstanceOnMainCollection(NewElementRequestArgs requestArgs)
        {
            return await Task.Run(async delegate
            {
                requestArgs.ParentCollectionId =SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId; //set the parent collection to be this one
                requestArgs.Id = requestArgs.Id ?? SessionController.Instance.GenerateId(); //make sure the request args has a defined new element Id

                //make sure the collection id isn't bogus
                var collectionController = SessionController.Instance.ContentController.GetLibraryElementModel(requestArgs.LibraryElementId) as CollectionLibraryElementModel;
                Debug.Assert(collectionController != null);

                //TODO override the args class and make a CreateNewCollectionElementArgs and apply the settings of collections there

                //create the request for the new element
                var elementRequest = new NewElementRequest(requestArgs);

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

                //add the collection element after the sucessful request
                await elementRequest.AddReturnedElementToSessionAsync();

                //if we haven't loaded this collection before
                if (!SessionController.Instance.ContentController.ContainsContentDataModel( collectionController.ContentDataModelId))
                {
                    //create a request to get the elements on the new collection
                    var getWorkspaceRequest = new GetEntireWorkspaceRequest(requestArgs.ParentCollectionId);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(getWorkspaceRequest);
                    await getWorkspaceRequest.AddReturnedDataToSessionAsync();
                    await getWorkspaceRequest.MakeCollectionFromReturnedElementsAsync();
                }
                //get and return the element collection controller
                if (SessionController.Instance.IdToControllers.ContainsKey(requestArgs.Id))
                {
                    return (ElementCollectionController)SessionController.Instance.IdToControllers[requestArgs.Id];
                }
                return null;
            });
        }

        /// <summary>
        /// This method can be used to create a copy of a library element which will appear in the library as 
        /// a separate entity. Takes in the library id of the element to be copied. 
        /// Returns the Library Id of the newly created copy.
        /// </summary>
        /// <param name="libraryElementId"></param>
        public static async Task<string> CreateDeepCopy(string libraryElementId)
        {
            var originalController = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementId);
            // Generate a new content Id (only for text) and library Id for the copy
            var newContentId = SessionController.Instance.GenerateId();
            var newLibraryId = SessionController.Instance.GenerateId();

            if (originalController.LibraryElementModel.Type != NusysConstants.ElementType.Text)
            {
                //Create and execute the new Library element request
                var newLibraryElementRequestArgs = new CreateNewLibraryElementRequestArgs()
                {
                    Title = originalController.Title + " copy",
                    ContentId = originalController.LibraryElementModel.ContentDataModelId,
                    AccessType = originalController.LibraryElementModel.AccessType,
                    LibraryElementType = originalController.LibraryElementModel.Type,
                    LibraryElementId = newLibraryId,
                    Small_Thumbnail_Url = originalController.SmallIconUri.AbsoluteUri,
                    Medium_Thumbnail_Url = originalController.MediumIconUri.AbsoluteUri,
                    Large_Thumbnail_Url = originalController.LargeIconUri.AbsoluteUri
                };
                var newLibraryElementRequest = new CreateNewLibraryElementRequest(newLibraryElementRequestArgs);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(newLibraryElementRequest);
                newLibraryElementRequest.AddReturnedLibraryElementToLibrary();
                return newLibraryId;
            }
            else
            {
                // For text elements, we make a copy of both the contnt data model and the library element model
                // Create and execute a new content request that carries the same data as the content we are trying to make a 
                // copy of.
                var newContentRequestArgs = new CreateNewContentRequestArgs()
                {
                    ContentId = newContentId,
                    DataBytes = originalController.ContentDataController.ContentDataModel.Data,
                    LibraryElementArgs = new CreateNewLibraryElementRequestArgs()
                    {
                        Title = originalController.Title + " copy",
                        ContentId = newContentId,
                        AccessType = originalController.LibraryElementModel.AccessType,
                        LibraryElementType = NusysConstants.ElementType.Text,
                        LibraryElementId = newLibraryId
                    }
                };

                var newContentRequest = new CreateNewContentRequest(newContentRequestArgs);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(newContentRequest);
                newContentRequest.AddReturnedLibraryElementToLibrary();
                return newLibraryId;
                
            }
            
        }
    }
}
