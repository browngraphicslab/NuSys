using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysConstants;

namespace NuSysApp.Network.Requests
{
    public class ChatDialogRequest : Request
    {
        public ChatDialogRequest(string text) : base(ServerConstants.RequestType.ChatDialogRequest)
        {
            _message["text"] = text;
            _message["senderIP"] = SessionController.Instance.NuSysNetworkSession.LocalIP;
        }

        public ChatDialogRequest(Message m) : base(ServerConstants.RequestType.ChatDialogRequest, m){}
        public override async Task ExecuteRequestFunction()
        {
            var session = SessionController.Instance;
            var time = _message.GetLong("system_sent_timestamp");
            var userip = _message.GetString("senderIP");
            var user = session.NuSysNetworkSession.NetworkMembers.ContainsKey(userip) ? session.NuSysNetworkSession.NetworkMembers[userip] : null;
            var text = _message.GetString("text");
            if (time != 0 && user != null)
            {
                session.SessionView.ChatPopupWindow.AddText(text,time,user);
            }
        }
    }
}
