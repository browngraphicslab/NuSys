using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NusysConstants;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class NewElementRequest : Request
    {
        public NewElementRequest(Message message) : base(ServerConstants.RequestType.NewNodeRequest, message)
        {
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            SetServerRequestType(ServerRequestType.Add);
        }

        public override async Task<bool> CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                _message["id"] = SessionController.Instance.GenerateId();
            }
            if (!_message.ContainsKey("contentId"))
            {
                throw new NewNodeRequestException("New Node requests require messages with at least 'contentId'");
            }
            return true;
        }

        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            var libraryId = _message.GetString("contentId");
            var creator = _message.GetString("creator");

            ElementModel elementModel = null;
            ElementController controller = null;

            var libraryElement = SessionController.Instance.ContentController.GetContent(libraryId);
            if (libraryElement == null)
            {
                libraryElement = LibraryElementModelFactory.CreateFromMessage(_message);
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
                    case ElementType.Text:
                        elementModel = new TextElementModel(id);
                        await elementModel.UnPack(_message);
                        controller = new TextNodeController((TextElementModel)elementModel);
                        break;
                    case ElementType.ImageRegion:
                    case ElementType.Image:
                        elementModel = new ImageElementModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ImageElementIntanceController(elementModel);
                        break;
                    case ElementType.Word:
                        elementModel = new WordNodeModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case ElementType.Powerpoint:
                        elementModel = new PowerpointNodeModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case ElementType.PDF:
                        elementModel = new PdfNodeModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case ElementType.Audio:
                        elementModel = new AudioNodeModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case ElementType.Video:
                        elementModel = new VideoNodeModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case ElementType.Tag:
                        elementModel = new TagNodeModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case ElementType.Web:
                        elementModel = new WebNodeModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;
                    case ElementType.Collection:
                        elementModel = new CollectionElementModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementCollectionController(elementModel);
                        break;
                    case ElementType.Area:
                        elementModel = new AreaModel(id);
                        await elementModel.UnPack(_message);
                        controller = new ElementController(elementModel);
                        break;/*
                    case ElementType.Link:
                        elementModel = new LinkModel(id);
                        await elementModel.UnPack(_message);
                        controller = new LinkController((LinkModel)elementModel);
                        break;*/
                    case ElementType.Recording:
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

                var parentCollectionLibraryElement =
                    (CollectionLibraryElementModel)SessionController.Instance.ContentController.GetContent(creator);
                parentCollectionLibraryElement.AddChild(id);

                if (parentCollectionLibraryElement.LibraryElementId ==
                    SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.LibraryElementId)
                {
                    Task.Run(async delegate
                    {
                        await
                            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                                new SubscribeToCollectionRequest(libraryId));
                    });
                }

                if (elementType == ElementType.Collection)
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
