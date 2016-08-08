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
    public class CreateNewLibraryElementRequest : Request
    {
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
        public CreateNewLibraryElementRequest(string id, string data, NusysConstants.ElementType type, string title = "")
            : base(NusysConstants.RequestType.CreateNewLibraryElementRequest)
        {
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
        public CreateNewLibraryElementRequest(CreateNewLibraryElementRequestArgs args) :  base(NusysConstants.RequestType.CreateNewLibraryElementRequest)
        {
            var message = args.PackToRequestKeys();
            _message.ForEach(kvp => message[kvp.Key] = kvp.Value);
            _message = message;
        }

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

            var libraryElement = LibraryElementModelFactory.DeserializeFromString(modelString);
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
    }
}
