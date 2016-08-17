using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// this request is used to fetch from the server the entire initial dicionary of User Id's to display Names.  
    /// To use this request, await its execution and then call AddReturnedDictionaryToSession.
    /// </summary>
    public class GetUserIdToDisplayNameDictionaryRequest : Request
    {
        /// <summary>
        /// Preferred constructor.
        /// Takes in nothing because it is clear what you will be fetching -- the whole dictionary;
        /// </summary>
        public GetUserIdToDisplayNameDictionaryRequest() : base(NusysConstants.RequestType.GetUserIdToDisplayNameDictionaryRequest) { }

        /// <summary>
        /// method to be called after the request has returned from the server and was succesfull.  
        /// This method will add the dictionary to the NusysNetworkSession. 
        /// This will throw an exception if the request was not successful or has not returned yet.  
        /// Use WasSuccessfull() before calling this method to check those conditions.
        /// </summary>
        public void AddReturnedDictionaryToSession()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_USER_ID_TO_DISPLAY_NAME_DICTIONARY_REQUEST_RETURNED_DICTIONARY));
            
            //get the dictionary from the returned message
            var dictionary = _returnMessage.Get<Dictionary<string, string>>(NusysConstants.GET_USER_ID_TO_DISPLAY_NAME_DICTIONARY_REQUEST_RETURNED_DICTIONARY);

            SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary = dictionary;
        }

    }
}
