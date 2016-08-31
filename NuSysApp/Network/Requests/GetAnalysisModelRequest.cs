using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;
using Newtonsoft.Json;

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
        /// preferred constructor.  Takes in the ContentDataModel Ids of the ContentDataModels we are fetching this AnalysisModels of.
        /// After calling this constructor, await the execution of ths request and then call GetReturnedAnalysisModel.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        public GetAnalysisModelRequest(List<string> contentDataModelIds) : base(NusysConstants.RequestType.GetAnalysisModelRequest)
        {
            Debug.Assert(contentDataModelIds != null && contentDataModelIds.Any());
            _message[NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_IDS] = JsonConvert.SerializeObject(contentDataModelIds);
        }

        /// <summary>
        /// preferred constructor.  Takes in the ContentDataModel Id of the ContentDataModel we are fetching this AnalysisModel of.
        /// After calling this constructor, await the execution of ths request and then call GetReturnedAnalysisModel.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        public GetAnalysisModelRequest(string contentDataModelId) : base(NusysConstants.RequestType.GetAnalysisModelRequest)
        {
            Debug.Assert(!string.IsNullOrEmpty(contentDataModelId));
            _message[NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_IDS] = JsonConvert.SerializeObject(new List<string>() { contentDataModelId });
        }

        /// <summary>
        /// just checks to make sure the ID is present in this overriden CheckOutgoingRequest method
        /// </summary>
        /// <returns></returns>
        public override async Task CheckOutgoingRequest()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_IDS));
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
        public IEnumerable<AnalysisModel> GetReturnedAnalysisModel()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_RETURNED_ANALYSIS_MODEL_JSONS));

            //get the json
            List<string> analysisJsonList;
            if (_returnMessage.Get(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_RETURNED_ANALYSIS_MODEL_JSONS) == null)
            {
                analysisJsonList = new List<string>();
            }
            else
            {
                analysisJsonList = _returnMessage.GetList<string>(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_RETURNED_ANALYSIS_MODEL_JSONS);
            }

            return analysisJsonList?.Select(json => AnalysisModelFactory.DeserializeFromString(json));
        }
    }
}
