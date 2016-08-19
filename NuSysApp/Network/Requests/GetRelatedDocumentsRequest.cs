using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class GetRelatedDocumentsRequest : Request
    {
        /// <summary>
        /// This constructor should only be used to create a new request from the message that was returned from the server.
        /// </summary>
        public GetRelatedDocumentsRequest(Message message) : base(NusysConstants.RequestType.GetRelatedDocumentsRequest, message)
        {
        }

        /// <summary>
        /// Prefered constructor When creating new request to send to the server. To use, pass in the contentDataModel id of the document you wish to see related documents for. 
        /// To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call ParseRelatedDocumentsLocally
        /// </summary>
        public GetRelatedDocumentsRequest(string contentDataModelId) : base(NusysConstants.RequestType.GetRelatedDocumentsRequest)
        {
            _message[NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_CONTENT_ID_KEY] = contentDataModelId;
        }

        /// <summary>
        /// This method should do something with the returned related contentDataModel ids. Will throw exception if request has not yet been returned or the request failed
        /// </summary>
        public List<Tuple<string,double>> ParseRelatedDocumentsLocally()
        {
            CheckWasSuccessfull();
            var listOfRelatedDocuments =
                _returnMessage.GetList<Tuple<string, double>>(
                    NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_RETURNED_RELATED_DOCUMENT_CONTENT_IDS_KEY);
            return listOfRelatedDocuments;
            ;
        }

        /// <summary>
        /// Makes sure that the request has the ID of the document to get related documents of
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_CONTENT_ID_KEY));
        }
    }
}