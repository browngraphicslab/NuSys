using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class NewElementRequest : Request
    {
        public NewElementRequest(Message message) : base(NusysConstants.RequestType.NewElementRequest, message)
        {
        }

        /// <summary>
        /// Preferred constructor.  
        /// Create and populate an args class.  
        /// Check the args properties to see what is and isn't required
        /// </summary>
        /// <param name="args"></param>
        public NewElementRequest(NewElementRequestArgs args) : base(NusysConstants.RequestType.NewElementRequest)
        {
            //asserts for required properties
            //TODO not make width and height required, just have defaults in nusysApp constants in they're not set;
            Debug.Assert(args.ParentCollectionId != null);
            Debug.Assert(args.LibraryElementId != null);
            Debug.Assert(args.Height != null);
            Debug.Assert(args.Width != null);
            Debug.Assert(args.Y != null);
            Debug.Assert(args.X != null);
            
            //set properties after assertions
            _message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY] = args.Id ?? SessionController.Instance.GenerateId();
            _message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY] = args.Y;
            _message[NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY] = args.X;
            _message[NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY] = args.ParentCollectionId;
            _message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY] = args.Height;
            _message[NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY] = args.Width;
            _message[NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY] = args.LibraryElementId;
        }

        /// <summary>
        /// this checker just debug.asserts() the required keys.
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY));
        }


        /// <summary>
        /// If the request was successful this will get the returned model and add it to the Session.
        /// This will throw an exception if the request hasn't returned or wasn't successful.
        /// </summary>
        public void AddReturnedElementToSession()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }
            var model = GetReturnedElementModel();
            Debug.Assert(SessionController.Instance.AddElement(model));//make sure the adding was succesful
        }

        /// <summary>
        /// This will return the Request-returned elementModel if the request was sucessful.  
        /// </summary>
        /// <returns></returns>
        public ElementModel GetReturnedElementModel()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY));
            try
            {
                var model = ElementModelFactory.DeserializeFromString(_returnMessage.GetString(NusysConstants.NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY));
                return model;
            }
            catch (JsonException e)
            {
                throw new Exception("The deserialization of an element model failed;");
            }
            
        }
       
        /// <summary>
        /// TODO: this fucking method all over again.  
        /// Holy shit does this need refactoring badly.  8/7/16
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            var libraryId = _message.GetString("contentId");
            var creator = _message.GetString("creator");

            ElementModel elementModel = null;
            ElementController controller = null;

            var libraryElement = SessionController.Instance.ContentController.GetLibraryElementModel(libraryId);
            if (libraryElement == null)
            {
                libraryElement = SessionController.Instance.ContentController.CreateAndAddModelFromMessage(_message);
            }
            if (libraryElement != null)
            {
                if (
                    !SessionController.Instance.ContentController.GetLibraryElementController(
                        libraryElement.LibraryElementId).LoadingOrLoaded)
                {
                    SessionController.Instance.NuSysNetworkSession.FetchLibraryElementData(libraryId);
                }

                var elementType = libraryElement.Type;

                switch (elementType)
                {
                    case NusysConstants.ElementType.Text:
                        elementModel = new TextElementModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new TextNodeController((TextElementModel)elementModel);
                        break;
                    case NusysConstants.ElementType.ImageRegion:
                    case NusysConstants.ElementType.Image:
                        elementModel = new ImageElementModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ImageElementIntanceController(elementModel);
                        break;
                    case NusysConstants.ElementType.Word:
                        elementModel = new WordNodeModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Powerpoint:
                        elementModel = new PowerpointNodeModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.PdfRegion:
                    case NusysConstants.ElementType.PDF:
                        elementModel = new PdfNodeModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.AudioRegion:
                    case NusysConstants.ElementType.Audio:
                        elementModel = new AudioNodeModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.VideoRegion:
                    case NusysConstants.ElementType.Video:

                        elementModel = new VideoNodeModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Tag:
                        elementModel = new TagNodeModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Web:
                        elementModel = new WebNodeModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Collection:
                        elementModel = new CollectionElementModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementCollectionController(elementModel);
                        break;
                    case NusysConstants.ElementType.Area:
                        elementModel = new AreaModel(id);
                        elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;/*
                    case ElementType.Link:
                        elementModel = new LinkModel(id);
                        await elementModel.UnPack(_message);
                        controller = new LinkController((LinkModel)elementModel);
                        break;*/
                    case NusysConstants.ElementType.Recording:
                        controller = new ElementController(null);
                        break;
                    default:
                        throw new InvalidOperationException("This node type is not yet supported");
                }

                foreach (var tag in controller?.LibraryElementModel?.Keywords ?? new HashSet<Keyword>())
                {
                    controller.LibraryElementModel.Keywords.Add(tag);
                }
                //controller.LibraryElementModel.Keywords.

                SessionController.Instance.IdToControllers[id] = controller;

                var parentCollectionLibraryElementController =
                    (CollectionLibraryElementController)SessionController.Instance.ContentController.GetLibraryElementController(creator);
                parentCollectionLibraryElementController.AddChild(id);

                if (parentCollectionLibraryElementController.CollectionModel.LibraryElementId ==
                    SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId)
                {
                    Task.Run(async delegate
                    {
                        await
                            SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(
                                new SubscribeToCollectionRequest(libraryId));
                    });
                }

                if (elementType == NusysConstants.ElementType.Collection)
                {
                    //TODO have this code somewhere but not stack overflow.  aka: add in a level checker so we don't recursively load 
                    var existingChildren = ((CollectionLibraryElementModel)(controller.LibraryElementModel))?.Children;
                    foreach (var childId in existingChildren ?? new HashSet<string>())
                    {
                        if (SessionController.Instance.IdToControllers.ContainsKey(childId))
                        {
                            ((ElementCollectionController)controller).AddChild(
                                SessionController.Instance.IdToControllers[childId]);
                        }
                    }
                }
                //SessionController.Instance.LinksController.AddAlias(controller);
            }
        }
    }

    public class NewNodeRequestException : Exception
    {
        public NewNodeRequestException(string message) : base(message)
        {
        }

        public NewNodeRequestException() : base("There was an error in the NewNodeRequest")
        {
        }
    }
}
