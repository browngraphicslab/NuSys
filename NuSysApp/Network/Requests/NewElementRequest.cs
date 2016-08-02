using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class NewElementRequest : Request
    {
        public NewElementRequest(Message message) : base(NusysConstants.RequestType.NewElementRequest, message)
        {
        }

        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                _message["id"] = SessionController.Instance.GenerateId();
            }
            if (!_message.ContainsKey("contentId"))
            {
                throw new NewNodeRequestException("New Node requests require messages with at least 'contentId'");
            }
        }

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
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new TextNodeController((TextElementModel)elementModel);
                        break;
                    case NusysConstants.ElementType.ImageRegion:
                    case NusysConstants.ElementType.Image:
                        elementModel = new ImageElementModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ImageElementIntanceController(elementModel);
                        break;
                    case NusysConstants.ElementType.Word:
                        elementModel = new WordNodeModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Powerpoint:
                        elementModel = new PowerpointNodeModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.PdfRegion:
                    case NusysConstants.ElementType.PDF:
                        elementModel = new PdfNodeModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.AudioRegion:
                    case NusysConstants.ElementType.Audio:
                        elementModel = new AudioNodeModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.VideoRegion:
                    case NusysConstants.ElementType.Video:

                        elementModel = new VideoNodeModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Tag:
                        elementModel = new TagNodeModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Web:
                        elementModel = new WebNodeModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case NusysConstants.ElementType.Collection:
                        elementModel = new CollectionElementModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
                        controller = new ElementCollectionController(elementModel);
                        break;
                    case NusysConstants.ElementType.Area:
                        elementModel = new AreaModel(id);
                        await elementModel.UnPackFromDatabaseMessage(_message);
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
