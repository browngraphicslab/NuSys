using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the deboucncing dictionary used to update Elements; 
    /// This subclass of the abstract DebouncingDictionary should only be used in the ElementControllerClass.
    /// It will be used to add properties to the base class to be updated whenever the timer expires.
    /// Check the base DebouncingDictionary class description to learn more about debouncing dictionaries in general;
    /// </summary>
    public class ElementDebouncingDictionary : DebouncingDictionary
    {
        /// <summary>
        /// the constructor only takes in the ID of the ElementModel that this debouncing dictonary will be updating.
        /// </summary>
        /// <param name="elementId"></param>
        public ElementDebouncingDictionary(string elementId) : base(elementId){}

        /// <summary>
        /// this override method will take the properties specified in the passed Message, and add them to a server call for updating elements.
        /// This method will make an ElementUpdateRequest with the properties.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="shouldSave"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        protected override async Task SendToServer(Message message, bool shouldSave, string objectId)
        {
            message[NusysConstants.ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY] = objectId;
            var request = new ElementUpdateRequest(message, shouldSave);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }
    }
}
