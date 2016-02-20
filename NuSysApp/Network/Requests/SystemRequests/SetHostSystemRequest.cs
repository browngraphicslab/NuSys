using System.Threading.Tasks;

namespace NuSysApp
{
    public class SetHostSystemRequest : SystemRequest
    {
        private string _ip;
        public SetHostSystemRequest(string ip) : base(SystemRequestType.SetHost)
        {
            _message["ip"] = ip;
        }

        public SetHostSystemRequest(Message m) : base(m) { }
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, ServerClient serverClient)
        {
            nusysSession.SetHost(_message.GetString("ip"));
        }
    }
}
