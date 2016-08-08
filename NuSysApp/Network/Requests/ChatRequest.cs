﻿using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace NuSysApp
{
    public class ChatRequest : Request
    {
        public ChatRequest(NetworkUser user, string chatMessage) : base(RequestType.ChatRequest)
        {
            Debug.Assert(chatMessage != null);
            Debug.Assert(user != null);
            _message["chat_message"] = chatMessage;
            _message["user"] = user.ID;
            setServerSettings();
        }

        private void setServerSettings()
        {
            //all clients need to listen for it
            SetServerEchoType(ServerEchoType.Everyone);
            SetServerItemType(ServerItemType.Alias);
            //server doesn't have to do anything except give the request to all clients
            SetServerIgnore(true);
            SetServerRequestType(ServerRequestType.Update);
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
                var chatMessage = _message.GetString("chat_message");
                Debug.Assert(chatMessage != null);
                string userAndMessage = user.Name + ": " + chatMessage;
                ChatBoxView cBox = SessionController.Instance.SessionView.GetChatBox();
                cBox.AppendText(user, chatMessage);

                //chatbot stuff
                string[] chatArr = chatMessage.Split(' ');
                if (Array.IndexOf(chatArr, "hey") > -1 || Array.IndexOf(chatArr, "Hey") > -1
                    || Array.IndexOf(chatArr, "hi") > -1 || Array.IndexOf(chatArr, "Hi") > -1)
                {
                    string another = "I only say hi to good coders";
                    cBox.AppendText(new NetworkUser("slackbot"), another);
                }

                //if the chatbox is closed
                if (SessionController.Instance.SessionView.GetChatBox().Visibility.Equals(Visibility.Collapsed))
                {
                    SessionController.Instance.SessionView.IncrementUnseenMessage();
                }
            }
            else
            {
                throw new Exception("user not in NetworkUsers");
            }
        }

    }
}