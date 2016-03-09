using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuSysApp.Controller;

namespace NuSysApp
{
    public class NewLinkRequest : Request
    {
        public NewLinkRequest(Message m) : base(RequestType.NewLinkRequest,m){}
        public NewLinkRequest(string id1, string id2, string creator, string contentId, string id = null) : base(RequestType.NewLinkRequest)
        {
            _message["id1"] = id1;
            _message["id2"] = id2;
            _message["id"] = id ?? SessionController.Instance.GenerateId();
            _message["creator"] = creator;
            _message["contentId"] = contentId;
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                _message["id"] = SessionController.Instance.GenerateId();
            }
            _message["type"] = ElementType.Link.ToString();
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            SetServerRequestType(ServerRequestType.Add);
        }

        public override async Task ExecuteRequestFunction()
        {
            var id1 = _message.GetString("id1");
            var id2 = _message.GetString("id2");
            var id = _message.GetString("id");
            var creator = _message.GetString("creator");
            if (SessionController.Instance.IdToControllers.ContainsKey(id1) && (SessionController.Instance.IdToControllers.ContainsKey(id2)))
            {
                var link = new LinkModel(id);
                await link.UnPack(_message);
                var linkController = new LinkElementController(link);
                SessionController.Instance.IdToControllers.Add(id, linkController);
                

                var parentController = (ElementCollectionController)SessionController.Instance.IdToControllers[creator];
                parentController.AddChild(linkController);
            }
        }
    }
}
