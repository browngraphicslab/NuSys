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
        /// Takes in a Key with all the Database keys for the properties being updated. 
        /// Must contain in the message the Id of the LibraryElementModel to update with the NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID key; 
        /// The Check Outgoing Request will catch you if you dont include that Id.
        /// </summary>
        /// <param name="m"></param>
        public UpdateLibraryElementModelRequest(Message m) : base(NusysConstants.RequestType.UpdateLibraryElementModelRequest, m){ }

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
            controller.UnPack(_message);
            if (_message.ContainsKey("favorited"))
            {
                controller.SetFavorited(bool.Parse(_message["favorited"].ToString()));
            }
            //TODO fix collection inking
            if (_message.ContainsKey("inklines"))
            {

                var inkIds = _message.GetList<string>("inklines");
                var collectionController = (CollectionLibraryElementController)controller;
                var oldInkLines = collectionController.InkLines;
                var added = inkIds.Except(oldInkLines).ToArray();
                var removed = oldInkLines.Except(inkIds).ToArray();

                foreach (var idremoved in removed)
                {
                    collectionController.RemoveInk(idremoved);
                }

                foreach (var idadded in added)
                {
                    collectionController.AddInk(idadded);
                }

            }
        }
    }
}
