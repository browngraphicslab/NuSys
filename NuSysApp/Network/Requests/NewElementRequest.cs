using System;
using System.Collections.Generic;
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
            if (!_message.ContainsKey("nodeType"))
            {
                throw new NewNodeRequestException("New Node requests require messages with at least 'nodeType'");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            var nodeType = (ElementType) Enum.Parse(typeof (ElementType), _message.GetString("nodeType"));
            var id = _message.GetString("id");
            var contentId = _message.GetString("contentId");
            var creatorContentID = _message.GetString("creatorContentID");

            ElementModel elementModel;
            ElementController controller;
            ElementViewModel nodeViewModel;

            switch (nodeType)
            {
                case ElementType.Text:
                    elementModel = new TextElementModel(id);
                    elementModel.ContentId = contentId;
                    if (SessionController.Instance.ContentController.Get(contentId) == null)
                    {
                        SessionController.Instance.ContentController.Add(new LibraryElementModel(null, contentId,
                            ElementType.Text, elementModel.Title));
                    }
                    controller = new TextNodeController((TextElementModel)elementModel);
                    break;
                case ElementType.Image:
                    elementModel = new ImageElementModel(id);
                    controller = new ImageElementIntanceController(elementModel);
                    break;
                case ElementType.Word:
                    elementModel = new WordNodeModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Powerpoint:
                    elementModel = new PowerpointNodeModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.PDF:
                    elementModel = new PdfNodeModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Audio:
                    elementModel = new AudioNodeModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Video:
                    elementModel = new VideoNodeModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Tag:
                    elementModel = new TagNodeModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Web:
                    elementModel = new WebNodeModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Collection:
                    elementModel = new CollectionElementModel(id);
                    elementModel.ContentId = contentId;
                    if (SessionController.Instance.ContentController.Get(contentId) == null)
                    {
                        SessionController.Instance.ContentController.Add(new CollectionLibraryElementModel(contentId,
                            null, elementModel.Title));
                    }
                    controller = new ElementCollectionController(elementModel);
                    break;
                case ElementType.Area:
                    elementModel = new AreaModel(id);
                    controller = new ElementController(elementModel);
                    break;/*
                case ElementType.Library:
                    elementModel = new ElementModel(id);
                    controller = new ElementController(elementModel);
                    break;*/
                case ElementType.Link:
                    elementModel = new LinkModel(id);
                    controller = new ElementController(elementModel);
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
            }

            await elementModel.UnPack(_message);
            elementModel.ContentId = contentId;
            SessionController.Instance.IdToControllers[controller.Model.Id] = controller;

            var content = (CollectionLibraryElementModel)SessionController.Instance.ContentController.Get(creatorContentID);
            content.AddChild(controller.Model.Id);

            if (SessionController.Instance.ContentController.ContainsAndLoaded(content.Id))
            {
                controller.FireContentLoaded(content);
            }
            //var parentController = (ElementCollectionController) SessionController.Instance.IdToControllers[creatorContentID];
            //parentController.AddChild(controller);

            if (nodeType == ElementType.Collection)
            {
                //TODO have this code somewhere but not stack overflow.  aka: add in a level checker so we don't recursively load 
                var startingChildren = ((CollectionLibraryElementModel) (controller.ContentModel))?.Children;
                foreach (var childId in startingChildren)
                {
                    if (SessionController.Instance.IdToControllers.ContainsKey(childId))
                    {
                        ((ElementCollectionController) controller).AddChild(
                            SessionController.Instance.IdToControllers[childId]);
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
