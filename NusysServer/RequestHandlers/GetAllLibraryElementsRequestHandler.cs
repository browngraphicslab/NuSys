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

            //store instance of sql connector
            var sql = ContentController.Instance.SqlConnector;

            //create arguments for selecting all libreary elements
            var libraryArgs = new SqlSelectQueryArgs();
            libraryArgs.ColumnsToGet = NusysConstants.LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS.Keys;
            libraryArgs.TableType = Constants.SQLTableType.LibrayElement;
            var libraryCmdArgs = sql.GetSelectCommand(libraryArgs);
            var elementMessages = sql.ExecuteSelectQueryAsMessages(libraryCmdArgs);

            //after query execution, map all the messages to the libraryId for that message
            var elementMap = new Dictionary<string,Message>();
            foreach (var elementMessage in elementMessages)
            {
                if (elementMessage.ContainsKey(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY))
                {
                    elementMap[elementMessage[NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY].ToString()] = elementMessage;
                }
            }

            //create select query for 
            var propertiesArgs = new SqlSelectQueryArgs();
            propertiesArgs.ColumnsToGet = NusysConstants.ACCEPTED_PROPERTIES_TABLE_KEYS;
            propertiesArgs.TableType = Constants.SQLTableType.Properties;
            propertiesArgs.Condition = new SingleTableWhereCondition(new SqlSelectQueryContains(Constants.SQLTableType.Properties, NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY, new List<string>(elementMap.Keys)));
            var propertiesCommand = sql.GetSelectCommand(propertiesArgs);

            //after execution, map messages back to original mapping
            var propertiesMessages = sql.ExecuteSelectQueryAsMessages(propertiesCommand, false);
            foreach (var property in propertiesMessages)
            {
                var key = property[NusysConstants.PROPERTIES_KEY_COLUMN_KEY].ToString();
                property.Remove(NusysConstants.PROPERTIES_KEY_COLUMN_KEY);

                var libraryId = property[NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY].ToString();
                property.Remove(NusysConstants.PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY);

                if (property.Keys.Any(k => property[k] != null && property[k].ToString() != ""))
                {
                    elementMap[libraryId].Add(key, property[property.Keys.First(k => property[k] != null && property[k].ToString() != "")]);
                }
                else if (property.Keys.Any())//if there is a non-null value left
                {
                    elementMap[libraryId].Add(key, property[property.Keys.First()]);
                }
            }

            var libraryElementModels = new List<string>();
            foreach (var m in elementMap.Values)
            {
                //add to library element models a json-serialzed version of a library element model from the factory
                libraryElementModels.Add(JsonConvert.SerializeObject(LibraryElementModelFactory.CreateFromMessage(m)));
            }
            var returnMessage = new Message();
            returnMessage[NusysConstants.GET_ALL_LIBRARY_ELEMENTS_REQUEST_RETURNED_LIBRARY_ELEMENT_MODELS_KEY] = libraryElementModels;
            return returnMessage;
        }
    }
}