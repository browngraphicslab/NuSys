using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// This is the request hadnler for getting analysis models of ContentDataModels.
    /// This handler will simply get the Analysis model from the database and return it
    /// </summary>
    public class GetAnalysisModelRequest : RequestHandler
    {
        /// <summary>
        /// this handler implementation will extract from the request the id of the analysis model that was requested.  
        /// Then it will fetch the required json model and return it.  Returns null as the json if the requested Id is not found
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetAnalysisModelRequest);
            var message = GetRequestMessage(request);

            //making sure the id exists in the requst message
            if (!message.ContainsKey(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_ID))
            {
                throw new Exception("No id was found for the getAnalysisModel request.");
            }
            var id = message.GetString(NusysConstants.GET_ANALYSIS_MODEL_REQUEST_CONTENT_DATA_MODEL_ID);

            //construct the select query
            var query = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.AnalysisModels),new SqlQueryEquals(Constants.SQLTableType.AnalysisModels, NusysConstants.ANALYIS_MODELS_TABLE_CONTENT_ID_KEY,id));

            var returnedMessages = query.ExecuteCommand();

            //if the json was not found
            if (!returnedMessages.Any())
            {
                //return it as null
                return new Message() { {NusysConstants.GET_ANALYSIS_MODEL_REQUEST_RETURNED_ANALYSIS_MODEL_JSON, null} };
            }

            return new Message() { {NusysConstants.GET_ANALYSIS_MODEL_REQUEST_RETURNED_ANALYSIS_MODEL_JSON, returnedMessages.First().GetString(NusysConstants.ANALYIS_MODELS_TABLE_CONTENT_ID_KEY)}};
        }
    }
}