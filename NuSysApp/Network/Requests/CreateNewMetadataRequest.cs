using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using System.Diagnostics;
using Newtonsoft.Json;

namespace NuSysApp
{
    class CreateNewMetadataRequest : Request
    {
        /// <summary>
        /// Used when the server sends back a message
        /// </summary>
        /// <param name="message"></param>
        public CreateNewMetadataRequest(Message message) : base(NusysConstants.RequestType.CreateNewMetadataRequest, message)
        {

        }
        
        /// <summary>
        /// This is the contructor prefered constructor when creating a new request to send to the server. To use, create new CreateNewMetadataRequest args, 
        /// populate it with corresponding values.
        /// </summary>
        /// <param name="requestArgs"></param>
        public CreateNewMetadataRequest(CreateNewMetadataRequestArgs requestArgs) : base(requestArgs, NusysConstants.RequestType.CreateNewMetadataRequest)
        {
        }

        /// <summary>
        /// simlply debug.asserts the important ID's.  
        /// Then adds a couple timestamps to the outgoing request message;
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY));
        }

        /// <summary>
        /// the method executed locally when another client deletes a metadata entry. 
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY));
            
            var key = _message.GetString(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY);
            var value = _message.GetList<string>(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY);
            var libraryId = _message.GetString(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY);
            string mutability = "";
            if (_message.ContainsKey(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY))
            {
                mutability = _message.GetString(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY);
            }
            else
            {
                mutability = MetadataMutability.MUTABLE.ToString();
            }

            CreateMetadataEntry(libraryId, key, value, (MetadataMutability)Enum.Parse(typeof(MetadataMutability), mutability));
        }

        /// <summary>
        /// the method to be called after the request is successful.  
        /// This method must be called from the original client if they want to delete the item locally.
        /// It will return true if the element was removed, false otherwise.  
        /// </summary>
        /// <returns></returns>
        public bool CreateLocally()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY));
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY));

            var key = _returnMessage.GetString(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY);
            var value = _returnMessage.GetList<string>(NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY);
            var libraryId = _returnMessage.GetString(NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY);
            string mutability = "";
            if (_returnMessage.ContainsKey(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY))
            {
                mutability = _returnMessage.GetString(NusysConstants.METADATA_MUTABILITY_COLUMN_KEY);
            }
            else
            {
                mutability = MetadataMutability.MUTABLE.ToString();
            }

            return CreateMetadataEntry(libraryId, key, value, (MetadataMutability)Enum.Parse(typeof(MetadataMutability), mutability));
        }

        /// <summary>
        /// the private method to be executed by either the ExecuteRequestFunction() or the explicit DeleteLocally().  
        /// This method actually deletes the metadata entry on the client side.
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        private bool CreateMetadataEntry(string libraryId, string metadataKey, List<string> values, MetadataMutability mutability)
        {
            var libraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(libraryId);
            if (libraryElementController == null)
            {
                return false;
            }

            var metadataEntry = new MetadataEntry(metadataKey, values, mutability);

            return libraryElementController.AddMetadata(metadataEntry);
            
        }


    }
}
