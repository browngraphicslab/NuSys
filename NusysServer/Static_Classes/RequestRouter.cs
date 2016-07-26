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
        public static async Task<bool> HandleRequest(Message m, NuWebSocketHandler handler)
        {
            try
            {
                Request request = new Request(m);
                switch (request.GetRequestType())
                {
                    case NusysConstants.RequestType.DeleteLibraryElementRequest:
                        break;
                    case NusysConstants.RequestType.AddInkRequest:
                        break;
                    case NusysConstants.RequestType.ChangeContentRequest:
                        break;
                    case NusysConstants.RequestType.ChatDialogRequest:
                        break;
                    case NusysConstants.RequestType.CreateNewLibrayElementRequest:
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
                        break;
                    default:
                        return false;
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