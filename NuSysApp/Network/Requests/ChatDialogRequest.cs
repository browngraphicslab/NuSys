using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Network.Requests
{
    public class ChatDialogRequest : Request
    {
        public ChatDialogRequest(string text) : base(RequestType.ChatDialogRequest)
        {
            _message["text"] = text;
        }

        public ChatDialogRequest(Message m) : base(RequestType.ChatDialogRequest, m){}
        public override async Task ExecuteRequestFunction()
        {
            var session = SessionController.Instance;
            var time = _message.GetLong("system_sent_timestamp");
            var userip = _message.GetString("system_sender_ip");
            var user = session.NuSysNetworkSession.NetworkMembers.ContainsKey(userip) ? session.NuSysNetworkSession.NetworkMembers[userip] : null;
            var text = _message.GetString("text");
            if (time != 0 && user != null)
            {
                session.SessionView.ChatPopupWindow.AddText(text,time,user);
            }
        }
    }
}
