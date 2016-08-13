using System;
using System.Collections.Generic;
using System.Linq;
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
        protected Message _returnMessage;
        
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
            _requestType = _message.GetEnum<NusysConstants.RequestType>(NusysConstants.REQUEST_TYPE_STRING_KEY);//set the request type

        }

        /// <summary>
        /// optional constructor for requests on the client-side.  
        /// You should populate the correct IRequestArgumentable and then pass it in here.  
        /// This will simply merge the IRequestArgumentable's PackToRequestKeys() message and merge it with the protected _message in all requests.
        /// Any merge conflicts will result in the _message class's original value for the conflicted key;
        /// 
        /// This also will need to take in the request type
        /// </summary>
        /// <param name="requestArgs"></param>
        public Request(IRequestArgumentable requestArgs, NusysConstants.RequestType requestType) : this(requestType)
        {
            //gets the keys from the RequestArgs class using the IRequestArgumentable's PackToRequestKeys()
            var argsMessage = requestArgs.PackToRequestKeys();

            //for each key-value-pair in _message, add it to the argsMessage
            foreach(var kvp in _message)
            {
                argsMessage[kvp.Key] = kvp.Value;
            };

            //set the new _message to be the updated args message
            _message = argsMessage;
        }

        public Message GetFinalMessage()
        {
            _message[NusysConstants.REQUEST_TYPE_STRING_KEY] = _requestType.ToString();
            _message["system_sent_timestamp"] = DateTime.UtcNow.Ticks;//TODO fix this
            return _message;
        }

        /// <summary>
        /// should only be called from the Server itself.
        /// This method should only have one reference. 
        /// NUSYSAPP SHOULD NEVER CALL THIS METHOD
        /// </summary>
        /// <returns></returns>
        public Message GetMessage()
        {
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
        /// this method should only ever be called by the server in NusysNetworkSession.  
        /// Should only have one reference
        /// </summary>
        /// <param name="returnMessage"></param>
        public void SetReturnMessage(Message returnMessage)
        {
            _returnMessage = returnMessage;
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
        }

        /// <summary>
        /// This function will be called when somebody else makes this request and it is forwarded to you.
        /// 
        /// For instance, A CreateNewLibraryElementRequest would return to the person who sent it.
        /// At the same time, the other clients would get the message and this would be called. 
        /// </summary>
        /// <returns></returns>
        public virtual async Task ExecuteRequestFunction() { }

        /// <summary>
        /// tells whether an executed requst was succesful or not.  
        /// returns null if the request hasnt returned yet.  
        /// also returns null if the message wasn't formatted corectly after return
        /// </summary>
        /// <returns></returns>
        public bool? WasSuccessful()
        {
            if (_returnMessage == null)
            {
                return null;
            }
            if (!_returnMessage.ContainsKey(NusysConstants.REQUEST_SUCCESS_BOOL_KEY))
            {
                return null;
            }
            return _returnMessage.GetBool(NusysConstants.REQUEST_SUCCESS_BOOL_KEY);
        }

        /// <summary>
        /// this method can be called in local request methods to ensure that that method is being executed only after a succesful request returns. 
        /// This will throw an exception if the request hasn't returned or was unsuccesful. 
        /// Saves from repeatedly copy and pasting code.
        /// </summary>
        protected void CheckWasSuccessfull()
        {
            if (WasSuccessful() != true)//Weird syntax because of nullable bool
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }
        }
        public class InvalidRequestTypeException : Exception
        {
            public InvalidRequestTypeException(string message) : base(message) { }
            public InvalidRequestTypeException() : base("The request type was invalid") { }
        }
    }
}
