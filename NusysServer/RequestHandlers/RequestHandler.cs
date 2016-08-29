using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NusysServer
{
    public abstract class RequestHandler
    {
        /// <summary>
        /// the method that is called by the request router to actually handle an incoming request.  
        /// The senderHandler is the WebSocketHandler of the original sender.
        /// When returning from this request, you must add to the message that the request has failed, otherwise it will default to being successful.
        /// To add a failure indicator to the reutnred message, add to the message the key value pair: {NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false}
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Message HandleRequest(Request request, NuWebSocketHandler senderHandler);

        /// <summary>
        /// method used to get the message from a request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        protected Message GetRequestMessage(Request request)
        {
            var message = request.GetMessage();
            return message; 
        }

        /// <summary>
        /// a protected method used to forward messages to everyone else from a requst handler.  
        /// Will automatically remove the id from the message that indicates an awaiting thread on the client side.
        /// WILL NOT MODIFY THE MESSAGE INSTANCE PASSED IN.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="senderHandlerToIgnore"></param>
        protected void ForwardMessage(Message messageToForward, NuWebSocketHandler senderHandlerToIgnore)
        {
            var forwardMessage = new Message(messageToForward);
            forwardMessage.Remove(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
            NuWebSocketHandler.BroadcastToSubset(forwardMessage, new HashSet<NuWebSocketHandler>() { senderHandlerToIgnore });
        }

        /// <summary>
        /// a protected method used to update a library element's last edited time stamp to the current time.
        /// </summary>
        /// <param name="libraryElementId"></param>
        /// <returns></returns>
        protected bool UpdateLibraryElementLastEditedTimeStamp(string libraryElementId)
        {
            Debug.Assert(libraryElementId != null);
            if(libraryElementId == null)
            {
                return false;
            }
            List<SqlQueryEquals> updateTimeStamp = new List<SqlQueryEquals>();
            updateTimeStamp.Add(new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY, DateTime.UtcNow.ToString()));
            var conditional = new SqlQueryEquals(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY, libraryElementId);
            SQLUpdateRowQuery updateLastEditedTimeStampQuery = new SQLUpdateRowQuery(new SingleTable(Constants.SQLTableType.LibraryElement), updateTimeStamp, conditional);
            return updateLastEditedTimeStampQuery.ExecuteCommand();
        }

        /// <summary>
        /// This makes a request to the sql table that returns the parent collection of a specific alias.
        /// </summary>
        /// <param name="aliasId"></param>
        /// <returns></returns>
        protected string GetParentCollectionIdOfAlias(string aliasId)
        {
            //Get the id of the collection that the element belongs to.
            //This is used to update the last edited time stamp of the collection.
            if(aliasId == null)
            {
                throw new Exception("alias id is null when trying to get the parent collection of an alias in the request handler super class");
            }
            var listColumnsToSelect = Constants.GetFullColumnTitle(Constants.SQLTableType.Alias,  NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY);
            var selectParentCollectionIdQuery = new SQLSelectQuery(new SingleTable(Constants.SQLTableType.Alias, listColumnsToSelect), new SqlQueryEquals(Constants.SQLTableType.Alias, NusysConstants.ALIAS_ID_KEY, aliasId));
            return selectParentCollectionIdQuery.ExecuteCommand().First().GetString(NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY);
        }
    }
}
