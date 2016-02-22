using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ChangeContentRequest : Request
    {
        public ChangeContentRequest(Message m) : base(RequestType.ChangeContentRequest, m)
        {
        }

        public ChangeContentRequest(string nodeID, string contentID, string contentData) : base(RequestType.ChangeContentRequest)
        {
            _message["contentId"] = contentID;
            _message["data"] = contentData;
            _message["id"] = nodeID;
        }
        public override async Task CheckOutgoingRequest()
        {
            //TODO fill in
        }
        public override async Task ExecuteRequestFunction()
        {
            NodeContentModel content = SessionController.Instance.ContentController.Get(_message.GetString("contentId"));
            content.Data = _message.GetString("data");

            var s = SessionController.Instance.IdToSendables[_message.GetString("id")];
            await s.UnPack(_message);
        }
    }
}
