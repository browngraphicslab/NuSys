using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NusysConstants
{
    public abstract class Request
    {
        protected Message _message;
        private ServerConstants.RequestType _requestType;
        public Request(ServerConstants.RequestType request, Message message = null)
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
            _requestType = (ServerConstants.RequestType)Enum.Parse(typeof(ServerConstants.RequestType), message.GetString("request_type"));//set the request type

        }
        public Message GetFinalMessage()
        {
            _message["request_type"] = _requestType.ToString();
            _message["system_sent_timestamp"] = DateTime.UtcNow.Ticks;
            return _message;
        }

        public ServerConstants.RequestType GetRequestType()
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

        public virtual async Task<bool> CheckOutgoingRequest()
        {
            return true;
        }//for anything you want to check right before execution

        //the function to be executed per the request
        public virtual async Task ExecuteRequestFunction() { }


        public class InvalidRequestTypeException : Exception
        {
            public InvalidRequestTypeException(string message) : base(message) { }
            public InvalidRequestTypeException() : base("The request type was invalid") { }
        }
    }
}
