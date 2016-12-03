using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Microsoft.Graphics.Canvas.Brushes;

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
                        //Debug.Fail("this should never even be hit because this is not copyable"); //tODO, maybe have this just create a snapshot instead?
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
                    DataBytes = originalController.Data,
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


        /// <summary>
        /// Adds an element of elementType to the current collection, at the point on the collection directly under screenpoint. If the element can exist without any
        /// predefined content, such as an empty text node, or collection node, then lec can be a null argument and the empty element will be created. Otherwise if content
        /// is required, the associated library element controller must be passed in.
        /// </summary>
        /// <param name="screenPoint">Point on the screen the new element will be created directly under this point on the main collection</param>
        /// <param name="elementType">The type of the elementy we are going to create. Must be able to exist without predefined content if library element controller is null</param>
        /// <param name="lec">The libraryy element controller of the element we are going to add</param>
        public static async void AddElementToCurrentCollection(Vector2 screenPoint, NusysConstants.ElementType elementType, LibraryElementController lec = null)
        {
            // transform the passed in screenpoint to a point on the main collection
            var collectionPoint = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(screenPoint, SessionController.Instance.SessionView.FreeFormViewer.InitialCollection);
            var libraryElementId = lec?.LibraryElementModel.LibraryElementId; // variable to hold the library element id of the element we are adding
            var contentId = lec?.LibraryElementModel.ContentDataModelId; // variable to hold the content id of the element we are adding

            // if we are creating empty content
            if (lec == null)
            {
                // make sure that the element type requested can be empty, and filter out requests for non-content such as recording nodes
                switch (elementType)
                {
                    case NusysConstants.ElementType.Collection:
                        break;
                    case NusysConstants.ElementType.Text:
                        break;
                    case NusysConstants.ElementType.Tools:
                        Debug.Assert(false);
                        return; // return after this we are not creating content
                    case NusysConstants.ElementType.Recording:
                        AddRecordingNode(screenPoint);
                        return; // return after this we are not creating content
                    default:
                        Debug.Fail($"In order to add an element of type, {elementType} to the collection you must provide a library element controller");
                        return;
                }


                // Create a new content request
                var createNewContentRequestArgs = new CreateNewContentRequestArgs
                {
                    LibraryElementArgs = new CreateNewLibraryElementRequestArgs
                    {
                        AccessType =
                            SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType,
                        LibraryElementType = elementType,
                        Title = elementType == NusysConstants.ElementType.Collection ? "Unnamed Collection" : "Unnamed Text",
                        LibraryElementId = SessionController.Instance.GenerateId()
                    },
                    ContentId = SessionController.Instance.GenerateId()
                };

                // execute the content request
                var contentRequest = new CreateNewContentRequest(createNewContentRequestArgs);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(contentRequest);
                contentRequest.AddReturnedLibraryElementToLibrary();

                // get the library element id and content id for use outside of this if statement
                libraryElementId = createNewContentRequestArgs.LibraryElementArgs.LibraryElementId;
                contentId = createNewContentRequestArgs.ContentId;
            }


            // create a new add element to collection request
            var newElementRequestArgs = new NewElementRequestArgs
            {
                LibraryElementId = libraryElementId,
                ParentCollectionId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                Height = Constants.DefaultNodeSize,
                Width = Constants.DefaultNodeSize,
                X = collectionPoint.X,
                Y = collectionPoint.Y
            };

            // execute the add element to collection request
            var elementRequest = new NewElementRequest(newElementRequestArgs);

            await SessionController.Instance.NuSysNetworkSession.FetchContentDataModelAsync(contentId);

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(elementRequest);

            await elementRequest.AddReturnedElementToSessionAsync();
        }


        /// <summary>
        /// Adds a recording node to the NuSessionView at the passed in screenpoint
        /// </summary>
        /// <param name="screenPoint"></param>
        private static void AddRecordingNode(Vector2 screenPoint)
        {
            var newRecNode = new RecordingNode(SessionController.Instance.NuSessionView, SessionController.Instance.NuSessionView.ResourceCreator);
            newRecNode.Load();
            newRecNode.Transform.LocalPosition = screenPoint;
            SessionController.Instance.NuSessionView.AddChild(newRecNode);
        }
    }
}
