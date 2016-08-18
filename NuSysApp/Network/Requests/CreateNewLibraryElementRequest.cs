using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using WinRTXamlToolkit.Tools;

namespace NuSysApp
{
    /// <summary>
    /// This request should only be used to create a LibraryElement when a content for that library element already exists.
    /// After a successful request, call AddReturnedLibraryElementToLibrary() to add the element locally.
    /// </summary>
    public class CreateNewLibraryElementRequest : Request
    {
        /// <summary>
        /// default constructor for from-server deserialization
        /// </summary>
        /// <param name="m"></param>
        public CreateNewLibraryElementRequest(Message m) : base(NusysConstants.RequestType.CreateNewLibraryElementRequest, m)
        {
        }

        /// <summary>
        /// DEPRECATED, SHOULD REMOVE
        /// pack the message here for creating a new library element. 
        /// In reality, this request should probably only be used for creating regions
        /// </summary>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <param name="type"></param>
        /// <param name="title"></param>
        public CreateNewLibraryElementRequest(string id, string data, NusysConstants.ElementType type, string title = ""): base(NusysConstants.RequestType.CreateNewLibraryElementRequest)
        {
            Debug.Fail("Congrats you found a deprecated method that's causing randon bad shit to happen in NuSys!  " +
                       "You should tell Me (Trent) so we can fix this so we don't call this method anymore.");
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = id;
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = data;
            _message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = type.ToString();
            if (title != null)
            {
                _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = title;
            }
        }

        /// <summary>
        /// preffered constructor for the CreateNewLibraryElementRequest.
        /// Takes in an arguments class.  Check the class properties to see which are required.  
        /// </summary>
        /// <returns></returns>
        public CreateNewLibraryElementRequest(CreateNewLibraryElementRequestArgs args) :  base(args,NusysConstants.RequestType.CreateNewLibraryElementRequest){}

        /// <summary>
        /// this method will parse and add the returned library Element after the request has successfully returned. 
        /// Will throw an exception if the request has not returned yet or has failed. 
        /// Returned whether the new libraryElementSuccessfully was added
        /// </summary>
        /// <returns></returns>
        public bool AddReturnedLibraryElementToLibrary()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }

            //make sure the returned model is present
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY));
            var modelString = _returnMessage.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY);

            return AddModelStringToSession(modelString);
        }

        /// <summary>
        /// this method can be called to add a json-serialized LibraryElementModel to the current session.  
        /// It should be called whenever the AddReturnedLibraryElementToLibrary or the ExecuteRequestFunction methods are called.
        /// It will return whether the element was succesfully added.
        /// </summary>
        /// <param name="libraryElementModelString"></param>
        /// <returns></returns>
        private bool AddModelStringToSession(string libraryElementModelString)
        {
            var libraryElement = LibraryElementModelFactory.DeserializeFromString(libraryElementModelString);
            return SessionController.Instance.ContentController.Add(libraryElement) != null;
        }

        /// <summary>
        /// simlply debug.asserts the important ID's.  
        /// Then adds a couple timestamps to the outgoing request message;
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            var time = DateTime.UtcNow.ToString();
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));
            _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY] = time;
            _message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LAST_EDITED_TIMESTAMP_KEY] = time;
        }

        /// <summary>
        /// this method should simply add the returned libraryElementModel to the session.  
        /// This will be called when another client adds a library element OR a new content since a default library element is made
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            //make sure the key for the json is present
            Debug.Assert(_message.ContainsKey(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY));

            //get the json and add it to the session
            var modelString = _message.GetString(NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY);
            AddModelStringToSession(modelString);
        }
    }
}
