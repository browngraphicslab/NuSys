using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class AddClientSystemRequest : SystemRequest
    {
        private string _ip;
        public AddClientSystemRequest(string ip) : base(SystemRequestType.AddClient)
        {
            _message["ip"] = ip;
        }

        public AddClientSystemRequest(Message m) : base(m){}
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, string senderIP)
        {
            session.AddIP(_ip);
            if (senderIP != null)
            {
                var request = new SetHostSystemRequest(senderIP);
                var list = new List<string>();
                list.Add(senderIP);
                await nusysSession.ExecuteSystemRequest(request, NetworkClient.PacketType.TCP, list);
            }
        }
    }
}
