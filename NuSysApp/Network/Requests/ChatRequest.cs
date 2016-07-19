using System.Diagnostics;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class ChatRequest : Request
    {
        public ChatRequest(NetworkUser user, string chatMessage) : base(RequestType.ChatRequest)
        {
            _message["chat_message"] = chatMessage;
            _message["user"] = user.ID;
            setServerSettings();
        }

        public ChatRequest(Message m) : base(RequestType.ChatRequest, m)
        {
            setServerSettings();
        }

        public override async Task ExecuteRequestFunction()
        {
            if (SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(_message.GetString("user")))
            {
                var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[_message.GetString("user")];
                Debug.Assert(user != null);
                string oldText = SessionController.Instance.SessionView.GetChatBox().ChatText;
                var chatMessage = _message.GetString("chat_message");
                Debug.Assert(chatMessage != null);
                string withUser = user.Name + ": " + chatMessage + "\n";
                SessionController.Instance.SessionView.GetChatBox().ChatText = oldText+withUser;
            }
        }

        private void setServerSettings()
        {
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            SetServerIgnore(true);
            SetServerRequestType(ServerRequestType.Update);
        }

    }
}