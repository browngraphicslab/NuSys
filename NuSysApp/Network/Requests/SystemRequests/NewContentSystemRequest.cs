using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NuSysApp.Network.Requests.SystemRequests;

namespace NuSysApp
{
    public class NewContentSystemRequest : SystemRequest
    {
        public NewContentSystemRequest(Message m) : base(SystemRequestType.NewContent, m) { }

        public NewContentSystemRequest(string id, string data) : base(SystemRequestType.NewContent)
        {
            _message["id"] = id;
            _message["data"] = data;
        }
        public async override Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id") || !_message.ContainsKey("data"))
            {
                throw new Exception("New Content requests require at least 'id' and 'data'");
            }
        }
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, string senderIP)
        {
            var data = _message.GetString("data");
            var id = _message.GetString("id");
            SessionController.Instance.ContentController.Add(data, id);
            if (nusysSession.IsHostMachine)
            {
                await nusysSession.ExecuteSystemRequest(new ContentAvailableNotificationSystemRequest(id));
            }
        }
    }
}
