using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NuSysApp
{
    public abstract class Request
    {
        public enum RequestType
        {
            DeleteSendableRequest,
            NewNodeRequest,
            FinalizeInkRequest,
            DuplicateNodeRequest,
            SystemRequest,
            NewLinkRequest,
            SendableUpdateRequest,
            NewThumbnailRequest,
            NewContentRequest,
            ChangeContentRequest,
            SetTagsRequest,
            ChatDialogRequest,
            CreateNewLibrayElementRequest,
            SubscribeToCollectionRequest,
            UnsubscribeFromCollectionRequest
        }

        public enum ServerItemType
        {
            Content,
            Alias
        }

        public enum ServerRequestType
        {
            Add,
            Remove,
            Update
        }

        public enum ServerEchoType
        {
            None,
            Everyone,
            EveryoneButSender
        }

        public enum ServerSubscriptionType
        {
            Subscribe,
            Unsubscribe
        }
        protected Message _message;
        protected bool _serverIgnore = false;
        private ServerItemType _serverItemType;
        private ServerRequestType _serverRequestType;
        private ServerEchoType _serverEchoType = ServerEchoType.None;
        private bool _serverItemTypeSet = false;
        private bool _serverRequestTypeSet = false;
        private RequestType _requestType;
        private bool _makeSubscriptionRequest = false;
        private ServerSubscriptionType _serverSubscriptionType;
        public Request(RequestType request, Message message = null)
        {
            _message = message;
            if (_message == null)
            {
                _message = new Message();
            }
            _requestType = request;
        }

        public Request(Message message)
        {
            if (message == null)
            {
                _message = new Message();
            }
            else
            {
                _message = message;
            }
            if (!message.ContainsKey("request_type"))//make sure there exists a request type
            {
                throw new InvalidRequestTypeException("No request type found");
            }
            _requestType = (RequestType)Enum.Parse(typeof(RequestType), message.GetString("request_type"));//set the request type

        }

        public void SetServerIgnore(bool ignore = true)
        {
            _serverIgnore = ignore;
        }

        protected void SetServerRequestType(ServerRequestType requestType)
        {
            _serverRequestTypeSet = true;
            _serverRequestType = requestType;
        }
        protected void SetServerItemType(ServerItemType itemType)
        {
            _serverItemTypeSet = true;
            _serverItemType = itemType;
        }

        public void SetServerEchoType(ServerEchoType echoType)
        {
            _serverEchoType = echoType;
        }

        public void SetSubscribingToCollection(bool subscribe, ServerSubscriptionType type)
        {
            _makeSubscriptionRequest = subscribe;
            _serverSubscriptionType = type;
        }

        public bool WaitForRequestReturn()
        {
            return _serverEchoType == ServerEchoType.Everyone;
        }
        public Message GetFinalMessage()
        {
            _message["request_type"] = _requestType.ToString();
            if (_serverIgnore)
            {
                _message["server_ignore_request"] = ""; //having the key present will act as the boolean
            }
            else
            {
                if (_makeSubscriptionRequest)
                {
                    _message["server_subscribe_to_collection_bool"] = _serverSubscriptionType.ToString();
                }
                else
                {
                    if (!_serverItemTypeSet || !_serverRequestTypeSet)
                    {
                        throw new Exception(
                            "Request tried to be sent to server without specifying request and item type");
                    }
                    else
                    {
                        _message["server_request_type"] = _serverRequestType.ToString();
                        _message["server_item_type"] = _serverItemType.ToString();
                    }
                }
            }
            _message["server_echo_type"] = _serverEchoType.ToString();
            _message["system_sent_timestamp"] = DateTime.UtcNow.Ticks;
            _message["system_sender_ip"] = SessionController.Instance.NuSysNetworkSession.LocalIP;
            return _message;
        }

        public RequestType GetRequestType()
        {
            return _requestType;
        }

        public void SetSystemProperties(Dictionary<string, object> propertiesDictionary)
        {
            foreach (var kvp in propertiesDictionary)
            {
                _message[kvp.Key] = kvp.Value;
            }
        }

        public virtual async Task CheckOutgoingRequest() { }//for anything you want to check right before execution

        //the function to be executed per the request
        public virtual async Task ExecuteRequestFunction() { }


        public class InvalidRequestTypeException : Exception
        {
            public InvalidRequestTypeException(string message) : base(message) { }
            public InvalidRequestTypeException() : base("The request type was invalid") { }
        }
    }
}
