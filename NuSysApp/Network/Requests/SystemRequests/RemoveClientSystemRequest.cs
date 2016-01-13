using System.Threading.Tasks;

namespace NuSysApp
{
    public class RemoveClientSystemRequest : SystemRequest
    {
        private string _ip;
        public RemoveClientSystemRequest(string ip) : base(SystemRequestType.RemoveClient)
        {
            _message["ip"] = ip;
        }

        public RemoveClientSystemRequest(Message m) : base(m) { }
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, string senderIP)
        {
            string ip = _message.GetString("ip");
            session.RemoveIP(ip);
            if (nusysSession.NetworkMembers.ContainsKey(ip))
            {
                nusysSession.NetworkMembers.Remove(ip);
            }
        }
    }
}
