using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;
using Windows.UI.Xaml;

namespace NuSysApp
{
    /// <summary>
    /// a request to send a chat message.  In order to add it locally, you must call AddSuccesfullChatLocally
    /// </summary>
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
            _message[NusysConstants.CHAT_REQUEST_USER_ID_KEY] = user.UserID;
        }

        /// <summary>
        /// Constructor to use when you've already made a Message to pass in
        /// </summary>
        /// <param name="m"></param>
        public ChatRequest(Message m) : base(NusysConstants.RequestType.ChatRequest, m)
        {
        }

        /// <summary>
        /// this method must be called in order to execute the request locally.  
        /// After a succressful request has returned, this can be called to actually add the message to your local chatbox as well as the other clients' chat boxes
        /// </summary>
        public void AddSuccesfullChatLocally()
        {
            CheckWasSuccessfull();

            var userID = _returnMessage.GetString(NusysConstants.CHAT_REQUEST_USER_ID_KEY);
            var userExists = SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(userID);
            if (!userExists)
            {
                throw new Exception("user not in NetworkUsers");
            }

            // Retreivers user and chat message strings, and makes sure they are not null
            var user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userID];
            Debug.Assert(user != null);
            var chatMessage = _returnMessage.GetString(NusysConstants.CHAT_REQUEST_CHAT_MESSAGE_KEY);
            Debug.Assert(chatMessage != null);

            AddChat(user, chatMessage);
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
            var chatMessage = _message.GetString(NusysConstants.CHAT_REQUEST_CHAT_MESSAGE_KEY);
            Debug.Assert(chatMessage != null);


            AddChat(user,chatMessage);
        }

        /// <summary>
        /// private method to actually do the adding of the chat message.
        /// 
        /// The user id is the id of the network user that sent the message.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="chat"></param>
        private void AddChat(NetworkUser user, string chatMessage)
        {
            // Obtains the chatbox
            var cBox = SessionController.Instance.SessionView?.GetChatBox();

            // if the chat box is null, the sessionview hasn't been instantiated yet so return
            if (cBox == null)
            {
                return;
            }

            // update the text in the chat box
            cBox.AppendText(user, chatMessage);

            // If the chatbox is closed, make sure to notify the client about receiving a message 
            if (SessionController.Instance.SessionView.GetChatBox().Visibility.Equals(Visibility.Collapsed))
            {
                //TODO factor this out into the chatbox
                //SessionController.Instance.SessionView.IncrementUnseenMessage();
            }

            // Chatbot stuff
            var chatArr = chatMessage.Split(' ');
            if (Array.IndexOf(chatArr, "hey") > -1 || Array.IndexOf(chatArr, "Hey") > -1
                || Array.IndexOf(chatArr, "hi") > -1 || Array.IndexOf(chatArr, "Hi") > -1)
            {
                string another = "I only say hi to good coders";
                cBox.AppendText(new NetworkUser("slackbot") {DisplayName = "SlackBot"}, another);
            }
        }

    }
}