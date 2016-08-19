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
        /// Prefered constructor When creating new request to send to the server. To use, pass in the library id of the document you wish to see related documents for. 
        /// To use this request, await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call ParseRelatedDocumentsLocally
        /// </summary>
        public GetRelatedDocumentsRequest(string libraryId) : base(NusysConstants.RequestType.GetRelatedDocumentsRequest)
        {
            _message[NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_LIBRARY_ID_KEY] = libraryId;
        }

        /// <summary>
        /// This method should do something with the returned related ids. Will throw exception if request has not yet been returned or the request failed
        /// </summary>
        public void ParseRelatedDocumentsLocally()
        {
            CheckWasSuccessfull();
            List<string> libraryIdsOfRelatedDocuments =
                _returnMessage.GetList<string>(
                    NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_RETURNED_RELATED_DOCUMENT_LIBRARY_IDS_KEY);
            //TODO: DO SOMETHING WITH THE RETURNED IDS
        }

        /// <summary>
        /// Makes sure that the request has the ID of the document to get related documents of
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_LIBRARY_ID_KEY));
        }

        /// <summary>
        /// This should be called when the server forwards the request to the client (except the client who initially created the request). 
        /// It should do something with the related documents
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_RETURNED_RELATED_DOCUMENT_LIBRARY_IDS_KEY));
            List<string> libraryIdsOfRelatedDocuments =
                _message.GetList<string>(
                    NusysConstants.GET_RELATED_DOCUMENTS_REQUEST_RETURNED_RELATED_DOCUMENT_LIBRARY_IDS_KEY);
            //TODO: DO SOMETHING WITH THE RETURNED IDS
        }

    }
}