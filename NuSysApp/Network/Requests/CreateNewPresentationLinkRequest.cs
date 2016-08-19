using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using Windows.Data.Json;
using Newtonsoft.Json;

namespace NuSysApp
{
    class CreateNewPresentationLinkRequest : Request
    {
        /// <summary>
        /// used when the server sends back a message
        /// </summary>
        /// <param name="message"></param>
        public CreateNewPresentationLinkRequest(Message message) : base(NusysConstants.RequestType.CreateNewPresentationLinkRequest, message)
        {
        }

        /// <summary>
        /// Prefered constructor When creating new request to send to the server. To use, create new CReateNewPresentationLinkRequestArgs class and 
        /// pass in the corresponding data to it. To use this request await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call AddPresentationLinkToLibrary
        /// </summary>
        /// <param name="requestArgs"></param>
        public CreateNewPresentationLinkRequest(CreateNewPresentationLinkRequestArgs requestArgs) : base(requestArgs,  NusysConstants.RequestType.CreateNewPresentationLinkRequest)
        {
        }

        /// <summary>
        /// this method will parse and add the returned presentation link after the request has successfully returned. 
        /// Will throw an exception if the request has not returned yet or has failed. 
        /// Returned whether the new presentation link was added
        /// </summary>
        /// <returns></returns>
        public async Task<bool> AddPresentationLinkToLibrary()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }

            //make sure the returned model is present
            //make sure the key for the json is present
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY));

            //get the json and add it to the session
            PresentationLinkModel model = JsonConvert.DeserializeObject<PresentationLinkModel>(_returnMessage.GetString(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY));

            return await SessionController.Instance.LinksController.AddPresentationLinkToLibrary(model);
        }

        /// <summary>
        /// This is called when the server sends a message to the client (except the client who initially created the request). It should create a new presentation link client side.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            //make sure the key for the json is present
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY));

            //get the json and add it to the session
            PresentationLinkModel model = JsonConvert.DeserializeObject<PresentationLinkModel>( _message.GetString(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY));

            // since we always create presentation links client side the same way, just call AddPresenationLinkToLibrary
            await SessionController.Instance.LinksController.AddPresentationLinkToLibrary(model);
        }

        /// <summary>
        /// just checks to see if the message contains the necessary keys
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY));
        }
    }
}
