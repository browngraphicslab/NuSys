using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewNodeRequest : Request
    {
        public NewNodeRequest(Message message) : base(Request.RequestType.NewNodeRequest, message)
        {
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
            var nodeType = (NodeType) Enum.Parse(typeof (NodeType), _message.GetString("nodeType"));
            var id = _message.GetString("id");

            NodeModel node;
            NodeViewModel nodeViewModel;
            switch (nodeType)
            {
                case NodeType.Text:
                    node = new TextNodeModel(id);
                    break;
                case NodeType.Image:
                    node = new ImageNodeModel(id);
                    break;
                case NodeType.Word:
                    node = new WordNodeModel(id);
                    break;
                case NodeType.Powerpoint:
                    node = new PowerpointNodeModel(id);
                    break;
                case NodeType.PDF:
                    node = new PdfNodeModel(id);
                    break;
                case NodeType.Audio:
                    node = new AudioNodeModel(id);
                    break;
                case NodeType.Video:
                    node = new VideoNodeModel(id);
                    break;
                case NodeType.Tag:
                    node = new NodeContainerModel(id);
                    break;
                case NodeType.Web:
                    node = new WebNodeModel(id);
                    break;
                case NodeType.Workspace:
                    node = new WorkspaceModel(id);
                    break;
                case NodeType.Group:
                    node = new NodeContainerModel(id);
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