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
    /// returns the contents and aliases needed to load an entire workspace and two levels down of emebeded collections
    /// </summary>
    public class GetEntireWorkspaceRequest : Request
    {
        private List<ContentDataModel> _returnedContentDataModels;
        private List<ElementModel> _returnedElementModels;
        public GetEntireWorkspaceRequest(string collectionId) : base(NusysConstants.RequestType.GetEntireWorkspaceRequest)
        {
            _message[NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY] = collectionId;
        }


        //just checks to see if the message contains an id to request
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY));
        }

        /// <summary>
        /// public method to return the list of content Data Models parsed from the returned Request.  
        /// </summary>
        /// <returns></returns>
        public List<ContentDataModel> GetReturnedContentDataModels()
        {
            if (_returnedContentDataModels == null || _returnedElementModels == null)
            {
                GetReturnedArgs();
            }
            return _returnedContentDataModels;
        }

        /// <summary>
        /// public method to return the list of element models parse from the returned Request.  
        /// </summary>
        /// <returns></returns>
        public List<ElementModel> GetReturnedElementModels()
        {
            if (_returnedContentDataModels == null || _returnedElementModels == null)
            {
                GetReturnedArgs();
            }
            return _returnedElementModels;
        }


        /// <summary>
        /// private method to parse the returned args strings and save them as lists in private variables.
        /// Use the public methods to actually fetch the parts
        /// </summary>
        private void GetReturnedArgs()
        {
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY));
            try
            {
                var args = _returnMessage.Get<GetEntireWorkspaceRequestArgs>(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY);

                //create the aliases from the returned args strings
                _returnedElementModels = new List<ElementModel>();
                foreach (var elementString in args.AliasStrings)
                {
                    _returnedElementModels.Add(ElementModelFactory.DeserializeFromString(elementString));
                }

                //create the content data models from the returned args strings
                _returnedContentDataModels = new List<ContentDataModel>();
                foreach (var contentString in args.ContentMessages)
                {
                    _returnedContentDataModels.Add(ContentDataModelFactory.DeserializeFromString(contentString));
                }

            }
            catch (JsonException parseException)
            {
                Debug.Fail("Shouldn't have failed the parse!");
                _returnedElementModels = new List<ElementModel>();
                _returnedContentDataModels = new List<ContentDataModel>();
            }
        }
    }
}
