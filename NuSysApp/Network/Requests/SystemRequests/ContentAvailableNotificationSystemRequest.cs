using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Network.Requests.SystemRequests
{
    public class ContentAvailableNotificationSystemRequest : SystemRequest
    {
        public ContentAvailableNotificationSystemRequest(string contentId)
            : base(SystemRequestType.ContentAvailableNotification)
        {
            _message["contentId"] = contentId;
            _message["fetchIP"] = SessionController.Instance.NuSysNetworkSession.HostIP;
        }
        public ContentAvailableNotificationSystemRequest(Message m) : base(m){}
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, string senderIP)
        {
            var list = new List<string>();
            list.Add(_message.GetString("fetchIP"));
            var request = new FetchContentSystemRequest(_message.GetString("contentId"));
            await nusysSession.ExecuteSystemRequest(request, NetworkClient.PacketType.TCP,list);
        }
    }
}
