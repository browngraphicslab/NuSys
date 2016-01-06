using System.Threading.Tasks;

namespace NuSysApp
{
    public class SetHostSystemRequest : SystemRequest
    {
        private string _ip;
        public SetHostSystemRequest(string ip) : base(SystemRequestType.AddClient)
        {
            _message["ip"] = ip;
        }

        public SetHostSystemRequest(Message m) : base(m) { }
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session)
        {
            nusysSession.SetHost(_ip);
        }
    }
}
