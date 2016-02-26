using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewElementInstanceRequest : Request
    {
        public NewElementInstanceRequest(Message message) : base(Request.RequestType.NewNodeRequest, message)
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

            ElementInstanceModel elementInstanceModel;
            ElementInstanceController controller;
            ElementInstanceViewModel nodeViewModel;

            switch (nodeType)
            {
                case ElementType.Text:
                    elementInstanceModel = new TextElementInstanceModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Image:
                    elementInstanceModel = new ImageElementInstanceModel(id);
                    controller = new ImageElementIntanceController(elementInstanceModel);
                    break;
                case ElementType.Word:
                    elementInstanceModel = new WordNodeModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Powerpoint:
                    elementInstanceModel = new PowerpointNodeModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.PDF:
                    elementInstanceModel = new PdfNodeModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Audio:
                    elementInstanceModel = new AudioNodeModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Video:
                    elementInstanceModel = new VideoNodeModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Tag:
                    elementInstanceModel = new TagNodeModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Web:
                    elementInstanceModel = new WebNodeModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Workspace:
                    elementInstanceModel = new WorkspaceModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Group:
                    elementInstanceModel = new NodeContainerModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                case ElementType.Area:
                    elementInstanceModel = new AreaModel(id);
                    controller = new ElementInstanceController(elementInstanceModel);
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
            }

            await elementInstanceModel.UnPack(_message);

            var parentController = (ElementCollectionInstanceController) SessionController.Instance.IdToControllers[controller.Model.Creator];
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
