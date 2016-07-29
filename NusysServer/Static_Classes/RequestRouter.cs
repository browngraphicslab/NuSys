using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    public class RequestRouter
    {
        public static async Task<bool> HandleRequest(Message originalMessage, NuWebSocketHandler webSocketHandler)
        {
            try
            {
                var requiresReturn = originalMessage.ContainsKey(NusysConstants.RETURN_AWAITABLE_REQUEST_ID_STRING);
                RequestHandler requestHandler;
                Request request = new Request(originalMessage);

                try
                {
                    switch (request.GetRequestType())
                    {
/*
                    case NusysConstants.RequestType.DeleteLibraryElementRequest:
                        break;
                    case NusysConstants.RequestType.AddInkRequest:
                        break;
                    case NusysConstants.RequestType.ChangeContentRequest:
                        break;
                    case NusysConstants.RequestType.ChatDialogRequest:
                        break;
                    case NusysConstants.RequestType.SubscribeToCollectionRequest:
                        break;
                    case NusysConstants.RequestType.DuplicateNodeRequest:
                        break;
                    case NusysConstants.RequestType.FinalizeInkRequest:
                        break;
                    case NusysConstants.RequestType.NewContentRequest:
                        break;
                    case NusysConstants.RequestType.NewLinkRequest:
                        break;
                    case NusysConstants.RequestType.NewNodeRequest:
                        break;
                    case NusysConstants.RequestType.NewThumbnailRequest:
                        break;
                    case NusysConstants.RequestType.UnsubscribeFromCollectionRequest:
                        break;
                    case NusysConstants.RequestType.SetTagsRequest:
                        break;
                    case NusysConstants.RequestType.DeleteSendableRequest:
                        break;*/
                    case NusysConstants.RequestType.CreateNewLibrayElementRequest:
                        requestHandler = null;
                        break;
                    case NusysConstants.RequestType.GetEntireWorkspaceRequest:
                        requestHandler = new GetEntireWorkspaceRequestHandler();
                        break;
                    case NusysConstants.RequestType.NewContentRequest:
                        requestHandler = new CreateNewContentRequestHander();
                        break;
                    default:
                        requestHandler = null;
                        return false;
                    }
                }
                catch (Exception e)
                {
                    var errorMessage = new Message();
                    errorMessage[NusysConstants.REQUEST_SUCCESS_BOOL_KEY] = false;
                    errorMessage[NusysConstants.REQUEST_ERROR_MESSAGE_KEY] = e.Message;
                    webSocketHandler.Send(errorMessage.GetSerialized());
                    ErrorLog.AddError(e);
                    return false;
                }

                var messageToReturn = requestHandler.HandleRequest(request, webSocketHandler) ?? new Message();
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