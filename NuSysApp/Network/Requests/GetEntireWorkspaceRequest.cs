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
    public class GetEntireWorkspaceRequest : Request
    {
        public GetEntireWorkspaceRequest(string collectionId) : base(NusysConstants.RequestType.GetEntireWorkspaceRequest)
        {
            _message[NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY] = collectionId;
        }

        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY));
        }

        public GetEntireWorkspaceRequestArgs GetReturnedArgs()
        {
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY));
            try
            {
                var args = _returnMessage.Get<GetEntireWorkspaceRequestArgs>(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY);
                return args;
            }
            catch (JsonException parseException)
            {
                return null;
            }
        }
    }
}
