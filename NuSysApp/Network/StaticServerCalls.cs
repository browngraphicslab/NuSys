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
                if (SessionController.Instance.ElementModelIdToElementController.ContainsKey(requestArgs.Id))
                {
                    return (ElementCollectionController)SessionController.Instance.ElementModelIdToElementController[requestArgs.Id];
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
        public static async Task<string> CreateDeepCopy(string libraryElementId, NusysConstants.AccessType access = NusysConstants.AccessType.Public)
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
                        return null;
                        break;
                }
                args.Title = originalController.Title + " copy";
                args.ContentId = originalController.LibraryElementModel.ContentDataModelId;
                args.AccessType = access;
                args.LibraryElementType = originalController.LibraryElementModel.Type;
                args.LibraryElementId = newLibraryId;
                args.Small_Thumbnail_Url = originalController.SmallIconUri.AbsoluteUri;
                args.Medium_Thumbnail_Url = originalController.MediumIconUri.AbsoluteUri;
                args.Large_Thumbnail_Url = originalController.LargeIconUri.AbsoluteUri;
                args.Origin = new LibraryElementOrigin() {Type = LibraryElementOrigin.OriginType.Copy,OriginId = originalController.LibraryElementModel.LibraryElementId};
                args.Metadata = new List<NusysIntermediate.MetadataEntry>(originalController.FullMetadata.Values);
                args.Metadata.Add(new MetadataEntry("Origin",new List<string>() {"This library element was copied from "+originalController.LibraryElementModel.Title},MetadataMutability.IMMUTABLE ));

                var newLibraryElementRequest = new CreateNewLibraryElementRequest(args);
                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(newLibraryElementRequest);
                newLibraryElementRequest.AddReturnedLibraryElementToLibrary();
                SessionController.Instance.ContentController.GetLibraryElementController(newLibraryId)?.SetAccessType(access);
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
                SessionController.Instance.ContentController.GetLibraryElementController(newLibraryId)?.SetAccessType(access);
                return newLibraryId;
                
            }
            
        }

        /// <summary>
        /// Adds the element to the proper collection in the workspace. That is if there are nested collections it adds the element to the
        /// inner most nested collection. If the element already exists then pass in a library elment controller. Otherwise if the element
        /// can be generated with empty content, such as a tool or a text node, you can just pass in the element type
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="elementType"></param>
        /// <param name="lec"></param>
        public static async Task AddElementToWorkSpace(Vector2 screenPoint, NusysConstants.ElementType elementType, LibraryElementController lec = null)
        {
            // get a list of the elements that lie under the passed in screen point
            var hits = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.GetRenderItemsAt(screenPoint);

            // get a list of the collections which lie under the thing we released
            var underlyingCollections = hits.OfType<CollectionRenderItem>().ToList();

            // get the last thing we hit, if we dragged over nested collections this would be the inner most collection
            var hit = underlyingCollections.Last();
            // add the element to that collection
            await AddElementToCollection(screenPoint, elementType, lec, hit);
        }

        /// <summary>
        /// Adds an element of elementType to the passed in collection, at the point on the collection directly under screenpoint. If the element can exist without any
        /// predefined content, such as an empty text node, or collection node, then lec can be a null argument and the empty element will be created. Otherwise if content
        /// is required, the associated library element controller must be passed in.
        /// This method safety checks to make sure acls are respected
        /// </summary>
        /// <param name="screenPoint">Point on the screen the new element will be created directly under this point on the passed in collection</param>
        /// <param name="elementType">The type of the elementy we are going to create. Must be able to exist without predefined content if library element controller is null</param>
        /// <param name="lec">The libraryy element controller of the element we are going to add</param>
        /// <param name="collection">The collection we are going to add the elemnt to</param>
        public static async Task AddElementToCollection(Vector2 screenPoint, NusysConstants.ElementType elementType, LibraryElementController lec, CollectionRenderItem collection)
        {
            // transform the passed in screenpoint to a point on the main collection
            var collectionPoint = SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(screenPoint, collection);
            var libraryElementId = lec?.LibraryElementModel.LibraryElementId; // variable to hold the library element id of the element we are adding
            var contentId = lec?.LibraryElementModel.ContentDataModelId; // variable to hold the content id of the element we are adding

            // get the library element model of the collection we are adding to
            var CollectionLibraryElementModel = SessionController.Instance.ContentController.GetLibraryElementModel(collection.ViewModel.LibraryElementId);

            // if we are creating empty content
            if (lec == null)
            {
                // make sure that the element type requested can be empty, and filter out requests for non-content such as recording nodes
                // both tools and recording nodes cannot be added to embedded collections!
                switch (elementType)
                {
                    case NusysConstants.ElementType.Collection:
                        break;
                    case NusysConstants.ElementType.Text:
                        break;
                    case NusysConstants.ElementType.Tools:

                        // add a tool to the workspace
                        var model = new BasicToolModel();
                        var controller = new BasicToolController(model);
                        UITask.Run(() =>
                        {
                            var viewModel = new BasicToolViewModel(controller)
                            {
                                Filter = ToolModel.ToolFilterTypeTitle.Title,
                            };
                            controller.SetSize(500, 500);
                            controller.SetPosition(collectionPoint.X, collectionPoint.Y);
                            SessionController.Instance.ActiveFreeFormViewer.AddTool(viewModel);
                        });

                        return; // return after this we are not creating content
                    case NusysConstants.ElementType.Recording:
                        AddRecordingNode(screenPoint);
                        return; // return after this we are not creating content
                    default:
                        Debug.Assert(false, $"In order to add an element of type, {elementType} to the collection you must provide a library element controller");
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

                // get the library element id and content id and library element controller for use outside of this if statement 
                libraryElementId = createNewContentRequestArgs.LibraryElementArgs.LibraryElementId;
                contentId = createNewContentRequestArgs.ContentId;
                lec = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementId);
            }

            // if the item is private and the collection we are adding it to is public then don't add it
            if ((lec.LibraryElementModel.AccessType == NusysConstants.AccessType.Private && CollectionLibraryElementModel.AccessType == NusysConstants.AccessType.Public) ||
                // if the itme is private and the collection we are adding it to is public then don't add it
                (lec.LibraryElementModel.AccessType == NusysConstants.AccessType.Private && CollectionLibraryElementModel.AccessType == NusysConstants.AccessType.ReadOnly) ||
                // if the collection we are adding it to is read only and we are not the owner of it then don't add it
                (CollectionLibraryElementModel.AccessType == NusysConstants.AccessType.ReadOnly && CollectionLibraryElementModel.Creator != WaitingRoomView.UserID))
            {
                SessionController.Instance.NuSessionView.ShowPrivateOnPublicPopup();
                return;
            }

            // if the item is the collection we are adding it to then don't add it
            if (lec.LibraryElementModel.LibraryElementId == CollectionLibraryElementModel.LibraryElementId)
            {
                SessionController.Instance.NuSessionView.ShowDraggingCollectionToCollectionPopup();
                return;
            }
            

            // create a new add element to collection request
            var newElementRequestArgs = new NewElementRequestArgs
            {
                LibraryElementId = libraryElementId,
                ParentCollectionId = CollectionLibraryElementModel.LibraryElementId,
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
        /// async static method to invite someone to your collection.
        /// Pass is the UserID of the person you wish to invite.
        /// This will take care of notifying the local user of the sent invite
        /// </summary>
        /// <param name="collaboratorId"></param>
        /// <returns></returns>
        public static async Task InviteCollaboratorToCollection(string collaboratorId)
        {
            var request = new SendCollaboratorCoordinatesRequest(new SendCollaboratorCoordinatesRequestArgs()
            {
                CollectionLibraryId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                RecipientUserId = collaboratorId,
                XCoordinatePosition = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalPosition.X,
                YCoordinatePosition = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalPosition.Y,
                YLocalScaleCenter = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScaleCenter.Y,
                XLocalScaleCenter = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScaleCenter.X,
                CameraScaleX = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScale.X,
                CameraScaleY = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScale.Y,
                AskBeforeJoining = true
            });
            SessionController.Instance.NuSessionView.Chatbox.AddChat(NetworkUser.ChatBot, "Invitation sent for " + SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[collaboratorId] + " to join your current workspace.");
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }

        /// <summary>
        /// async static method to request to join someone's collection.
        /// Pass is the UserID of the person you wish to join.
        /// This will automatically notify the local user of the join request sent
        /// </summary>
        /// <param name="collaboratorId"></param>
        /// <returns></returns>
        public static async Task JoinCollaborator(string collaboratorId)
        {
            var request = new GetCollaboratorCoordinatesRequest(new GetCollaboratorCoordinatesRequestArgs()
            {
                UserId = collaboratorId
            });
            SessionController.Instance.NuSessionView.Chatbox.AddChat(NetworkUser.ChatBot, "Request sent to join " + SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[collaboratorId] + "'s current workspace.");
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }

        /// <summary>
        /// Adds an element of elementType to the current collection, at the point on the collection directly under screenpoint. If the element can exist without any
        /// predefined content, such as an empty text node, or collection node, then lec can be a null argument and the empty element will be created. Otherwise if content
        /// is required, the associated library element controller must be passed in.
        /// This method safety checks to make sure acls are respected
        /// </summary>
        /// <param name="screenPoint">Point on the screen the new element will be created directly under this point on the main collection</param>
        /// <param name="elementType">The type of the elementy we are going to create. Must be able to exist without predefined content if library element controller is null</param>
        /// <param name="lec">The libraryy element controller of the element we are going to add</param>
        /// <param name="transformPoint">This bool represents whether you want to have the screen point be transformed into a collection point.  True if it needs to be transformed, false if it is already in collection coordinates</param>
        public static async Task AddElementToCurrentCollection(Vector2 screenPoint, NusysConstants.ElementType elementType, LibraryElementController lec = null, bool transformPoint = true)
            {
            // transform the passed in screenpoint to a point on the main collection
            var collectionPoint =  transformPoint ? SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(screenPoint, SessionController.Instance.SessionView.FreeFormViewer.InitialCollection) : screenPoint;
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

                        // add a tool to the workspace
                        var model = new BasicToolModel();
                        var controller = new BasicToolController(model);

                            var viewModel = new BasicToolViewModel(controller)
                            {
                                Filter = ToolModel.ToolFilterTypeTitle.Title,
                            };
                            controller.SetSize(500,500);
                            controller.SetPosition(collectionPoint.X, collectionPoint.Y);
                            SessionController.Instance.ActiveFreeFormViewer.AddTool(viewModel);

                        return; // return after this we are not creating content
                    case NusysConstants.ElementType.Recording:
                        AddRecordingNode(screenPoint);
                        return; // return after this we are not creating content
                    default:
                        Debug.Assert(false, $"In order to add an element of type, {elementType} to the collection you must provide a library element controller");
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

                // get the library element id and content id and library element controller for use outside of this if statement 
                libraryElementId = createNewContentRequestArgs.LibraryElementArgs.LibraryElementId;
                contentId = createNewContentRequestArgs.ContentId;
                lec = SessionController.Instance.ContentController.GetLibraryElementController(libraryElementId);
            }

            // if the item is private and the workspace is public or the item is the current workspace then don't add it
            if ((lec.LibraryElementModel.AccessType == NusysConstants.AccessType.Private &&
                SessionController.Instance.CurrentCollectionLibraryElementModel.AccessType == NusysConstants.AccessType.Public) || 
                lec.LibraryElementModel.LibraryElementId == SessionController.Instance.CurrentCollectionLibraryElementModel.LibraryElementId ||
                (lec.LibraryElementModel.AccessType == NusysConstants.AccessType.Private && SessionController.Instance.CurrentCollectionLibraryElementModel.AccessType == NusysConstants.AccessType.ReadOnly))
            {
                SessionController.Instance.NuSessionView.ShowPrivateOnPublicPopup();
                return;
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

        /// <summary>
        /// creates a collection from a provided screen point, list of element models, and a title (to be the title of the collection)
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="elementModels"></param>
        /// <param name="title"></param>
        public static async Task CreateCollectionOnMainCollection(Vector2 screenPoint, List<LibraryElementController> elements, string title)
        {
            // the library element id of the collection we are creating, used as the parent collection id when adding elements to it later in the method
            var collectionLibElemId = SessionController.Instance.GenerateId();

            // We determine the access type of the tool generated collection based on the collection we're in and pass that in to the request
            var newCollectionAccessType = SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.AccessType;

            // create a new library element args class to assist in creating the collection
            var createNewLibraryElementRequestArgs = new CreateNewLibraryElementRequestArgs
            {
                ContentId = SessionController.Instance.GenerateId(),
                LibraryElementType = NusysConstants.ElementType.Collection,
                Title = title,
                LibraryElementId = collectionLibElemId,
                AccessType = newCollectionAccessType
            };

            // create a new content request args to assist in creating the collection
            var createNewContentRequestArgs = new CreateNewContentRequestArgs
            {
                LibraryElementArgs = createNewLibraryElementRequestArgs
            };

            var args = new CreateNewCollectionServerRequestArgs();
            args.CreateNewContentRequestDictionary = createNewContentRequestArgs.PackToRequestKeys().ToDictionary(k => k.Key, v => v.Value);
            args.NewElementRequestDictionaries = new List<Dictionary<string, object>>();

            // Add all the elements to the newly created collection
            foreach (var controller in elements)
            {
                var lem = controller.LibraryElementModel;

                // if the library element model doesn't exist, or is a link don't add it to the collection
                if (lem == null || lem.Type == NusysConstants.ElementType.Link ||
                (lem.AccessType == NusysConstants.AccessType.Private && newCollectionAccessType == NusysConstants.AccessType.ReadOnly) ||
                (lem.AccessType == NusysConstants.AccessType.Private && newCollectionAccessType == NusysConstants.AccessType.Public))
                {
                    continue;
                }

                // create a new element request args, and pass in the required fields
                var newElementRequestArgs = new NewElementRequestArgs
                {
                    // set the position
                    X = 50000,
                    Y = 50000,

                    // size
                    Width = Constants.DefaultNodeSize,
                    Height = Constants.DefaultNodeSize,

                    // ids
                    ParentCollectionId = collectionLibElemId,
                    LibraryElementId = lem.LibraryElementId
                };

                args.NewElementRequestDictionaries.Add(newElementRequestArgs.PackToRequestKeys().ToDictionary(k => k.Key, v => v.Value));
            }

            var request = new CreateNewCollectionRequest(args);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            request.AddReturnedLibraryElementToLibrary();

            // add the collection to the current session
            var collectionLEM = SessionController.Instance.ContentController.GetLibraryElementController(collectionLibElemId);
            await collectionLEM.AddElementAtPosition(screenPoint.X, screenPoint.Y).ConfigureAwait(false);
        }

        /// <summary>
        /// creates a stack of elements on the main collection from a list of elementmodels
        /// </summary>
        /// <param name="screenPoint"></param>
        /// <param name="elementModels"></param>
        public static async Task CreateStackOnMainCollection(Vector2 screenPoint, List<LibraryElementController> elements)
        {
            // use the i counter to offset each new element in the stack
            int i = 0;
            int offset = 40;
            var point =
                SessionController.Instance.SessionView.FreeFormViewer.RenderEngine.ScreenPointerToCollectionPoint(
                    screenPoint, SessionController.Instance.SessionView.FreeFormViewer.InitialCollection);

            foreach (var controller in elements)
            {
                // if the library element model doesn't exist, is a link, or is greater than 20, don't add it to the session
                if (controller == null || controller.LibraryElementModel.Type == NusysConstants.ElementType.Link)
                {
                    continue;
                }

                // add the element to the collection
                await AddElementToCurrentCollection(point + new Vector2(i * offset), controller.LibraryElementModel.Type, controller,false).ConfigureAwait(false);

                // increment to finish loop and perform offset
                i++;
            }
        }
    }
}
