using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Network.Requests.SystemRequests
{
    public class FetchContentSystemRequest : SystemRequest
    {
        public FetchContentSystemRequest(string contentId) : base(SystemRequestType.FetchContent)
        {
            _message["contentId"] = contentId;
        }

        public FetchContentSystemRequest(Message m) : base(m){}


        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, string senderIP)
        {
            var contentId = _message.GetString("contentId");
            var request = new NewContentSystemRequest(contentId, SessionController.Instance.ContentController.Get(contentId).Data);
            await nusysSession.ExecuteSystemRequest(request, NetworkClient.PacketType.TCP, new List<string>() { senderIP});
        }
    }
}
