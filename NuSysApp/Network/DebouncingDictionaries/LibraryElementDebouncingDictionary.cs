using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the debouncing dictionary sub class should be used for all library elements.
    /// This class should only be used in the Library element controller and should only be used to update properties.
    /// For mor information about debouncing dictionary, check out the description of the base class.
    /// </summary>
    public class LibraryElementDebouncingDictionary : DebouncingDictionary
    {
        /// <summary>
        /// constructor just takes in the LibraryElement Id of the object that will be getting updated from this debouncing dictionary.  
        /// </summary>
        /// <param name="libraryElementId"></param>
        public LibraryElementDebouncingDictionary(string libraryElementId) : base(libraryElementId) { }

        /// <summary>
        /// this override simply updates the server with the latest values. 
        ///  The values are stored in the message argument.  
        /// This method will just sned a UpdateLibraryElementModelRequest, used to update the library element;
        /// </summary>
        /// <param name="message"></param>
        /// <param name="shouldSave"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        protected override async Task SendToServer(Message message, bool shouldSave, string objectId)
        {
            message[NusysConstants.UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID] = objectId; //set the id of the library element to be updated;
            var request = new UpdateLibraryElementModelRequest(message);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }
    }
}
