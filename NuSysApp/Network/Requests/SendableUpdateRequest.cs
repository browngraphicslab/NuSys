using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SendableUpdateRequest : Request
    {
        public SendableUpdateRequest(Message m) : base(RequestType.SendableUpdateRequest, m)
        {
            SetServerSettings();
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                throw new Exception("The Sendable update must have a key labeled 'id'");
            }
            SetServerSettings();
        }

        private void SetServerSettings()
        {
            SetServerEchoType(ServerEchoType.EveryoneButSender);
            SetServerItemType(ServerItemType.Alias);
            SetServerIgnore(true);
            SetServerRequestType(ServerRequestType.Update);
        }

        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            if (SessionController.Instance.IdToControllers.ContainsKey(id))
            {
                var sendable = SessionController.Instance.IdToControllers[id];
                await sendable.UnPack(_message);
            }
        }
    }
}
