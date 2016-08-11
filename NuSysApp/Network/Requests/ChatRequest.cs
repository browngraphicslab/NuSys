using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class ChatRequest : Request
    {
        /// <summary>
        /// ChatRequests needs references to the user that sent the request, and the string message they want to 
        /// send to other clients via the chat window.
        /// </summary>
        /// <param name="user"></param>
        /// <param name="chatMessage"></param>
        public ChatRequest(NetworkUser user, string chatMessage) : base(NusysConstants.RequestType.ChatRequest)
        {
            Debug.Assert(chatMessage != null);
            Debug.Assert(user != null);
            _message[NusysConstants.CHAT_REQUEST_CHAT_MESSAGE_KEY] = chatMessage;
            _message[NusysConstants.CHAT_REQUEST_USER_ID_KEY] = user.ID;
        }

        /// <summary>
        /// Constructor to use when you've already made a Message to pass in
        /// </summary>
        /// <param name="m"></param>
        public ChatRequest(Message m) : base(NusysConstants.RequestType.ChatRequest, m)
        {
        }

        public override async Task ExecuteRequestFunction()
        {
            // Error checking -- throws an exception if the user does not exist
            var userID = _message.GetString(NusysConstants.CHAT_REQUEST_USER_ID_KEY);
            var userExists = SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(userID);
            if (!userExists)
            {
                throw new Exception("user not in NetworkUsers");
            }

            // Retreivers user and chat message strings, and makes sure they are not null
            var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userID];
            Debug.Assert(user != null);
            var chatMessage = _message.GetString("chat_message");
            Debug.Assert(chatMessage != null);

            // Obtains the chatbox and calls one of its methods to update the text
            var cBox = SessionController.Instance.SessionView.GetChatBox();
            cBox.AppendText(user, chatMessage);       

            // If the chatbox is closed, make sure to notify the client about receiving a message 
            if (SessionController.Instance.SessionView.GetChatBox().Visibility.Equals(Visibility.Collapsed))
            {
                SessionController.Instance.SessionView.IncrementUnseenMessage();
            }

            // Chatbot stuff
            var chatArr = chatMessage.Split(' ');
            if (Array.IndexOf(chatArr, "hey") > -1 || Array.IndexOf(chatArr, "Hey") > -1
                || Array.IndexOf(chatArr, "hi") > -1 || Array.IndexOf(chatArr, "Hi") > -1)
            {
                string another = "I only say hi to good coders";
                cBox.AppendText(new NetworkUser("slackbot"), another);
            }

        }

    }
}