using NusysIntermediate;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class MoveElementToCollectionRequest : Request
    {
        /// <summary>
        /// Used to create request when server sends back message. Should not be used to create the request to send of to the server.
        /// </summary>
        public MoveElementToCollectionRequest(Message message) : base(NusysConstants.RequestType.MoveElementToCollectionRequest, message)
        {
        }

        /// <summary>
        /// You should be using this constructor to create a MoveElementToCollectionRequest that will be sent to the server . 
        /// Takes in an arguments class. Populate the args class with the proper information then pass it into this method.
        ///To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call MoveElementToCollectionLocally.
        /// </summary>
        /// <param name="args"></param>
        public MoveElementToCollectionRequest(MoveElementToCollectionRequestArgs args) : base(args, NusysConstants.RequestType.MoveElementToCollectionRequest)
        {

        }

        /// <summary>
        /// Ensures the outgoing request has the keys it needs to have, in this case, the element id, 
        /// the new parent collection id, and the new coordinates of the element
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY));
        }

        /// <summary>
        /// This should be called when the server forwards the request to the client (except the client who initially created the request). 
        /// It should move the element to the new parent collection locally.
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY));
            Debug.Assert(_message.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY));
            var elementId = _message.GetString(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY);
            var newParentCollectionId = _message.GetString(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY);
            var x = _message.GetInt(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY);
            var y = _message.GetInt(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY);

            //TODO: Actually move the element to the new parent collection locally
        }

        /// <summary>
        /// It should move the element to the new parent collection locally. If the request hasn't returned yet 
        /// or was not successful this will throw an exception. It returns whether or not the move was successful or not.
        /// </summary>
        public bool UpdateContentLocally()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY));
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY));
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY));
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY));
            var elementId = _returnMessage.GetString(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_ELEMENT_ID_KEY);
            var newParentCollectionId = _returnMessage.GetString(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_NEW_PARENT_COLLECTION_ID_KEY);
            var x = _returnMessage.GetInt(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_X_KEY);
            var y = _returnMessage.GetInt(NusysConstants.MOVE_ELEMENT_TO_COLLECTION_REQUEST_Y_KEY);
            //TODO: Actually move the element to the new parent collection locally
            return false;

        }


    }
}
