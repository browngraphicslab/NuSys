using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using NusysConstants;

namespace NusysServer
{
    public class RequestRouter
    {
        public static async Task<bool> HandleRequest(Message m, NuWebSocketHandler handler)
        {
            try
            {
                Debug.Assert(m.ContainsKey(Constants.REQUEST_TYPE_STRING));
                var type = (ServerConstants.RequestType)Enum.Parse(typeof(ServerConstants.RequestType), m.GetString(Constants.REQUEST_TYPE_STRING), true);
                switch (type)
                {
                    case ServerConstants.RequestType.DeleteLibraryElementRequest:
                        break;
                    case ServerConstants.RequestType.AddInkRequest:
                        break;
                    case ServerConstants.RequestType.ChangeContentRequest:
                        break;
                    case ServerConstants.RequestType.ChatDialogRequest:
                        break;
                    case ServerConstants.RequestType.CreateNewLibrayElementRequest:
                        break;
                    case ServerConstants.RequestType.SubscribeToCollectionRequest:
                        break;
                    case ServerConstants.RequestType.DuplicateNodeRequest:
                        break;
                    case ServerConstants.RequestType.FinalizeInkRequest:
                        break;
                    case ServerConstants.RequestType.NewContentRequest:
                        break;
                    case ServerConstants.RequestType.NewLinkRequest:
                        break;
                    case ServerConstants.RequestType.NewNodeRequest:
                        break;
                    case ServerConstants.RequestType.NewThumbnailRequest:
                        break;
                    case ServerConstants.RequestType.UnsubscribeFromCollectionRequest:
                        break;
                    case ServerConstants.RequestType.SetTagsRequest:
                        break;
                    case ServerConstants.RequestType.DeleteSendableRequest:
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