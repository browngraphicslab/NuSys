using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewLinkRequest : Request
    {
        public NewLinkRequest(Message m) : base(RequestType.NewLinkRequest,m){}
        public NewLinkRequest(string id1, string id2) : base(RequestType.NewLinkRequest)
        {
            _message["id1"] = id1;
            _message["id2"] = id2;
            _message["id"] = SessionController.Instance.GenerateId();
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                _message["id"] = SessionController.Instance.GenerateId();
            }
        }

        public override async Task ExecuteRequestFunction()
        {
            var id1 = _message.GetString("id1");
            var id2 = _message.GetString("id2");
            var id = _message.GetString("id");
            if (SessionController.Instance.IdToSendables.ContainsKey(id1) && (SessionController.Instance.IdToSendables.ContainsKey(id2)))
            {
                SessionController.Instance.CreateLink((AtomModel)SessionController.Instance.IdToSendables[id1], (AtomModel)SessionController.Instance.IdToSendables[id2], id);
                var link = (LinkModel)SessionController.Instance.IdToSendables[id];
                await link.UnPack(_message);

                var creators = link.Creators;
                if (creators.Count > 0)
                {
                    foreach (var creator in creators)
                    {
                        await (SessionController.Instance.IdToSendables[creator] as NodeContainerModel).AddChild(link);
                    }
                }
                else
                    await (SessionController.Instance.ActiveWorkspace.Model as WorkspaceModel).AddChild(link);

            }
        }
    }
}
