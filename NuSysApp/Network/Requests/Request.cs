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
            SystemRequest,
            NewLinkRequest,
            SendableUpdateRequest,
            NewGroupRequest,
            NewContentRequest
        }
        protected Message _message;
        private RequestType _requestType;
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

        public Message GetFinalMessage()
        {
            _message["request_type"] = _requestType.ToString();
            return _message;
        }

        public RequestType GetRequestType()
        {
            return _requestType;
        }

        public virtual async Task CheckOutgoingRequest(){}//for anything you want to check right before execution

        public virtual async Task ExecuteRequestFunction(){}//the function to be executed per the request

        public class InvalidRequestTypeException : Exception
        {
            public InvalidRequestTypeException(string message) : base(message) { }
            public InvalidRequestTypeException() :base("The request type was invalid"){ }
        }
    }
}
