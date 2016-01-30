using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewNodeRequest : Request
    {
        public NewNodeRequest(Message message) : base(Request.RequestType.NewNodeRequest, message){}

        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                _message["id"] = SessionController.Instance.GenerateId();
            }
            if (!_message.ContainsKey("nodeType"))
            {
                Debug.WriteLine("UUUUUUUUUUUAAAAAAAAAAAAAAAAAAAAAAA");
                throw new NewNodeRequestException("New Node requests require messages with at least 'nodeType'");
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            /*
            if (_message.ContainsKey("contentId") && SessionController.Instance.ContentController.Get(_message.GetString("contentId")) == null)
            {
                await Task.Run(async delegate
                {
                    var mre = new ManualResetEvent(false);
                    SessionController.Instance.ContentController.AddWaitingNodeCreation(
                        _message.GetString("contentId"), mre);
                    mre.WaitOne();
                });
            }*/
            NodeModel node = await SessionController.Instance.CreateNewNode(_message.GetString("id"), (NodeType)Enum.Parse(typeof(NodeType),_message.GetString("nodeType")));
            SessionController.Instance.IdToSendables[_message.GetString("id")] = node;
            await node.UnPack(_message);

            if (!_message.GetBool("autoCreate"))
                return;

            var creators = node.Creators;
            var addedModels = new List<AtomModel>();
            SessionController.Instance.RecursiveCreate(node, addedModels);
            
        }
        public override async Task UndoTaskFunction()
        {
            
        }
    }

    public class NewNodeRequestException : Exception
    {
        public NewNodeRequestException(string message) : base(message) { }
        public NewNodeRequestException() : base("There was an error in the NewNodeRequest") { }
    }
}
