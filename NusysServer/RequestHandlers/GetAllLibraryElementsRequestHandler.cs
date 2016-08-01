using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    public class GetAllLibraryElementsRequestHandler : RequestHandler
    {
        /// <summary>
        /// simply adds an IEnumerable of LibraryElementModels to the returned message of this inheritted method
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.GetAllLibraryElementsRequest);
            var message = GetRequestMessage(request);
            var libraryArgs = new SqlSelectQueryArgs();
            libraryArgs.ColumnsToGet = NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS.Keys;
            libraryArgs.TableType = Constants.SQLTableType.LibrayElement;
            var libraryCmdArgs = ContentController.Instance.SqlConnector.GetSelectCommand(libraryArgs);
            var elementMessages = ContentController.Instance.SqlConnector.ExecuteSelectQueryAsMessages(libraryCmdArgs);
            
            var libraryElementModels = new List<string>();
            foreach (var m in elementMessages)
            {
                libraryElementModels.Add(JsonConvert.SerializeObject(LibraryElementModelFactory.CreateFromMessage(m)));
            }
            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_ALL_LIBRARY_ELEMENTS_REQUEST_RETURNED_LIBRARY_ELEMENT_MODELS_KEY] = libraryElementModels;
            return returnMessage;
        }
    }
}