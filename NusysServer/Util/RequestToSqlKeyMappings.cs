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
        /// the mapping dicitonary of request key to database key for library elements
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
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY, NusysConstants.LIBRARY_ELEMENT_ACCESS_KEY }, // access type
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_PARENT_ID_KEY, NusysConstants.LIBRARY_ELEMENT_MODEL_PARENT_ID_KEY },//clipping parent id

            //links start
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_IN_KEY, NusysConstants.LINK_LIBRARY_ELEMENT_IN_ID_KEY },//link in id
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_OUT_KEY, NusysConstants.LINK_LIBRARY_ELEMENT_OUT_ID_KEY },//link out id

            //collections start
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_COLLECTION_FINITE_BOOLEAN_KEY, NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_FINITE_BOOLEAN_KEY },//finite collection
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SHAPED_COLLECTION_POINTS_KEY, NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_POINTS_LIST_KEY },//shaped collection points
            {NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SHAPED_COLLECTION_ASPECT_RATIO_KEY, NusysConstants.COLLECTION_LIBRARY_ELEMENT_MODEL_SHAPED_ASPECT_RATIO_KEY }, // aspect ratio

            //video
            {NusysConstants.NEW_VIDEO_LIBRARY_ELEMENT_REQUEST_ASPECT_RATIO_KEY, NusysConstants.VIDEO_LIBRARY_ELEMENT_MODEL_RATIO_KEY },//video aspect ratio

            //image
            {NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_ASPECT_RATIO_KEY, NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_RATIO_KEY },//image aspect ratio
            {NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_HEIGHT, NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_HEIGHT_KEY },//rect height 
            {NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_WIDTH, NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_NORMALIZED_WIDTH_KEY },//rect width
            {NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_X, NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_X_KEY },//top left point x
            {NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_Y, NusysConstants.IMAGE_LIBRARY_ELEMENT_MODEL_TOP_LEFT_Y_KEY },//top left point y

            //audio
            {NusysConstants.NEW_AUDIO_LIBRARY_ELEMENT_REQUEST_TIME_END, NusysConstants.AUDIO_LIBRARY_ELEMENT_END_TIME_KEY }, //time end
            {NusysConstants.NEW_AUDIO_LIBRARY_ELEMENT_REQUEST_TIME_START, NusysConstants.AUDIO_LIBRARY_ELEMENT_START_TIME_KEY },//time start

            //pdf
            {NusysConstants.NEW_PDF_LIBRARY_ELEMENT_REQUEST_PAGE_START_KEY, NusysConstants.PDF_PAGE_START_KEY },//pdf page number
            {NusysConstants.NEW_PDF_LIBRARY_ELEMENT_REQUEST_PAGE_END_KEY, NusysConstants.PDF_PAGE_END_KEY },//pdf page number
        };


        /// <summary>
        /// the mapping dictionary of request key to database key for elements (aliases, nodes)
        /// </summary>
        public static readonly Dictionary<string, string> ElementMapping = new BiDictionary<string, string>()
        {
            //base types start
            {NusysConstants.NEW_ELEMENT_REQUEST_CREATOR_ID_KEY, NusysConstants.ALIAS_CREATOR_ID_KEY },//creator user id
            {NusysConstants.NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY, NusysConstants.ALIAS_LIBRARY_ID_KEY },//library Id
            {NusysConstants.NEW_ELEMENT_REQUEST_ACCESS_KEY, NusysConstants.ALIAS_ACCESS_KEY },//access
            {NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY, NusysConstants.ALIAS_ID_KEY }, // alias id
            {NusysConstants.NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY, NusysConstants.ALIAS_PARENT_COLLECTION_ID_KEY }, // parent collection id
            {NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_X_KEY, NusysConstants.ALIAS_LOCATION_X_KEY }, // x coordinate
            {NusysConstants.NEW_ELEMENT_REQUEST_LOCATION_Y_KEY, NusysConstants.ALIAS_LOCATION_Y_KEY }, // y coordinate
            {NusysConstants.NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY, NusysConstants.ALIAS_SIZE_HEIGHT_KEY }, // height
            {NusysConstants.NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY, NusysConstants.ALIAS_SIZE_WIDTH_KEY }, // width
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

        /// <summary>
        /// the mappings method for elements (aliases).  See the class description for more info.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        public static Message ElementRequestKeysToDatabaseKeys(Message requestMessage)
        {
            var databaseMessage = new Message(
                requestMessage.Where(kvp => ElementMapping.ContainsKey(kvp.Key))
                .Select(kvp => new KeyValuePair<string, object>(ElementMapping[kvp.Key], kvp.Value))
                .ToDictionary(k => k.Key, v => v.Value));
            return databaseMessage;
        }
    }
}