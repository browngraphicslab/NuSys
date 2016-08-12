using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NusysIntermediate;
using NusysServer.Util.SQLQuery;

namespace NusysServer.RequestHandlers
{
    public class UpdatePresentationLinkRequestHandler : RequestHandler
    {
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.CreateNewPresentationLinkRequest);
            var message = GetRequestMessage(request);

            //TODO not make these debug.asserts
            //Check to make sure message has everything we need
            Debug.Assert(message.ContainsKey(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY));
            var linkId = message.GetString(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY);
            var propertiesToUpdate = GetPropertiesToUpdate(message);
            if (!propertiesToUpdate.Any())
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });
            }
            SQLUpdateRowQuery updateQuery = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.PresentationLink), propertiesToUpdate, new SqlQueryEquals(Constants.SQLTableType.PresentationLink, NusysConstants.PRESENTATION_LINKS_TABLE_LINK_ID_KEY, linkId));
            var success = updateQuery.ExecuteCommand();
            if (success)
            {
                ForwardMessage(message, senderHandler);
            }
            var returnMessage = new Message(message);
            returnMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = success;
            return returnMessage;

        }

        /// <summary>
        /// Returns the properties to update in the presentation table based on the message passed in
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        private List<SqlQueryEquals> GetPropertiesToUpdate(Message message)
        {
            List<SqlQueryEquals> propertiesToUpdate = new List<SqlQueryEquals>();
            if (message.ContainsKey(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY))
            {
                propertiesToUpdate.Add(new SqlQueryEquals(Constants.SQLTableType.PresentationLink, NusysConstants.PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY, message.GetString(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY)));
            }
            if (message.ContainsKey(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY))
            {
                propertiesToUpdate.Add(new SqlQueryEquals(Constants.SQLTableType.PresentationLink, NusysConstants.PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY, message.GetString(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY)));
            }
            if (message.ContainsKey(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_ANNOTATION_KEY))
            {
                propertiesToUpdate.Add(new SqlQueryEquals(Constants.SQLTableType.PresentationLink, NusysConstants.PRESENTATION_LINKS_TABLE_ANNOTATION_TEXT_KEY, message.GetString(NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_ANNOTATION_KEY)));
            }
            return propertiesToUpdate;

        }
    }
}