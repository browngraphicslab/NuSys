using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewNodeRequest : Request
    {
        public NewNodeRequest(Message message) : base(Request.RequestType.NewNodeRequest, message)
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

            ElementInstanceModel node;
            NodeViewModel nodeViewModel;
            switch (nodeType)
            {
                case ElementType.Text:
                    node = new TextNodeModel(id);
                    break;
                case ElementType.Image:
                    node = new ImageNodeModel(id);
                    break;
                case ElementType.Word:
                    node = new WordNodeModel(id);
                    break;
                case ElementType.Powerpoint:
                    node = new PowerpointNodeModel(id);
                    break;
                case ElementType.PDF:
                    node = new PdfNodeModel(id);
                    break;
                case ElementType.Audio:
                    node = new AudioNodeModel(id);
                    break;
                case ElementType.Video:
                    node = new VideoNodeModel(id);
                    break;
                case ElementType.Tag:
                    node = new TagNodeModel(id);
                    break;
                case ElementType.Web:
                    node = new WebNodeModel(id);
                    break;
                case ElementType.Workspace:
                    node = new WorkspaceModel(id);
                    break;
                case ElementType.Group:
                    node = new NodeContainerModel(id);
                    break;
                case ElementType.Area:
                    node = new AreaModel(id);
                    break;
                default:
                    throw new InvalidOperationException("This node type is not yet supported");
            }


            if (!SessionController.Instance.IdToSendables.ContainsKey(id))
            {
                SessionController.Instance.IdToSendables.Add(id, node);
            }
            else
            {
                SessionController.Instance.IdToSendables[id] = node;
            }

            await node.UnPack(_message);

            if (!_message.GetBool("autoCreate"))
                return;

            SessionController.Instance.RecursiveCreate(node);
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
