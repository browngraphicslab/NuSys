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


                //if we haven't loaded this collection before
                if (!SessionController.Instance.ContentController.ContainsContentDataModel(collectionController.ContentDataModelId))
                {
                    //create a request to get the elements on the new collection
                    var getWorkspaceRequest = new GetEntireWorkspaceRequest(collectionController.LibraryElementId, 1);
                    await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(getWorkspaceRequest);
                    await getWorkspaceRequest.AddReturnedDataToSessionAsync();
                    await getWorkspaceRequest.MakeCollectionFromReturnedElementsAsync();
                }

                //add the collection element after the sucessful request
                await elementRequest.AddReturnedElementToSessionAsync();

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
                CreateNewLibraryElementRequestArgs args;

                switch (originalController.LibraryElementModel.Type)
                {
                    case NusysConstants.ElementType.Image:
                        var imageArgs = new CreateNewImageLibraryElementRequestArgs();
                        var imageModel = originalController?.LibraryElementModel as ImageLibraryElementModel;
                        Debug.Assert(imageModel != null);
                        imageArgs.AspectRatio = imageModel.Ratio;
                        imageArgs.NormalizedHeight = imageModel.NormalizedHeight;
                        imageArgs.NormalizedWidth = imageModel.NormalizedWidth;
                        imageArgs.NormalizedX = imageModel.NormalizedX;
                        imageArgs.NormalizedY = imageModel.NormalizedY;
                        imageArgs.ParentLibraryElementId = imageModel.ParentId;
                        args = imageArgs;
                        break;
                    case NusysConstants.ElementType.Video:
                        var videoArgs = new CreateNewVideoLibraryElementRequestArgs();
                        var videoModel = originalController?.LibraryElementModel as VideoLibraryElementModel;
                        Debug.Assert(videoModel != null);
                        videoArgs.AspectRatio = videoModel.Ratio;
                        videoArgs.StartTime = videoModel.NormalizedStartTime;
                        videoArgs.Duration = videoModel.NormalizedDuration;
                        videoArgs.ParentLibraryElementId = videoModel.ParentId;
                        args = videoArgs;
                        break;
                    case NusysConstants.ElementType.Audio:
                        var audioArgs = new CreateNewAudioLibraryElementRequestArgs();
                        var audioModel = originalController?.LibraryElementModel as AudioLibraryElementModel;
                        Debug.Assert(audioModel != null);
                        audioArgs.StartTime = audioModel.NormalizedStartTime;
                        audioArgs.Duration = audioModel.NormalizedDuration;
                        audioArgs.ParentLibraryElementId = audioModel.ParentId;
                        args = audioArgs;
                        break;
                    case NusysConstants.ElementType.PDF:
                        var pdfArgs = new CreateNewPdfLibraryElementModelRequestArgs();
                        var pdfModel = originalController?.LibraryElementModel as PdfLibraryElementModel;
                        Debug.Assert(pdfModel != null);
                        pdfArgs.NormalizedHeight = pdfModel.NormalizedHeight;
                        pdfArgs.NormalizedWidth = pdfModel.NormalizedWidth;
                        pdfArgs.NormalizedX = pdfModel.NormalizedX;
                        pdfArgs.NormalizedY = pdfModel.NormalizedY;
                        pdfArgs.PdfPageStart = pdfModel.PageStart;
                        pdfArgs.PdfPageEnd = pdfModel.PageEnd;
                        pdfArgs.ParentLibraryElementId = pdfModel.ParentId;
                        args = pdfArgs;
                        break;
                    default:
                        Debug.Fail("this should never even be hit because this is not copyable");
                        return "";
                        break;
                }
                args.Title = originalController.Title + " copy";
                args.ContentId = originalController.LibraryElementModel.ContentDataModelId;
                args.AccessType = originalController.LibraryElementModel.AccessType;
                args.LibraryElementType = originalController.LibraryElementModel.Type;
                args.LibraryElementId = newLibraryId;
                args.Small_Thumbnail_Url = originalController.SmallIconUri.AbsoluteUri;
                args.Medium_Thumbnail_Url = originalController.MediumIconUri.AbsoluteUri;
                args.Large_Thumbnail_Url = originalController.LargeIconUri.AbsoluteUri;
                args.Metadata = new List<NusysIntermediate.MetadataEntry>(originalController.FullMetadata.Values);

                var newLibraryElementRequest = new CreateNewLibraryElementRequest(args);
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
