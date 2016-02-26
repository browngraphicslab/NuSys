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
            if (!_message.ContainsKey("nodeType"))
            {
                throw new NewNodeRequestException("New Node requests require messages with at least 'nodeType'");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            var nodeType = (ElementType) Enum.Parse(typeof (ElementType), _message.GetString("nodeType"));
            var id = _message.GetString("id");

            ElementModel elementModel;
            ElementController controller;
            ElementViewModel nodeViewModel;

            switch (nodeType)
            {
                case ElementType.Text:
                    elementModel = new TextElementModel(id);
                    controller = new ElementController(elementModel);
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
                case ElementType.Workspace:
                    elementModel = new WorkspaceModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Group:
                    elementModel = new NodeContainerModel(id);
                    controller = new ElementController(elementModel);
                    break;
                case ElementType.Area:
                    elementModel = new AreaModel(id);
                    controller = new ElementController(elementModel);
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
            }

            await elementModel.UnPack(_message);

            var parentController = (ElementCollectionController) SessionController.Instance.IdToControllers[controller.Model.Creator];
            parentController.AddChild(controller);
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
