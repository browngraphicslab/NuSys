using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// used like a constants static method class.  
    /// All the classes should be of the same format,
    /// they should take in a message of request keys and then return a message where each of its keys are replaced with the database equivalent.  
    /// For any key in the original message that cannot be mapped, the key-value pair will be removed from the returned message
    /// </summary>
    public class RequestToSqlKeyMappings
    {
        /// <summary>
        /// the mapping dicitonary of request key to database key
        /// </summary>
        public static readonly Dictionary<string, string> LibraryElementMapping = new BiDictionary<string, string>()
        {
            //base types start
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY, NusysConstants.LIBRARY_ELEMENT_CONTENT_ID_KEY },//content Id
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY, NusysConstants.LIBRARY_ELEMENT_LIBRARY_ID_KEY },//library Id
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY, NusysConstants.LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY },//creation timestamp
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_KEYWORDS_KEY, NusysConstants.LIBRARY_ELEMENT_KEYWORDS_KEY },  //keywords
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY, NusysConstants.LIBRARY_ELEMENT_CREATOR_USER_ID_KEY }, // Creator user id
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_FAVORITED_KEY, NusysConstants.LIBRARY_ELEMENT_FAVORITED_KEY },//favorited
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LAST_EDITED_TIMESTAMP_KEY, NusysConstants.LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY }, // last edited
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY, NusysConstants.LIBRARY_ELEMENT_TYPE_KEY }, // type
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY, NusysConstants.LIBRARY_ELEMENT_TITLE_KEY }, // title

            //regions start
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_CLIPPING_PARENT_ID, NusysConstants.REGION_CLIPPING_PARENT_ID_KEY },//clipping parent id
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_PDF_PAGE_LOCATION, NusysConstants.PDF_REGION_PAGE_NUMBER_KEY },//pdf page number
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_TOP_LEFT_POINT, NusysConstants.RECTANGLE_REGION_TOP_LEFT_POINT_KEY },//top left point
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_TIMESPAN_END, NusysConstants.TIMESPAN_REGION_END_KEY }, //time end
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_TIMESPAN_START, NusysConstants.TIMESPAN_REGION_START_KEY },//time start
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_HEIGHT, NusysConstants.RECTANGLE_REGION_HEIGHT_KEY },//rect height 
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_WIDTH, NusysConstants.RECTANGLE_REGION_WIDTH_KEY },//rect width

            //links start
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_IN_ID_KEY, NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY },//link in id
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_OUT_ID_KEY, NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY },//link out id
        };

        /// <summary>
        /// the mappings method for libraryElements.  See the class description for more info.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Message LibraryElementRequestKeysToDatabaseKeys(Message requestMessage)
        {
            var databaseMessage = new Message(
                requestMessage.Where(kvp => LibraryElementMapping.ContainsKey(kvp.Key))
                .Select(kvp => new KeyValuePair<string,object>(LibraryElementMapping[kvp.Key],kvp.Value))
                .ToDictionary(k => k.Key, v=> v.Value));
            return databaseMessage;
        }
    }
}