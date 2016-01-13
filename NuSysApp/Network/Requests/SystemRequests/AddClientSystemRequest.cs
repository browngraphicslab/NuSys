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
            session.AddIP(_message.GetString("ip"));
            if (senderIP != null)
            {
                var request = new SetHostSystemRequest(nusysSession.HostIP);//set host
                var list = new List<string>();
                list.Add(senderIP);
                await nusysSession.ExecuteSystemRequest(request, NetworkClient.PacketType.TCP, list);

                var clientRequest = new SendClientInfoSystemRequest();//send client info
                await nusysSession.ExecuteSystemRequest(clientRequest);

                if (nusysSession.IsHostMachine)
                {
                    var l = new List<string>();
                    l.Add(senderIP);
                    await nusysSession.ExecuteSystemRequest(new SendWorkspaceRequest(),NetworkClient.PacketType.TCP,l);//send entire workspace
                }

            }
        }
    }
}
