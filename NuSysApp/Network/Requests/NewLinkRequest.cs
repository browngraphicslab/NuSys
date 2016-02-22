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
        public NewLinkRequest(string id1, string id2, string creator, bool autoCreate = false) : base(RequestType.NewLinkRequest)
        {
            _message["id1"] = id1;
            _message["id2"] = id2;
            _message["id"] = SessionController.Instance.GenerateId();
            _message["creator"] = creator;
            _message["autoCreate"] = autoCreate;
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
                var link = new LinkModel((ElementInstanceModel)SessionController.Instance.IdToSendables[id1], (ElementInstanceModel)SessionController.Instance.IdToSendables[id2], id);
                SessionController.Instance.IdToSendables.Add(id, link);
                await link.UnPack(_message);

                if (!_message.GetBool("autoCreate"))
                    return;

                SessionController.Instance.RecursiveCreate(link);
            }
        }
    }
}
