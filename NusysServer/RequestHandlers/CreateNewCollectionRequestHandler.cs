using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// this request handler is supposed to call the handler for creating a new content data model.
    /// Then it will call the handlers for creating individual elements in the collection.
    /// </summary>
    public class CreateNewCollectionRequestHandler : RequestHandler
    {
        /// <summary>
        /// this handler will not forward on any messages since the CreateNewLibraryElement handler will take care of forwarding its request.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            var args = GetRequestArgs<CreateNewCollectionServerRequestArgs>(request); //get the args for this request type;

            var contentMessage = new Message(args.CreateNewContentRequestDictionary);
            var aliasMessages = args.NewElementRequestDictionaries.Select(dict => new Message(dict));

            var contentHandler = new CreateNewContentRequestHandler();
            var contentHandledMessage = contentHandler.HandleRequest(new Request(new Request(NusysConstants.RequestType.CreateNewContentRequest, contentMessage).GetFinalMessage()), senderHandler);

            if (!contentHandledMessage.GetBool(NusysConstants.REQUEST_SUCCESS_BOOL_KEY)) //if content request was not successfull
            {
                return new Message(new Dictionary<string, object>() { { NusysConstants.REQUEST_SUCCESS_BOOL_KEY, false } });//return that is failed
            }

            var aliasHandler = new CreateNewElementRequestHandler();
            foreach (var aliasMessage in aliasMessages)
            {
                aliasHandler.HandleRequest(new Request(new Request(NusysConstants.RequestType.NewElementRequest, aliasMessage).GetFinalMessage()), senderHandler);
            }
            return contentHandledMessage; //successful
        }
    }
}