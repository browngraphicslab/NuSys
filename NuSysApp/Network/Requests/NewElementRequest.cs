using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewElementRequest : Request
    {
        public NewElementRequest(Message message) : base(Request.RequestType.NewNodeRequest, message)
        {
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            SetServerRequestType(ServerRequestType.Add);
        }

        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                _message["id"] = SessionController.Instance.GenerateId();
            }
            if (!_message.ContainsKey("contentId") || ! _message.ContainsKey("creatorContentID"))
            {
                throw new NewNodeRequestException("New Node requests require messages with at least 'contentId' and 'creatorContentID'");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            var libraryId = _message.GetString("contentId");
            var creatorContentID = _message.GetString("creatorContentID");

            ElementModel elementModel;
            ElementController controller;

            var libraryElement = SessionController.Instance.ContentController.Get(libraryId);
            if (libraryElement == null)
            {
                var type = (ElementType) Enum.Parse(typeof (ElementType), (string) _message["nodeType"], true);
                if (type == ElementType.Collection)
                {
                    libraryElement = new CollectionLibraryElementModel(libraryId);
                }
                else
                {
                    libraryElement = new LibraryElementModel(libraryId,type);
                }
                SessionController.Instance.ContentController.Add(libraryElement);
            }
            if (!libraryElement.LoadingOrLoaded())
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
                    break;
                case ElementType.Link:
                    elementModel = new LinkModel(id);
                    await elementModel.UnPack(_message);
                    controller = new LinkElementController((LinkModel)elementModel);
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
            }

            SessionController.Instance.IdToControllers[id] = controller;

            var parentCollectionLibraryElement = (CollectionLibraryElementModel)SessionController.Instance.ContentController.Get(creatorContentID);
            parentCollectionLibraryElement.AddChild(id);

            if (parentCollectionLibraryElement.Id == SessionController.Instance.ActiveFreeFormViewer.Controller.LibraryElementModel.Id)
            {
                Task.Run(async delegate
                {
                    await
                        SessionController.Instance.NuSysNetworkSession.ExecuteRequest(
                            new SubscribeToCollectionRequest(libraryId));
                });
            }

            if (SessionController.Instance.ContentController.Get(libraryId).Loaded)
            {
                controller.FireContentLoaded();
            }
            else
            {
                SessionController.Instance.ContentController.Get(libraryId).OnLoaded += delegate
                {
                    controller.FireContentLoaded();
                };
            }

            if (elementType == ElementType.Collection)
            {
                //TODO have this code somewhere but not stack overflow.  aka: add in a level checker so we don't recursively load 
                var existingChildren = ((CollectionLibraryElementModel) (controller.LibraryElementModel))?.Children;
                foreach (var childId in existingChildren)
                {
                    if (SessionController.Instance.IdToControllers.ContainsKey(childId))
                    {
                        ((ElementCollectionController) controller).AddChild(SessionController.Instance.IdToControllers[childId]);
                    }
                }
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
