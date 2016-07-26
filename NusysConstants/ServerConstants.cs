using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NusysConstants
{
    public class ServerConstants
    {
        public static readonly string GET_REQUEST_ID_STRING = "get_request_id";

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
        #endregion alias

        #region libraryElementModel
        /// <summary>
        /// 32-character string, aka an ID.  
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_ID_KEY = "id";

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
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_KEYWORDS_KEY = "keywords";

        /// <summary>
        /// string title for the library element
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_TITLE_KEY = "title";

        /// <summary>
        /// boolean representing if the library element model is favorited
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_FAVORITED_KEY = "favorited";

        /// <summary>
        /// URL for the large icon for this library element model.  
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_LARGE_ICON_URL_KEY = "large_icon_url";

        /// <summary>
        /// URL for the medium icon for this library element model.  
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_MEDIUM_ICON_URL_KEY = "medium_icon_url";

        /// <summary>
        /// URL for the small icon for this library element model.  
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_SMALL_ICON_URL_KEY = "small_icon_url";

        /// <summary>
        /// string id (NOT A REGULAR, 32-CHAR STRING ID) for the user creator of this object.  
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_CREATOR_USER_ID_KEY = "creator_user_id";

        /// <summary>
        /// datetime-parseable string for the creation of the current libraryelementmodel. 
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_CREATION_TIMESTAMP_KEY = "creation_timestamp";

        /// <summary>
        /// datetime-parseable string for the last edited time of the current libraryElementModel
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_LAST_EDITED_TIMESTAMP_KEY = "last_edited_timestamp";
        #endregion libraryElementModel

        #region metadata
        /// <summary>
        /// 32 character ID of the library element this metadata entry belongs to
        /// </summary>
        public static readonly string METADATA_LIBRARY_ELEMENT_ID_KEY = "library_id";

        /// <summary>
        /// the string used as the name of the 'key' column for metadata.  
        /// PROBABLY ONLY FOR SERVER-SIDE USE
        /// </summary>
        public static readonly string METADATA_KEY_COLUMN_KEY = "key";

        /// <summary>
        /// the string used as the name of the 'value' column for metadata.  
        /// PROBABLY ONLY FOR SERVER-SIDE USE
        /// </summary>
        public static readonly string METADATA_VALUE_COLUMN_KEY = "value";
        #endregion metadata

        #region propertiesTable
        /// <summary>
        /// 32 character ID of the library element this property belongs to
        /// </summary>
        public static readonly string PROPERTIES_LIBRARY_ID_KEY = "library_id";

        /// <summary>
        /// the string used as the name of the 'key' column for unique properties.  
        /// PROBABLY ONLY FOR SERVER-SIDE USE
        /// </summary>
        public static readonly string PROPERTIES_KEY_COLUMN_KEY = "key";

        /// <summary>
        /// the string used as the name of the string 'value' column for properties.  
        /// PROBABLY ONLY FOR SERVER-SIDE USE
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
        #endregion propertiesTable

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

        #endregion SQLTableNames

        /// <summary>
        /// the list of the request types 
        /// as of 7/25/16, many are depricated but still exist.  We should figure out which are which...
        /// </summary>
        public enum RequestType
        {
            DeleteSendableRequest,
            NewNodeRequest,
            FinalizeInkRequest,
            DuplicateNodeRequest,
            SystemRequest,
            NewLinkRequest,
            SendableUpdateRequest,
            NewThumbnailRequest,
            NewContentRequest,
            ChangeContentRequest,
            SetTagsRequest,
            ChatDialogRequest,
            CreateNewLibrayElementRequest,
            SubscribeToCollectionRequest,
            UnsubscribeFromCollectionRequest,
            DeleteLibraryElementRequest,
            AddInkRequest,
            RemoveInkRequest,
        }
    }
}
