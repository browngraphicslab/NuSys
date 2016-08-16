using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NusysIntermediate;
using NusysServer.RequestHandlers;

namespace NusysServer
{
    public class RequestRouter
    {
        /// <summary>
        /// this class will take in a message from a request and route it to the  correct request handler.
        /// All requests should go through this router.
        /// This class also checks to see if the client is awaiting a response.  If so, it ALWAYS returns a message back indicating a succesful (or not) request from that client.
        /// </summary>
        /// <param name="originalMessage"></param>
        /// <param name="webSocketHandler"></param>
        /// <returns></returns>
        public static async Task<bool> HandleRequest(Message originalMessage, NuWebSocketHandler webSocketHandler)
        {
            try
            {
                var requiresReturn = originalMessage.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
                RequestHandler requestHandler;
                Request request = new Request(originalMessage);
                Message messageToReturn;

                try
                {
                    switch (request.GetRequestType())
                    {
                    case NusysConstants.RequestType.GetAnalysisModelRequest:
                        requestHandler = new GetAnalysisModelRequestHandler();
                        break;
                    case NusysConstants.RequestType.ChatRequest:
                        requestHandler = new ChatRequestHandler();
                        break;
                    case NusysConstants.RequestType.SearchRequest:
                        requestHandler = new SearchRequestHandler();
                        break;
                    case NusysConstants.RequestType.DeleteLibraryElementRequest:
                        requestHandler = new DeleteLibraryElementRequestHandler();
                        break;
                    case NusysConstants.RequestType.DeleteElementRequest:
                        requestHandler = new DeleteElementRequestHandler();
                        break;
                    case NusysConstants.RequestType.ElementUpdateRequest:
                        requestHandler = new ElementUpdateRequestHandler();
                        break;
                    case NusysConstants.RequestType.NewElementRequest:
                        requestHandler = new CreateNewElementRequestHandler();
                        break;
                    case NusysConstants.RequestType.GetContentDataModelRequest:
                        requestHandler = new GetContentDataModelRequestHandler();
                        break;
                    case NusysConstants.RequestType.CreateNewLibraryElementRequest:
                        requestHandler = new CreateNewLibraryElementRequestHandler();
                        break;
                    case NusysConstants.RequestType.GetEntireWorkspaceRequest:
                        requestHandler = new GetEntireWorkspaceRequestHandler();
                        break;
                    case NusysConstants.RequestType.CreateNewContentRequest:
                        requestHandler = new CreateNewContentRequestHandler();
                        break;
                    case NusysConstants.RequestType.GetAllLibraryElementsRequest:
                        requestHandler = new GetAllLibraryElementsRequestHandler();
                        break;
                    case NusysConstants.RequestType.UpdateLibraryElementModelRequest:
                        requestHandler = new UpdateLibraryElementRequestHandler();
                        break;
                    case NusysConstants.RequestType.CreateNewPresentationLinkRequest:
                        requestHandler = new CreateNewPresentationLinkRequestHandler();
                        break;
                    case NusysConstants.RequestType.DeletePresentationLinkRequest:
                        requestHandler = new DeletePresentationLinkRequestHandler();
                        break;
                    case NusysConstants.RequestType.UpdatePresentationLinkRequest:
                        requestHandler = new UpdatePresentationLinkRequestHandler();
                            break;
                    case NusysConstants.RequestType.CreateNewMetadataRequest:
                        requestHandler = new CreateNewMetadataRequestHandler();
                        break;
                    case NusysConstants.RequestType.DeleteMetadataRequest:
                        requestHandler = new DeleteMetadataRequestHandler();
                        break;
                    case NusysConstants.RequestType.UpdateMetadataEntryRequest:
                        requestHandler = new UpdateMetadataRequestHandler();
                        break;
                    default:
                        requestHandler = null;
                        return false;
                    }
                    messageToReturn = requestHandler.HandleRequest(request, webSocketHandler) ?? new Message();
                }
                catch (Exception e)
                {
                    var errorMessage = new Message();
                    errorMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = false;
                    errorMessage[NusysConstants.REQUEST_ERROR_MESSAGE_KEY] = e.Message;
                    if (originalMessage.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING))
                    {
                        errorMessage[NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING] = originalMessage[NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING];
                    }
                    webSocketHandler.Send(errorMessage.GetSerialized());
                    ErrorLog.AddError(e);
                    return false;
                }

                //if a return is required but we don't send a message, the client will have a thread waiting forever
                if (requiresReturn)
                {
                    //defaults to returning successful request if the individual handler hasn't specified it 
                    if (!messageToReturn.ContainsKey(NusysConstants.REQUEST_SUCCESS_BOOL_KEY))
                    {
                        messageToReturn[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = true;
                    }
                    messageToReturn[NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING] = originalMessage[NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING];
                    var serialized = messageToReturn.GetSerialized();
                    webSocketHandler.Send(serialized);
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorLog.AddError(e);
                return false;
            }
        }
    }
}