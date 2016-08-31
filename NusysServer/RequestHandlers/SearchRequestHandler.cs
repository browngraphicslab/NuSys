using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the request handler for all search requests
    /// </summary>
    public class SearchRequestHandler : RequestHandler
    {
        /// <summary>
        /// this request handler should only handle the search results and return the result to the original sender.  
        /// The request is not forwarded to any other clients. 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        public override Message HandleRequest(Request request, NuWebSocketHandler senderHandler)
        {
            Debug.Assert(request.GetRequestType() == NusysConstants.RequestType.SearchRequest);
            var message = GetRequestMessage(request);
            if (!message.ContainsKey(NusysConstants.SEARCH_REQUEST_SERIALIZED_QUERY_KEY))
            {
                throw new Exception("No search query was found in the search request.");
            }

            //get the query from the json
            var query = message.Get<QueryArgs>(NusysConstants.SEARCH_REQUEST_SERIALIZED_QUERY_KEY);
            if (query.SearchString.Equals(""))
            {
                message.Add(NusysConstants.REQUEST_SUCCESS_BOOL_KEY, true);
                message[NusysConstants.SEARCH_REQUEST_RETURNED_SEARCH_RESULTS_KEY] = JsonConvert.SerializeObject(new List<SearchResult>());
                return message;
            }
            //todo actually search and return a new search result

            var libraryElementTable = new SingleTable(Constants.SQLTableType.LibraryElement);
            var userColumnsToSelect = new List<string>();
            userColumnsToSelect.Add(NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY);
            userColumnsToSelect.Add(NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY);
            var userTable = new SingleTable(Constants.SQLTableType.Users, Constants.GetFullColumnTitles(Constants.SQLTableType.Users, userColumnsToSelect));
            var args = new SqlJoinOperationArgs();
            args.LeftTable = libraryElementTable;
            args.RightTable = userTable;
            args.JoinOperator = Constants.JoinedType.LeftJoin;
            args.Column1 = Constants.GetFullColumnTitle(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY).First();
            args.Column2 = Constants.GetFullColumnTitle(Constants.SQLTableType.Users, NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY).First();
            var libraryElementsJoinUser = new JoinedTable(args);

            SQLSelectQuery searchQuery = new SQLSelectQuery(libraryElementsJoinUser, GetFullSearchConditional(query));
            var returnedSearch = searchQuery.ExecuteCommand(); 
            var searchResults = new List<SearchResult>();
            foreach (var searchResult in returnedSearch)
            {
                searchResults.Add(new SearchResult(searchResult.GetString(NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY), SearchResult.ResultType.Creator, searchResult.GetString(NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY)));
            }
            
            var returnMessage = new Message();
            returnMessage[NusysConstants.SEARCH_REQUEST_RETURNED_SEARCH_RESULTS_KEY] = JsonConvert.SerializeObject(searchResults);

            return returnMessage;
        }

        /// <summary>
        /// This returns the sql conditional for searching the entire library element table for a list of strings. Returns null if list of strings to search for is null or empty.
        /// </summary>
        /// <param name="listOfStringsToSearchFor"></param>
        /// <returns></returns>
        private SqlQueryConditional GetSearchEntireTableForStringConditional(List<string> listOfStringsToSearchFor)
        {
            if (listOfStringsToSearchFor != null && listOfStringsToSearchFor.Any())
            {
                List<SqlQueryConditional> searchTextConditionalList = new List<SqlQueryConditional>();

                if (listOfStringsToSearchFor.Count == 1)
                {
                    foreach (var column in Constants.GetAcceptedKeys(Constants.SQLTableType.LibraryElement, false))
                    {
                        searchTextConditionalList.Add(new SqlQueryIsSubstring(Constants.SQLTableType.LibraryElement, column,
                            listOfStringsToSearchFor.First()));
                    }
                }
                else
                {
                    foreach (var column in Constants.GetAcceptedKeys(Constants.SQLTableType.LibraryElement, false))
                    {
                        searchTextConditionalList.Add(new SqlQueryContainsSubstring(Constants.SQLTableType.LibraryElement, column,
                            listOfStringsToSearchFor));
                    }
                }
                SqlQueryOperator searchTextConditional = new SqlQueryOperator(searchTextConditionalList,
                    Constants.Operator.Or);
                return searchTextConditional;
            }
            else
            {
                return null;
            }

        }

        /// <summary>
        /// Given the query args, returns the appropriate full search conditional to pass into the select search query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private SqlQueryConditional GetFullSearchConditional(QueryArgs query)
        {
            var completeListOfConditionals = new List<SqlQueryConditional>();

            //set up searchTextConditional
            var completeConditionalsForSearchText = GetSearchEntireTableForStringConditional(query.SearchText);
            if (completeConditionalsForSearchText != null)
            {
                completeListOfConditionals.Add(completeConditionalsForSearchText);
            }

            //create the conditional to search for keywords 
            if (query.Keywords != null && query.Keywords.Any())
            {
                SqlQueryConditional searchKeyWordsConditional;
                if (query.Keywords.Count == 1)
                {
                    searchKeyWordsConditional = new SqlQueryIsSubstring(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY, query.Keywords.First());
                }
                else
                {
                    searchKeyWordsConditional = new SqlQueryContainsSubstring(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY, query.Keywords);
                }
                completeListOfConditionals.Add(searchKeyWordsConditional);
            }

            //create the conditional to search for element types
            if (query.ElementTypes != null && query.ElementTypes.Any())
            {
                SqlQueryConditional searchTypeConditional;
                if (query.ElementTypes.Count == 1)
                {
                    searchTypeConditional = new SqlQueryIsSubstring(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_TYPE_KEY, query.ElementTypes.First());
                }
                else
                {
                    searchTypeConditional = new SqlQueryContainsSubstring(Constants.SQLTableType.LibraryElement, NusysConstants.LIBRARY_ELEMENT_TYPE_KEY, query.ElementTypes);
                }

                completeListOfConditionals.Add(searchTypeConditional);
            }

            //create the conditional to search for creator ids
            if (query.CreatorUserIds != null && query.CreatorUserIds.Any())
            {
                SqlQueryConditional searchCreatorsConditional;
                if (query.CreatorUserIds.Count == 1)
                {
                    searchCreatorsConditional = new SqlQueryIsSubstring(Constants.SQLTableType.Users, NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY, query.CreatorUserIds.First());

                }
                else
                {
                    searchCreatorsConditional = new SqlQueryContainsSubstring(Constants.SQLTableType.Users, NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY, query.CreatorUserIds);

                }
                completeListOfConditionals.Add(searchCreatorsConditional);
            }

            SqlQueryConditional fullSearchConditional;
            //If there are no conditionals, just search the entire table for the search string
            if (!completeListOfConditionals.Any() && !query.SearchString.Equals(""))
            {
                fullSearchConditional = GetSearchEntireTableForStringConditional(new List<string>() { query.SearchString });
            }
            else if (completeListOfConditionals.Count > 1)
            {
                fullSearchConditional = new SqlQueryOperator(completeListOfConditionals,
                   Constants.Operator.And);
            }
            else
            {
                fullSearchConditional = completeListOfConditionals.First();
            }
            return fullSearchConditional;
        }
    }
}