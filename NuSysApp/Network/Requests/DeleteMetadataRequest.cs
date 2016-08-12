using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    class DeleteMetadataRequest : Request
    {
        public DeleteMetadataRequest(Message message) : base(NusysConstants.RequestType.DeleteMetadataRequest, message)
        {

        }

        public DeleteMetadataRequest(DeleteMetadataRequestArgs requestArgs) : base(requestArgs, NusysConstants.RequestType.DeleteMetadataRequest)
        {

        }

        /// <summary>
        /// Ensures the outgoing request has the keys it needs to have.
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY));
        }

        /// <summary>
        /// the method executed locally when another client deletes a metadata entry. 
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY));

            var libraryId = _message.GetString(NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY);
            var entryKeyToDelete = _message.GetString(NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY);

            DeleteMetadataEntry(libraryId, entryKeyToDelete);
        }

        /// <summary>
        /// the method to be called after the request is successful.  
        /// This method must be called from the original client if they want to delete the item locally.
        /// It will return true if the element was removed, false otherwise.  
        /// </summary>
        /// <returns></returns>
        public bool DeleteLocally()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY));

            //get the library id of the item entry key for the metadata that is do be deleted
            var libraryId = _returnMessage.GetString(NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY);
            var entryKeyToDelete = _returnMessage.GetString(NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY);

            //return whether the deletion was successful
            return DeleteMetadataEntry(libraryId, entryKeyToDelete);
        }

        /// <summary>
        /// the private method to be executed by either the ExecuteRequestFunction() or the explicit DeleteLocally().  
        /// This method actually deletes the metadata entry on the client side.
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        private bool DeleteMetadataEntry(string libraryId, string metadataKey)
        {
            var libraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(libraryId);
            if (libraryElementController == null)
            {
                return false;
            }
            
            UITask.Run(delegate {
                libraryElementController.RemoveMetadata(metadataKey);
            });
            return true;
        }
    }
}
