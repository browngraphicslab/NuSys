using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Input.Inking;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the request to update any and all properties of a libraryElementModel.  
    /// You should make the updates locally and then call this to forward the updates to the server and other clients. 
    /// This should probably only be getting called from the DebouncingDictionary class within the LibraryElementController.
    /// </summary>
    public class UpdateMetadataEntryRequest : Request
    {
        /// <summary>
        /// Alternate constructor. takes in a message contaning the library id, the key whose value is to be changed and the list of new values.
        /// </summary>
        /// <param name="m"></param>
        public UpdateMetadataEntryRequest(Message m) : base(NusysConstants.RequestType.UpdateMetadataEntryRequest, m){ }

        /// <summary>
        /// Takes in the key whose value is being updated. 
        /// Must contain in the message the Id of the library element whose metadata entry needs to be updated 
        /// The Check Outgoing Request will catch you if you dont include that Id.
        /// Usage:
        /// Create request
        /// Execute it
        /// Call UpdateLocally
        /// </summary>
        /// <param name="requestArgs"></param>
        public UpdateMetadataEntryRequest(UpdateMetadataEntryRequestArgs requestArgs) : base(requestArgs, NusysConstants.RequestType.UpdateMetadataEntryRequest)
        {

        }
        /// <summary>
        /// Makes sure that the request has a library id and the key and new value for the entry being updated.
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE));
        }

        /// <summary>
        /// this will be called whenever another client calls this execute request function.  
        /// It will update the metadata entry of interest.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE));

            var key = _message.GetString(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY);
            var value = _message.GetList<string>(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE);

            //get the library element controller to update
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(_message.GetString(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY));
            if (controller == null)
            {
                return;
            }
            controller.UpdateMetadataLocally(key, value);
            
        }

        /// <summary>
        /// the method to be called after the request is successful.  
        /// This method must be called from the original client if they want to update the item locally.
        /// It will return true if the element was removed, false otherwise.  
        /// </summary>
        /// <returns></returns>
        public bool UpdateLocally()
        {
            CheckWasSuccessfull();

            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE));

            var key = _returnMessage.GetString(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY);
            var value = JsonConvert.DeserializeObject<List<string>>(_returnMessage.GetString(NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE));

            //get the library element controller to update
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(_returnMessage.GetString(NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY));
            return controller.UpdateMetadataLocally(key, value);
        }
    }
}
