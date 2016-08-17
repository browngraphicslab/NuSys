using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp.Network.Requests
{
    class CreateSnapshotOfCollectionRequest: Request
    {
        /// <summary>
        /// This is the constructor that should be used to create a request when the server sends the client a message
        /// </summary>
        /// <param name="message"></param>
        public CreateSnapshotOfCollectionRequest(Message message) : base(NusysConstants.RequestType.CreateSnapshotOfCollectionRequest, message)
        {
        }

        /// <summary>
        /// Prefered constructor When creating new request to send to the server.
        ///  To use this request await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request) then call CreateSnapshotLocally
        /// </summary>
        public CreateSnapshotOfCollectionRequest(string collectionIdToDuplicate) : base(NusysConstants.RequestType.CreateSnapshotOfCollectionRequest)
        {
            _message[NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_COLLECTION_ID] = collectionIdToDuplicate;
        }

        /// <summary>
        /// just checks to see if the message contains the necessary keys
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_COLLECTION_ID));
        }

        /// <summary>
        /// This is called when the server sends a message to the client (except the client who initially created the request). It should create a new collection locally
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            //make sure the key for the json is present
            Debug.Assert(_message.ContainsKey(NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_RETURNED_COLLECTION_LIBRARY_ELEMENT_MODEL));

            //get the json and add it to the session
            CollectionLibraryElementModel model = JsonConvert.DeserializeObject<CollectionLibraryElementModel>(_message.GetString(NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_RETURNED_COLLECTION_LIBRARY_ELEMENT_MODEL));
            SessionController.Instance.ContentController.Add(model);
        }
        
        /// <summary>
        /// Call this after you await executing this request. This should add the collection locally
        /// </summary>
        public void AddSnapshotCollectionLocally()
        {
            //make sure the key for the json is present
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_RETURNED_COLLECTION_LIBRARY_ELEMENT_MODEL));
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }
            //get the json and add it to the session
            var model = LibraryElementModelFactory.DeserializeFromString(_returnMessage.GetString(NusysConstants.CREATE_SNAPSHOT_OF_COLLECTION_REQUEST_RETURNED_COLLECTION_LIBRARY_ELEMENT_MODEL));

            SessionController.Instance.ContentController.Add(model);

        }
    }
}
