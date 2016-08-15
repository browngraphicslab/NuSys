using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// this class will be used to fetch the contentDataAnalysis models from the server. 
    /// You simply have to instantiate this request with a ContentDataModelId, await the execution of this request, 
    /// and then call GetReturnedAnalysisModel to actually get back the AnalysisModel.
    /// </summary>
    public class GetAnalysisModelRequest : Request
    {
        /// <summary>
        /// preferred constructor.  Takes in the ContentDataModel Id of the ContentDataModel we are fetching this AnalysisModel of.
        /// After calling this constructor, await the execution of ths request and then call GetReturnedAnalysisModel.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        public GetAnalysisModelRequest(string contentDataModelId) : base(NusysConstants.RequestType.GetAnalysisModelRequest)
        {
            Debug.Assert(!string.IsNullOrEmpty(contentDataModelId));
            _message[NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_ID] = contentDataModelId;
        }

        /// <summary>
        /// just checks to make sure the ID is present in this overriden CheckOutgoingRequest method
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_ID));
        }

        /// <summary>
        /// this method returns the AnalysisModel after this reuqest has been successfully awaited.  
        /// This request will return to you the requested AnalysisModel.
        /// This will return null if no analysisModel was found.
        /// 
        /// This method will return the base AnalysisModel type, so you should cast to the needed type for your content type.
        /// 
        /// This method will throw an exception if the request hasn't return yet or wasn't successful.
        /// 
        /// This method also requires that the contentDataModel for the requested analysisModel exist locally.  
        /// 
        /// </summary>
        /// <returns></returns>
        public AnalysisModel GetReturnedAnalysisModel()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_RETURNED_ANALYSIS_MODEL_JSON));

            //get the json
            var analysisJson = _returnMessage.GetString(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_RETURNED_ANALYSIS_MODEL_JSON);

            //returns null if the json is null, aka no mdel was found on the server
            if (analysisJson == null)
            {
                return null;
            }


            //get the contentDataModel
            var contentDataModel = SessionController.Instance.ContentController.GetContentDataModel(_message.GetString(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_ID));
            if (contentDataModel == null)
            {
                throw new Exception("The contentDataModel for the requested Analysis Model was null locally.");
            }

            //return the analysis model deserialized to the correct type
            return AnalysisModelFactory.DeserializeFromString(analysisJson, contentDataModel.ContentType);
        }
    }
}
