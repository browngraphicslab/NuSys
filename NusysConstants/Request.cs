using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// ONLY THE SERVER SHOULD EVER INSTANTIATE THIS BASE CLASS.
    /// 
    /// EVERYWHERE ELSE THIS SHOULD BE TREATED AS AN ABSTRACT CLASS
    /// </summary>
    public class Request
    {
        protected Message _message;
        private NusysConstants.RequestType _requestType;
        public Request(NusysConstants.RequestType requestType, Message message = null)
        {
            _message = message;
            if (_message == null)
            {
                _message = new Message();
            }
            _requestType = requestType;
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
            if (!_message.ContainsKey(NusysConstants.REQUEST_TYPE_STRING_KEY))//make sure there exists a request type
            {
                throw new InvalidRequestTypeException("No request type found");
            }
            _requestType = (NusysConstants.RequestType)Enum.Parse(typeof(NusysConstants.RequestType), _message.GetString(NusysConstants.REQUEST_TYPE_STRING_KEY));//set the request type

        }
        public Message GetFinalMessage()
        {
            _message["request_type"] = _requestType.ToString();
            _message["system_sent_timestamp"] = DateTime.UtcNow.Ticks;
            return _message;
        }

        public NusysConstants.RequestType GetRequestType()
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

        /// <summary>
        ///  THIS METHOD SHOULD ONLY BE USED TO VERIFY THE REQUEST CONTAINS THE CORRECT KEYS AND OBJECTS.  
        /// 
        /// Please do not put any logic or calculations in this method.  
        /// It makes it hard to understand where things are happening
        /// </summary>
        /// <returns></returns>
        public virtual async Task CheckOutgoingRequest()
        {
            return;
        }

        /// <summary>
        /// This function will be called if/when the request is to be executed locally.
        /// 
        /// For instance, A CreateNewLibraryElementRequest should acutally make a library element in this method.
        /// Other methods in that subclass would be used to prepare and set up the request to have all the tools needed to execute this request
        /// </summary>
        /// <returns></returns>
        public virtual async Task ExecuteRequestFunction() { }


        public class InvalidRequestTypeException : Exception
        {
            public InvalidRequestTypeException(string message) : base(message) { }
            public InvalidRequestTypeException() : base("The request type was invalid") { }
        }
    }
}
