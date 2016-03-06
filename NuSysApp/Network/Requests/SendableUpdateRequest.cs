using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SendableUpdateRequest : Request
    {
        public SendableUpdateRequest(Message m, bool saveToServer = false) : base(RequestType.SendableUpdateRequest, m)
        {
            SetServerSettings(saveToServer);
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                throw new Exception("The Sendable update must have a key labeled 'id'");
            }
        }

        private void SetServerSettings(bool saveToServer = false)
        {
            SetServerEchoType(ServerEchoType.EveryoneButSender);
            SetServerItemType(ServerItemType.Alias);
            SetServerIgnore(!saveToServer);
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
