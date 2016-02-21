using System.Threading.Tasks;
using SQLiteNetExtensions.Extensions;

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
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, ServerClient serverClient)
        {
            nusysSession.DropNetworkUser(_message.GetString("ip"));
        }
    }
}
