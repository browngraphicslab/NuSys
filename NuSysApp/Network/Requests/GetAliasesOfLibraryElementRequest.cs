using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// This request can be used to get all the element models of a certain library element.  
    /// It should take in just the library element id and then, after execution, give back a list of element models
    /// </summary>
    public class GetAliasesOfLibraryElementRequest : Request
    {
        /// <summary>
        /// this constructor takes in the library element id of the library element you want to find element models of.
        /// After creating this request and awaiting its successful return, call GetReturnedElementModels to get the list of requested elements;
        /// </summary>
        /// <param name="libraryElementId"></param>
        public GetAliasesOfLibraryElementRequest(string libraryElementId) : base(NusysConstants.RequestType.GetAliasesOfLibraryElementRequest)
        {
            _message[NusysConstants.GET_ALIASES_OF_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY] = libraryElementId;
        }

        /// <summary>
        /// this check outgoing request method just checks to see if the library element id is present in the outoging message.
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_ALIASES_OF_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY));
        }

        /// <summary>
        /// gets the list of returned element models from a successful request.  
        /// Call this after the request has returned and was successful. 
        /// </summary>
        /// <returns></returns>
        public List<ElementModel> GetReturnedElementModels()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_ALIASES_OF_LIBRARY_ELEMENT_REQUEST_RETURNED_ELEMENTS_MODELS_KEY));

            var models = _returnMessage.GetList<ElementModel>(NusysConstants.GET_ALIASES_OF_LIBRARY_ELEMENT_REQUEST_RETURNED_ELEMENTS_MODELS_KEY);//get the list of models

            return models;
        }
    }
}
