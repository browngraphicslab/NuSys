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
    public class UpdateLibraryElementModelRequest : Request
    {
        /// <summary>
        /// preferred constuctor.  
        /// Takes in a Message with all the Database keys for the properties being updated. 
        /// Must contain in the message the Id of the LibraryElementModel to update with the NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID key; 
        /// The Check Outgoing Request will catch you if you dont include that Id. If you do not want to save the update to server, set savetoserver = false. By default its set to true
        /// </summary>
        /// <param name="m"></param>
        public UpdateLibraryElementModelRequest(Message m, bool saveToServer = true)
            : base(NusysConstants.RequestType.UpdateLibraryElementModelRequest, m)
        {
            _message[NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_SAVE_TO_SERVER_BOOLEAN] = saveToServer;
        }

        /// <summary>
        /// this check outgoing request just adds a timestamp to last edited, and debug asserts that the message contains the correct ID key
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID));
            var time = DateTime.UtcNow.ToString();
            _message[NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY] = time;
        }

        /// <summary>
        /// this will be called whenever another client calls this execute request function.  
        /// It will update the correct library element controller.  
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID));

            //get the library element controller to update
            var controller = SessionController.Instance.ContentController.GetLibraryElementController(_message.GetString(NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID));
            controller?.UnPack(_message);
        }
    }
}
