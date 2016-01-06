using System.Threading.Tasks;

namespace NuSysApp
{
    public class RemoveClientSystemRequest : SystemRequest
    {
        private string _ip;
        public RemoveClientSystemRequest(string ip) : base(SystemRequestType.AddClient)
        {
            _message["ip"] = ip;
        }

        public RemoveClientSystemRequest(Message m) : base(m) { }
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session)
        {
            session.RemoveIP(_ip);
        }
    }
}
