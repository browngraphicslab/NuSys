using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewNodeRequest : Request
    {
        public NewNodeRequest(Message message) : base(Request.RequestType.NewNodeRequest, message)
        {
            _message["id"] = SessionController.Instance.GenerateId();
        }

        public override async Task CheckRequest()
        {
            if (!_message.ContainsKey("nodetype"))
            {
                throw new NewNodeRequestException("New Node requests require messages with at least 'nodetype'");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            NodeModel node = await SessionController.Instance.CreateNewNode(_message.GetString("id"), (NodeType)Enum.Parse(typeof(NodeType),_message.GetString("nodetype")));
            SessionController.Instance.IdToSendables[_message.GetString("id")] = node;
            await node.UnPack(_message);
            var creator = (node as AtomModel).Creator;
            if (creator != null)
                await (SessionController.Instance.IdToSendables[creator] as NodeContainerModel).AddChild(node);
            else
                await ((NodeContainerModel)SessionController.Instance.ActiveWorkspace.Model).AddChild(node);

        }
        private byte[] ParseToByteArray(string s)
        {
            return Convert.FromBase64String(s);
        }
    }
    public class NewNodeRequestException : Exception
    {
        public NewNodeRequestException(string message) : base(message) { }
        public NewNodeRequestException() : base("There was an error in the NewNodeRequest") { }
    }
}
