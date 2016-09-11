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
        private List<PresentationLinkModel> _returnedPresentationLinkModels;
        private List<InkModel> _returnedInkModels;

        /// <summary>
        /// this is the preferred constructor.  It takes in a LibaryElementId of the collection you want to fetch, and the levels of recursion you want.
        /// The default levels of recursion is 2.
        /// </summary>
        /// <param name="collectionId"></param>
        public GetEntireWorkspaceRequest(string collectionId, int levelsOfRecursion = 2) : base(NusysConstants.RequestType.GetEntireWorkspaceRequest)
        {
            _message[NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY] = collectionId;
            _message[NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_LEVELS_OF_RECURSION] = levelsOfRecursion;
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
            if (_returnedContentDataModels == null || _returnedElementModels == null || _returnedPresentationLinkModels == null || _returnedInkModels == null)
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
            if (_returnedContentDataModels == null || _returnedElementModels == null || _returnedPresentationLinkModels == null || _returnedInkModels == null)
            {
                GetReturnedArgs();
            }
            return _returnedElementModels;
        }

        /// <summary>
        /// public method to return the list of presentation links models parsed from the returned Request.  
        /// </summary>
        /// <returns></returns>
        public List<PresentationLinkModel> GetReturnedPresentationLinkModels()
        {
            if (_returnedContentDataModels == null || _returnedElementModels == null || _returnedPresentationLinkModels == null || _returnedInkModels == null)
            {
                GetReturnedArgs();
            }
            return _returnedPresentationLinkModels;
        }

        /// <summary>
        /// method to get the returned ink models after a successful request.
        /// </summary>
        /// <returns></returns>
        public List<InkModel> GetReturnedInkModels()
        {
            if (_returnedContentDataModels == null || _returnedElementModels == null || _returnedPresentationLinkModels == null || _returnedInkModels == null)
            {
                GetReturnedArgs();
            }
            return _returnedInkModels;
        }


        /// <summary>
        /// private method to parse the returned args strings and save them as lists in private variables.
        /// Use the public methods to actually fetch the parts
        /// </summary>
        private void GetReturnedArgs()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY));
            try
            {
                var args = _returnMessage.Get<GetEntireWorkspaceRequestReturnArgs>(NusysConstants.GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY);

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

                //create the presentation links
                _returnedPresentationLinkModels = new List<PresentationLinkModel>();
                foreach (var presentationLink in args.PresentationLinks)
                {
                    _returnedPresentationLinkModels.Add(JsonConvert.DeserializeObject<PresentationLinkModel>(presentationLink));
                }

                //create the ink models
                _returnedInkModels = new List<InkModel>();
                foreach (var ink in args.InkStrokes)
                {
                    _returnedInkModels.Add(JsonConvert.DeserializeObject<InkModel>(ink));
                }

            }
            catch (JsonException parseException)
            {
                Debug.Fail("Shouldn't have failed the parse!");
                _returnedElementModels = new List<ElementModel>();
                _returnedContentDataModels = new List<ContentDataModel>();
                _returnedPresentationLinkModels = new List<PresentationLinkModel>();
                _returnedInkModels = new List<InkModel>();
            }
        }

        /// <summary>
        /// this method performs the common functions on all the returned elements.  
        /// THis method will add the returned elements to the correct collections and will add all the new contentDataModels it needs to.
        /// Most times you use this request, you should call this method
        /// </summary>
        /// <returns></returns>
        public async Task AddReturnedDataToSessionAsync()
        {
            //get the contents and elements
            var contentDataModels = GetReturnedContentDataModels();
            var presentationLinks = GetReturnedPresentationLinkModels();

            //for each contentDataModel, add it to the contentController if it doesn't exist
            foreach (var content in contentDataModels)
            {
                if (!SessionController.Instance.ContentController.ContainsContentDataModel(content.ContentId))
                {
                    SessionController.Instance.ContentController.AddContentDataModel(content);
                }
            }

            foreach (var presentationLink in presentationLinks)//add the presentation links
            {
                await SessionController.Instance.LinksController.AddPresentationLinkToLibrary(presentationLink);
            }
        }

        /// <summary>
        /// this method will make a collection out of the returned elements.  
        /// This can be called in conjunction with the AddReturnedDataToSessionAsync() method
        /// </summary>
        /// <returns></returns>
        public async Task MakeCollectionFromReturnedElementsAsync()
        {
            var elements = GetReturnedElementModels();
            if (SessionController.Instance.SessionView != null)
            {
                await SessionController.Instance.SessionView.MakeCollection(elements.ToDictionary(e => e.Id, e => e));
            }
        }
    }
}
