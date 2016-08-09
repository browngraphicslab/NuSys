﻿using System;
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
        /// <summary>
        /// this requst will 
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
                        */
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