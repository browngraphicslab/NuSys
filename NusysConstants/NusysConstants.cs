using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NusysIntermediate
{
    public class NusysConstants
    {

        #region RequestManagementKeys

        /// <summary>
        /// key for the 32-char value id that represents the id of this request.
        /// This will be used to stop a thread and await this id returning from the server to resume that thead. 
        /// used to simulate async functions that will return agter the server call has executed.
        /// </summary>
        public static readonly string RETURN_AWAITABLE_REQUEST_ID_STRING = "awaitable_request_id";

        /// <summary>
        /// the string key used to identify the request type of a request being sent. 
        /// the value for this key should be an stringified ElementType with the .ToString() method called
        /// </summary>
        public static readonly string REQUEST_TYPE_STRING_KEY = "request_type";

        #endregion RequestManagementKeys

        #region RequestKeys

            #region AllRequests
            /// <summary>
            /// should be returned by all requests if it is successful
            /// </summary>
            public static readonly string REQUEST_SUCCESS_BOOL_KEY = "successful_request";

            /// <summary>
            /// MIGHT be returned as the key that hold the error message 
            /// if an error occurs during the request handling
            /// </summary>
            public static readonly string REQUEST_ERROR_MESSAGE_KEY = "error_message";

            #endregion AllRequests

            #region GetContentDataModelRequest

            /// <summary>
            /// The key whose value will be the 32 character string for the id of the contentDataModel you are fetching
            /// </summary>
            public static readonly string GET_CONTENT_DATA_MODEL_REQUEST_CONTENT_ID_KEY = "content_data_model_id";
        
            /// <summary>
            /// The key whose value should be the returned, json serialized, ContentDataModel requested
            /// </summary>
            public static readonly string GET_CONTENT_DATA_MODEL_REQUEST_RETURNED_CONTENT_DATA_MODEL_KEY = "returned_content_data_model";

            #endregion GetContentDataModelRequest

            #region GetEntireWorkspaceRequest
            /// <summary>
            /// the key used to send the 32-char value of the libraryId of the collection being asked for 
            /// </summary>
            public static readonly string GET_ENTIRE_WORKSPACE_REQUEST_COLLECTION_ID_KEY = "collection_id_to_get";

            /// <summary>
            /// The key used to hold the GetEntireWorkspaceRequestArgs class
            /// </summary>
            public static readonly string GET_ENTIRE_WORKSPACE_REQUEST_RETURN_ARGUMENTS_KEY = "returned_workspace_arguments";

            #endregion GetEntireWorkspaceRequest

            #region CreateNewContentRequest

            /// <summary>
            /// The key used to hold the type of content being added
            /// </summary>
            public static readonly string CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY = "content_type_key";

            /// <summary>
            /// The key used to hold the content id.
            /// </summary>
            public static readonly string CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY = "content_id_key";

            /// <summary>
            /// The key used to hold the base 64 string of the contents data bytes
            /// </summary>
            public static readonly string CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES = "content_id_key";

            /// <summary>
            /// The key used to hold the string file extention required when creating audio, video, or image content (e.g .jpeg, .mp3, .mp4)
            /// </summary>
            public static readonly string CREATE_NEW_CONTENT_REQUEST_CONTENT_FILE_EXTENTION = "content_file_extention";

            #endregion CreateNewContentRequest

            #region DeleteLibraryElementRequest

            /// <summary>
            /// the key used to send the library element id key of the library element to be deleted
            /// </summary>
            public static readonly string DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY = "library_id";

        #endregion DeleteLibraryElementRequest

        #region DeleteElementRequest
        /// <summary>
        /// the key used to send the element id key of the element (alias) to be deleted
        /// </summary>
        public static readonly string DELETE_ELEMENT_REQUEST_LIBRARY_ID_KEY = "library_id";
        #endregion DeleteElementRequest

        #region NewElementRequest
        /// <summary>
        /// the key used to send the element id of the new element to be created
        /// </summary>
        public static readonly string NEW_ELEMENT_REQUEST_ELEMENT_ID_KEY = "id";

        /// <summary>
        /// the key used to send the library element id of the new element to be created
        /// </summary>
        public static readonly string NEW_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID_KEY = "library_id";

        /// <summary>
        /// the key used to send the x coordinate of the new element to be created
        /// </summary>
        public static readonly string NEW_ELEMENT_REQUEST_LOCATION_X_KEY = "x";

        /// <summary>
        /// the key used to send the y coordinate of the new element to be created
        /// </summary>
        public static readonly string NEW_ELEMENT_REQUEST_LOCATION_Y_KEY = "y";

        /// <summary>
        /// the key used to send the width of the new element to be created
        /// </summary>
        public static readonly string NEW_ELEMENT_REQUEST_SIZE_WIDTH_KEY = "width";

        /// <summary>
        /// the key used to send the height of the new element to be created
        /// </summary>
        public static readonly string NEW_ELEMENT_REQUEST_SIZE_HEIGHT_KEY = "height";

        /// <summary>
        /// the key used to send the collection id of the collection the element belongs to
        /// </summary>
        public static readonly string NEW_ELEMENT_REQUEST_ELEMENT_PARENT_COLLECTION_ID_KEY = "parent_collection_id";
        #endregion NewElementRequest

        #endregion RequestKeys

        #region SQLColumnNames

            #region alias

        /// <summary>
        /// 32 character string, aka an ID.  
        /// </summary>
        public static readonly string ALIAS_ID_KEY = "id";

            /// <summary>
            /// 32 character string, aka an ID. 
            /// the library element ID that this alias points to
            /// </summary>
            public static readonly string ALIAS_LIBRARY_ID_KEY = "library_id";

            /// <summary>
            /// double, the x coordinate of this alias
            /// </summary>
            public static readonly string ALIAS_LOCATION_X_KEY = "x";

            /// <summary>
            /// double, the y coordinate of this alias
            /// </summary>
            public static readonly string ALIAS_LOCATION_Y_KEY = "y";

            /// <summary>
            /// double, the width of this alias
            /// </summary>
            public static readonly string ALIAS_SIZE_WIDTH_KEY = "width";

            /// <summary>
            /// double, the height of this alias
            /// </summary>
            public static readonly string ALIAS_SIZE_HEIGHT_KEY = "height";

            /// <summary>
            /// 32 character string, aka an ID.  
            /// the id of the collection that this alias belongs inside of
            /// </summary>
            public static readonly string ALIAS_PARENT_COLLECTION_ID_KEY = "parent_collection_id";

            /// <summary>
            /// the dictionary from keys that are accepted and put into the Alias table to their type. 
            /// the keys not included in this list are put into the properties table
            /// </summary>
            public static readonly Dictionary<string, Type> ALIAS_ACCEPTED_KEYS = new Dictionary<string, Type>()
            {
                {ALIAS_ID_KEY, typeof(string)},
                { ALIAS_LIBRARY_ID_KEY, typeof(string)},
                { ALIAS_LOCATION_X_KEY,typeof(float)},
                { ALIAS_LOCATION_Y_KEY,typeof(float)},
                {ALIAS_SIZE_WIDTH_KEY,typeof(float)},
                { ALIAS_SIZE_HEIGHT_KEY,typeof(float)},
                { ALIAS_PARENT_COLLECTION_ID_KEY,typeof(string)},
            };

            #endregion alias

            #region libraryElementModel
            /// <summary>
            /// element type enum in string form.  call ElementType.ToString() to get this string when you have an elementtype variable
            /// 32 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_TYPE_KEY = "type";

            /// <summary>
            /// 32 character string, aka an ID.  
            /// the id of this library element
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_LIBRARY_ID_KEY = "library_id";

            /// <summary>
            /// 32 character string, aka an ID.  
            /// the content that this library element points to
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_CONTENT_ID_KEY = "content_id";

            /// <summary>
            /// string that represents the json-stringified dictionary of keywords
            /// approximately 2048 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_KEYWORDS_KEY = "keywords";

            /// <summary>
            /// string title for the library element
            /// approximately 2048 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_TITLE_KEY = "title";

            /// <summary>
            /// boolean representing if the library element model is favorited
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_FAVORITED_KEY = "favorited";

            /// <summary>
            /// URL for the large icon for this library element model.  
            /// approximately 512 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_LARGE_ICON_URL_KEY = "large_icon_url";

            /// <summary>
            /// URL for the medium icon for this library element model.  
            /// approximately 512 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY = "medium_icon_url";

            /// <summary>
            /// URL for the small icon for this library element model.  
            /// approximately 512 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_SMALL_ICON_URL_KEY = "small_icon_url";

            /// <summary>
            /// string id (NOT A REGULAR, 32-CHAR STRING ID) for the user creator of this object.
            /// approximately 2048 characters max  
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_CREATOR_USER_ID_KEY = "creator_user_id";

            /// <summary>
            /// datetime-parseable string for the creation of the current libraryelementmodel. 
            /// approximately 512 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY = "creation_timestamp";

            /// <summary>
            /// datetime-parseable string for the last edited time of the current libraryElementModel. 
            /// approximately 512 characters max
            /// </summary>
            public static readonly string LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY = "last_edited_timestamp";

            /// <summary>
            /// the dictionary from keys that are accepted and put into the library elmeent table to their type. 
            /// the keys not included in this list are put into the properties table
            /// </summary>
            public static readonly Dictionary<string, Type> LIBRARY_ELEMENT_MODEL_ACCEPTED_KEYS = new Dictionary<string, Type>()
            {
                {LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY, typeof(string)},
                { LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY, typeof(string)},
                { LIBRARY_ELEMENT_CREATOR_USER_ID_KEY,typeof(string)},
                { LIBRARY_ELEMENT_SMALL_ICON_URL_KEY,typeof(string)},
                { LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY,typeof(string)},
                { LIBRARY_ELEMENT_LARGE_ICON_URL_KEY,typeof(string)},
                { LIBRARY_ELEMENT_FAVORITED_KEY,typeof(bool)},
                { LIBRARY_ELEMENT_TITLE_KEY,typeof(string)},
                { LIBRARY_ELEMENT_KEYWORDS_KEY,typeof(string)},
                { LIBRARY_ELEMENT_LIBRARY_ID_KEY,typeof(string)},
                { LIBRARY_ELEMENT_CONTENT_ID_KEY,typeof(string)},
                { LIBRARY_ELEMENT_TYPE_KEY,typeof(string)},
            };
            #endregion libraryElementModel

            #region metadata
            /// <summary>
            /// 32 character ID of the library element this metadata entry belongs to
            /// </summary>
            public static readonly string METADATA_LIBRARY_ELEMENT_ID_KEY = "library_id";

            /// <summary>
            /// the string used as the name of the 'key' column for metadata.  
            /// approximately 512 characters max
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// </summary>
            public static readonly string METADATA_KEY_COLUMN_KEY = "key_string";

            /// <summary>
            /// the string used as the name of the 'value' column for metadata.  
            /// approximately 2048 characters max
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// </summary>
            public static readonly string METADATA_VALUE_COLUMN_KEY = "value_string";

            /// <summary>
            /// the list of all the column names for the metadata table.
            /// Should be populated by other constants as strings
            /// </summary>
            public static readonly HashSet<string> ACCEPTED_METADATA_TABLE_KEYS = new HashSet<string>()
            {
                METADATA_LIBRARY_ELEMENT_ID_KEY,
                METADATA_KEY_COLUMN_KEY,
                METADATA_VALUE_COLUMN_KEY
            };
            #endregion metadata

            #region propertiesTable

            /// <summary>
            /// 32 character ID of the library element or alias that this property belongs to
            /// </summary>
            public static readonly string PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY = "library_id";

            /// <summary>
            /// the string used as the name of the 'key' column for unique properties.  
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// approximately 512 characters max
            /// </summary>
            public static readonly string PROPERTIES_KEY_COLUMN_KEY = "key_string";

            /// <summary>
            /// the string used as the name of the string 'value' column for properties.  
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// approximately 2048 characters max
            /// </summary>
            public static readonly string PROPERTIES_STRING_VALUE_COLUMN_KEY = "string_value";

            /// <summary>
            /// the string used as the name of the numerical 'value' column for properties.  
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// </summary>
            public static readonly string PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY = "numerical_value";

            /// <summary>
            /// the string used as the name of the date 'value' column for properties.  
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// </summary>
            public static readonly string PROPERTIES_DATE_VALUE_COLUMN_KEY = "date_value";

            /// <summary>
            /// the list of all the column names for the properties table.
            /// Should be populated by other constants as strings
            /// </summary>
            public static readonly HashSet<string> ACCEPTED_PROPERTIES_TABLE_KEYS = new HashSet<string>()
            {
                PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY,
                PROPERTIES_KEY_COLUMN_KEY,
                PROPERTIES_STRING_VALUE_COLUMN_KEY,
                PROPERTIES_NUMERICAL_VALUE_COLUMN_KEY,
                PROPERTIES_DATE_VALUE_COLUMN_KEY
            };
            
            /// <summary>
            /// List of strings that cannot be used to populate the properties table.  
            /// Keys used to manage Networking and Requests should not be stored as properties.  
            /// This list should be populated by other constants.  
            /// </summary>
            public static readonly HashSet<string> ILLEGAL_PROPERTIES_TABLE_KEY_NAMES = new HashSet<string>()
            {
                
            };

            #endregion propertiesTable

            #region Content
            /// <summary>
            /// 32 character ID of the content
            /// </summary>
            public static readonly string CONTENT_TABLE_CONTENT_ID_KEY = "content_id";

            /// <summary>
            /// 32 character type of content. e.g. image, video, pdf
            /// </summary>
            public static readonly string CONTENT_TABLE_TYPE_KEY = "content_type";

            /// <summary>
            /// varchar url of the content.
            /// approximately 512 characters max
            /// </summary>
            public static readonly string CONTENT_TABLE_CONTENT_URL_KEY = "content_ur";

            /// <summary>
            /// the list of keys that will safely be entered into the contents table.  
            /// Use this to make sure that you're entering correct keys into the database
            /// </summary>
            public static readonly HashSet<string> ACCEPTED_CONTENT_TABLE_KEYS = new HashSet<string>()
            {
                CONTENT_TABLE_CONTENT_ID_KEY,
                CONTENT_TABLE_TYPE_KEY,
                CONTENT_TABLE_CONTENT_URL_KEY
            };
            #endregion Content

        #endregion SQLColumnNames

        #region SQLTableNames

        /// <summary>
        /// The string name of the properties SQL table in our database
        /// </summary>
        public static readonly string PROPERTIES_SQL_TABLE_NAME = "properties";

        /// <summary>
        /// The string name of the aliases SQL table in our database
        /// </summary>
        public static readonly string ALIASES_SQL_TABLE_NAME = "alias";

        /// <summary>
        /// The string name of the SQL table for library elements in our database
        /// </summary>
        public static readonly string LIBRARY_ELEMENTS_SQL_TABLE_NAME = "library_elements";

        /// <summary>
        /// The string name of the metadata SQL table in our database
        /// </summary>
        public static readonly string METADATA_SQL_TABLE_NAME = "metadata";

        /// <summary>
        /// The string name of the metadata SQL table in our database
        /// </summary>
        public static readonly string CONTENTS_SQL_TABLE_NAME = "contents";

        #endregion SQLTableNames

        #region UnPackingModelKeys

            #region LibraryElementModel

        /// <summary>
        /// This key is used to hold the metadata for library element models when represented in message form.
        /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_METADATA_KEY = "library_element_metadata";

        #endregion LibraryElementModel

        #endregion UnPackingModelKeys
        
        #region Enums
        /// <summary>
        /// The base types for every content.  ElementTypes can have the same content Types.
        /// For instance, Image and ImageRegion both have ContentType Image
        /// </summary>
        public enum ContentType
        {
            Text,
            PDF,
            Image,
            Video,
            Audio
        }

        /// <summary>
        /// the list of the request types 
        /// as of 7/25/16, many are depricated but still exist.  We should figure out which are which...
        /// </summary>
        public enum RequestType
        {
            FinalizeInkRequest,
            DuplicateNodeRequest,
            SystemRequest,
            NewLinkRequest,
            SendableUpdateRequest,
            NewThumbnailRequest,
            ChangeContentRequest,
            SetTagsRequest,
            ChatDialogRequest,
            SubscribeToCollectionRequest,
            UnsubscribeFromCollectionRequest,
            AddInkRequest,
            RemoveInkRequest,
            ChatRequest,
            //below this line should be the new-server-approved requests
            GetContentDataModelRequest,
            CreateNewLibrayElementRequest,
            GetEntireWorkspaceRequest,
            NewElementRequest,
            DeleteLibraryElementRequest,
            DeleteElementRequest,

            /// <summary>
            /// This request will create a new content AND a default new library element for that content
            /// Therefore this request should be called when someone uploads a new content to the library
            /// </summary>
            CreateNewContentRequest
        }

        /// <summary>
        /// the list of all the element types in all of Nusys
        /// </summary>
        public enum ElementType
        {

            // Basic Types
            Text, Image, Word, Powerpoint, Collection, PDF, Audio, Video, Tag, Web, Area, Link, Recording,

            // Region Types
            ImageRegion, PdfRegion, AudioRegion, VideoRegion,

            // weird type that possibly shouldn't be here
            Tools
        }

        #endregion Enums

        #region staticMethods

        public static bool IsRegionType(NusysConstants.ElementType type)
        {
            return type == NusysConstants.ElementType.AudioRegion || type == NusysConstants.ElementType.ImageRegion || type == NusysConstants.ElementType.VideoRegion ||
                   type == NusysConstants.ElementType.PdfRegion;
        }

        #endregion staticMethods
    }
}