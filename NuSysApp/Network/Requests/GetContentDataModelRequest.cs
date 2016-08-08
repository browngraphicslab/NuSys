using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class GetContentDataModelRequest : Request
    {
        /// <summary>
        /// this constructor should be used only for post-server deserialization
        /// </summary>
        /// <param name="message"></param>
        public GetContentDataModelRequest(Message message) : base(NusysConstants.RequestType.GetContentDataModelRequest, message)
        {
            
        }

        /// <summary>
        /// this should be the constuctor used the majority of the time
        /// </summary>
        /// <param name="contentId"></param>
        public GetContentDataModelRequest(string contentDataModelId) : base(NusysConstants.RequestType.GetContentDataModelRequest)
        {
            _message[NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_CONTENT_ID_KEY] = contentDataModelId;
        }

        /// <summary>
        /// checks to see if the message contains the id for the contentDataModel we are fetching.
        /// Throws exceptions if it doesn't
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey(NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_CONTENT_ID_KEY))
            {
                throw new Exception("Get content data model request must have the key '"+ NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_CONTENT_ID_KEY+"'");
            }
        }

        /// <summary>
        /// should be called on the inital request instance after calling and awaiting
        /// NusysNetworkSession.ExecuteRequestAsync on the request.  This will get the returned contentDataModel
        /// </summary>
        /// <returns></returns>
        public ContentDataModel GetReturnedContentDataModel()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_RETURNED_CONTENT_DATA_MODEL_KEY));
            var modelString = _returnMessage.GetString(NusysConstants.GET_CONTENT_DATA_MODEL_REQUEST_RETURNED_CONTENT_DATA_MODEL_KEY);
            return ContentDataModelFactory.DeserializeFromString(modelString);
        }
    }
}
