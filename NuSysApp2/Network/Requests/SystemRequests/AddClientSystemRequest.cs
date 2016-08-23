using System.Collections.Generic;
using System.Threading.Tasks;

namespace NuSysApp2
{
    /*
    public class AddClientSystemRequest : SystemRequest
    {
        private string _ip;
        public AddClientSystemRequest(string ip) : base(SystemRequestType.AddClient)
        {
            _message["ip"] = ip;
        }

        public AddClientSystemRequest(Message m) : base(m){}
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, ServerClient serverClient)
        {
            session.AddIP(_message.GetString("ip"));
            if (senderIP != null)
            {
                var list = new List<string>();
                list.Add(senderIP);

                var request = new SetHostSystemRequest(nusysSession.HostIP);//set host
                await nusysSession.ExecuteSystemRequest(request, NetworkClient.PacketType.TCP, list);

                if (nusysSession.IsHostMachine)
                {
                    await nusysSession.ExecuteSystemRequest(new SendWorkspaceRequest(),NetworkClient.PacketType.TCP,list);//send entire workspace
                }

                var clientRequest = new SendClientInfoSystemRequest();//send client info
                await nusysSession.ExecuteSystemRequest(clientRequest, NetworkClient.PacketType.TCP, list);
            }
        }
    }*/
}
