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

            //Joins alias and library element tables where alias.libraryelementid = libraryelement.libraryelementid
            SqlJoinOperationArgs libraryElementJoinProperties = new SqlJoinOperationArgs();
            libraryElementJoinProperties.LeftTable = new SingleTable(Constants.SQLTableType.LibraryElement);
            libraryElementJoinProperties.RightTable = new SingleTable(Constants.SQLTableType.Properties);
            libraryElementJoinProperties.JoinOperator = Constants.JoinedType.LeftJoin;
            libraryElementJoinProperties.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement,
                NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY).First();
            libraryElementJoinProperties.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Properties,
                NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY).First();
            JoinedTable aliasJoinLibraryElement = new JoinedTable(libraryElementJoinProperties);
            //creates a list of all columns from libraryelement, and properties tables
            
            //var columnsToGet =
            //     new List<string>(
            //    Constants.GetAcceptedKeys(Constants.SQLTableType.LibraryElement)
            //    .Concat(
            //        Constants.GetAcceptedKeys(Constants.SQLTableType.Properties)));
            var query = new SQLSelectQuery(aliasJoinLibraryElement);

            var libraryElementReturnedMessages = query.ExecuteCommand();
            PropertiesParser propertiesParser = new PropertiesParser();
            var libraryElementConcatPropertiesMessages = propertiesParser.ConcatMessageProperties(libraryElementReturnedMessages);
            var libraryElementModels = new List<string>();
            foreach (var m in libraryElementConcatPropertiesMessages)
            {
                //add to library element models a json-serialzed version of a library element model from the factory
                libraryElementModels.Add(JsonConvert.SerializeObject(LibraryElementModelFactory.CreateFromMessage(Constants.StripTableNames(m))));
            }
            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_ALL_LIBRARY_ELEMENTS_REQUEST_RETURNED_LIBRARY_ELEMENT_MODELS_KEY] = libraryElementModels;
            return returnMessage;
        }


    }
}