using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SendableUpdateRequest : Request
    {
        public SendableUpdateRequest(Message m) : base(RequestType.SendableUpdateRequest, m) { }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                throw new Exception("The Sendable update must have a key labeled 'id'");
            }
        }
        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            if (SessionController.Instance.IdToSendables.ContainsKey(id))
            {
                Sendable sendable = SessionController.Instance.IdToSendables[id];
                await sendable.UnPack(_message);
            }
        }
    }
}
