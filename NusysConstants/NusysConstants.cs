using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NusysIntermediate
{
    public class NusysConstants
    {
        /// <summary>
        /// The boolean for testing locally.  If this is not true and you are running a local server, you won't connect.
        /// </summary>
        public static bool TEST_LOCAL_BOOLEAN = true;

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

            #region DeleteElementRequest

            /// <summary>
            /// The ID of the element Model that you wish to delete using this DeleteElementRequest
            /// </summary>
            public static readonly string DELETE_ELEMENT_REQUEST_ELEMENT_ID = "element_id_to_delete";

            /// <summary>
            /// The ID of the element Model that you wish to delete using this DeleteElementRequest
            /// </summary>
            public static readonly string DELETE_ELEMENT_REQUEST_RETURNED_DELETED_ELEMENT_ID = "element_id_deleted";

            #endregion DeleteElementRequest

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
            public static readonly string CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES = "content_data_bytes_key";

            /// <summary>
            /// The key used to hold the string file extention required when creating audio, video, or image content (e.g .jpeg, .mp3, .mp4)
            /// </summary>
            public static readonly string CREATE_NEW_CONTENT_REQUEST_CONTENT_FILE_EXTENTION = "content_file_extention";

            /// <summary>
            /// key for holding the pdf text of a newly uploaded pdf content.  This is not a sql table field.
            /// </summary>
            public static readonly string CREATE_NEW_PDF_CONTENT_REQUEST_PDF_TEXT_KEY = "pdf_text";

            /// <summary>
            /// This is returned when the NewContentRequest is succesfull. 
            /// The value will be a josn-serialized library element model that can be run through the factory to get the model.
            /// </summary>
            public static readonly string NEW_CONTENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY = "returned_library_element_model";

        #endregion CreateNewContentRequest

            #region SearchRequest

            /// <summary>
            /// the key that will hold the json-serialized Query class when creating a search request.
            /// </summary>
            public static readonly string SEARCH_REQUEST_SERIALIZED_QUERY_KEY = "query_json";

            /// <summary>
            /// The key that will hold the json-serialized version of the returned SearchResults class.  
            /// The search resuls can be deserialized and fetched from the search request.
            /// </summary>
            public static readonly string SEARCH_REQUEST_RETURNED_SEARCH_RESULTS_KEY = "search_results_json";

            #endregion SearchRequest

            #region DeleteLibraryElementRequest

            /// <summary>
            /// the key used to send the library element id key of the library element to be deleted
            /// </summary>
            public static readonly string DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY = "library_id";

            /// <summary>
            /// the key used in the returning request indicating which IDs were deleted from the library via the server.  
            /// There can be multiple keys because sometimes the server will have to delete multiple items in order to maintain the collections correctly. 
            /// The returned object as this key's value will be in the form of a list.  
            /// </summary>
            public static readonly string DELETE_LIBRARY_ELEMENT_REQUEST_RETURNED_DELETED_LIBRARY_IDS_KEY = "deleted_library_ids";

            #endregion DeleteLibraryElementRequest

            #region GetAllLibraryElementsRequest

            /// <summary>
            /// The key that represents the returned list of libraryElementModels serialzed as json strings.
            /// </summary>
            public static readonly string GET_ALL_LIBRARY_ELEMENTS_REQUEST_RETURNED_LIBRARY_ELEMENT_MODELS_KEY = "returned_library_element_models";

                #endregion GetAllLibraryElementsRequst

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

            /// <summary>
            /// the key used to send the id of the creator
            /// </summary>
            public static readonly string NEW_ELEMENT_REQUEST_CREATOR_ID_KEY = "creator_id_key";

            /// <summary>
            /// the key used to send the access level of the element request key
            /// </summary>
            public static readonly string NEW_ELEMENT_REQUEST_ACCESS_KEY = "access";

            /// <summary>
            /// This is returned when the NewElementReqeust request is succesfull. 
            /// The value will be a josn-serialized ElementModel that can be run through the factory
            /// </summary>
            public static readonly string NEW_ELEMENT_REQUEST_RETURNED_ELEMENT_MODEL_KEY = "returned_element_model";


        #endregion NewElementRequest

            #region ElementUpdateRequest

            /// <summary>
            /// the id of the element to update with the ElementUpdateRequest
            /// </summary>
            public static readonly string ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY = "element_id";

            /// <summary>
            /// This key will hold the boolean indicating whether the request should save to the server or simply be forwarded to everyone else
            /// </summary>
            public static readonly string ELEMENT_UPDATE_REQUEST_SAVE_TO_SERVER_BOOLEAN = "save_changes_to_server";

        #endregion ElementUpdateRequest

            #region UpdateLibraryElementRequest

            /// <summary>
            /// In the UpdateLibraryElementRequest this is the key that will be used to hold the LibraryElementId of the library element being updated. 
            /// </summary>
            public static readonly string UPDATE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ELEMENT_ID = "library_element_id_to_update";

            #endregion UpdateLibraryElementRequest

            #region NewLibraryElementRequest
            /// <summary>
            /// key in message for sending type when creating new library element request
            /// </summary>
            public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY = "type";

                /// <summary>
                /// key in message for sending library id when creating new library element request

                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY = "library_id";

                /// <summary>
                /// key in message for sending content id when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY = "content_id";

                /// <summary>
                /// key in message for sending keywords when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_KEYWORDS_KEY = "keywords";

                /// <summary>
                /// key in message for sending title when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY = "title";

                /// <summary>
                /// key in message for sending favorited bool when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_FAVORITED_KEY = "favorited";

                /// <summary>
                /// key in message for sending large icon base-64 byte string when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_LARGE_ICON_BYTE_STRING_KEY = "large_icon_bytes";

                /// <summary>
                /// key in message for sending medium icon base-64 byte string when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_MEDIUM_ICON_BYTE_STRING_KEY = "medium_icon_bytes";

                /// <summary>
                /// key in message for sending small icon base-64 byte string when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_SMALL_ICON_BYTE_STRING_KEY = "small_icon_bytes";

                /// <summary>
                /// key in message for sending creator id when creating new library element request 
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_CREATOR_USER_ID_KEY = "creator_user_id";

                /// <summary>
                /// key in message for sending creation date when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_CREATION_TIMESTAMP_KEY = "creation_timestamp";

                /// <summary>
                /// key in message for sending last edited timestamp when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_LAST_EDITED_TIMESTAMP_KEY = "last_edited_timestamp";

                /// <summary>
                /// key in message for access type when creating new library element request
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY = "access";

                /// <summary>
                /// the key that will hold the library ElementId of the clipping parent of a requested region.  
                /// Used during a region libraryElementRequest.  
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_REGION_CLIPPING_PARENT_ID = "clipping_parent_id";
                /// <summary>
                /// the key that will hold the PointModel of the top left point of the requested region  
                /// Used during a region libraryElementRequest.  
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_TOP_LEFT_POINT = "rectangle_top_left_point"; 
                /// <summary>
                /// the key that will hold the width of the requested region  
                /// Used during a region libraryElementRequest.  
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_WIDTH = "rectangle_region_width";
                /// <summary>
                /// the key that will hold the height of the requested region  
                /// Used during a region libraryElementRequest.  
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_REGION_RECTANGLE_HEIGHT = "rectangle_region_height";
                /// <summary>
                /// the key that will hold the page location of the requested region  
                /// Used during a region libraryElementRequest.  
                /// </summary>
               public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_REGION_PDF_PAGE_LOCATION = "pdf_region_page_location";
               /// <summary>
               /// the key that will hold the start of the interval of the requested region  
               /// Used during a region libraryElementRequest.  
               /// </summary>
               public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_REGION_TIMESPAN_START = "time_region_start";    
               /// <summary>
               /// the key that will hold the end of the interval of the requested region  
               /// Used during a region libraryElementRequest.  
               /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_REGION_TIMESPAN_END = "time_region_end";
                /// <summary>
                /// key in message for when the request returns with the fully populated libraryelementModel.
                /// When the library element model request returns, it will pass a library element model as a json using this key
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY = "returned_library_element_model";
                /// <summary>
                /// the key that will hold the libraryelementmodelid of one of the libraryelementmodels being linked  
                /// Used during a region libraryElementRequest.  
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_IN_KEY = "link_library_element_model_id_in";
                /// <summary>
                /// the key that will hold the libraryelementmodelid of one of the libraryelementmodels being linked  
                /// Used during a region libraryElementRequest.  
                /// </summary>
                public static readonly string NEW_LIBRARY_ELEMENT_REQUEST_LINK_ID_OUT_KEY = "link_library_element_model_id_out";
        #endregion NewLibraryElementRequest

            #region CreateNewMetadataRequest
        /// <summary>
        /// key in message for library id of the element that the metadata belongs to
        /// </summary>
        public static readonly string CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY = "library_id";

        /// <summary>
        /// key in message for sending the metadata key
        /// </summary>
        public static readonly string CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY = "key";

        /// <summary>
        /// key in message for sending the metadata value
        /// </summary>
        public static readonly string CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY = "value";

        /// <summary>
        /// key in message for sending the metadata mutability type
        /// </summary>
        public static readonly string CREATE_NEW_METADATA_REQUEST_METADATA_MUTABILITY_KEY = "mutability";

        /// <summary>
        /// key in message for forwarding the MetadataEntry class to everyone
        /// </summary>
        public static readonly string CREATE_NEW_METADATA_REQUEST_RETURNED_METADATA_ENTRY_KEY = "metadata_entry";
        #endregion CreateNewMetadataRequest

            #region DeleteMetadataRequest
        /// <summary>
        /// key in message for library id of the element that the metadata to be deleted belongs to
        /// </summary>
        public static readonly string DELETE_METADATA_REQUEST_LIBRARY_ID_KEY = "library_id";

        /// <summary>
        /// key in message for signifying which entry should be deleted
        /// </summary>
        public static readonly string DELETE_METADATA_REQUEST_METADATA_KEY = "key";
        #endregion DeleteMetadataRequest

        #region UpdateMetadataRequest
        /// <summary>
        /// key in message for library id of the element that the metadata to be edited belongs to
        /// </summary>
        public static readonly string UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY = "library_id";

        /// <summary>
        /// key in message for signifying which entry should be edited
        /// </summary>
        public static readonly string UPDATE_METADATA_REQUEST_METADATA_KEY = "key";

        /// <summary>
        /// key in message for signifying the new value for the entry
        /// </summary>
        public static readonly string UPDATE_METADATA_REQUEST_METADATA_VALUE = "value";
        
        #endregion UpdateMetadataRequest

        #region CreateNewPresentationLinkRequest
        /// <summary>
        /// The key for sending the link id for the create new presentation link request
        /// </summary>
        public static readonly string CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_ID_KEY = "link_id";

        /// <summary>
        /// The key for sending the link IN id for the create new presentation link request
        /// </summary>
        public static readonly string CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY = "link_in_element_id";

        /// <summary>
        /// The key for sending the link OUT id for the create new presentation link request
        /// </summary>
        public static readonly string CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY = "link_out_element_id";

        /// <summary>
        /// The key for sending the parent collection id for the create new presentation link request
        /// </summary>
        public static readonly string CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY = "parent_collection_id";

        /// <summary>
        /// The key for sending the annotation for the create new presentation link request
        /// </summary>
        public static readonly string CREATE_NEW_PRESENTATION_LINK_REQUEST_ANNOTATION_KEY = "annotation";

        /// <summary>
        /// The key for sending the annotation for the create new presentation link request
        /// </summary>
        public static readonly string CREATE_NEW_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY = "returned_presentation_link_model";

        #endregion CreateNewPresentationLinkRequest

        #region DeletePresentationLinkRequest
        /// <summary>
        /// The key for sending the link id for the delete presentation link request
        /// </summary>
        public static readonly string DELETE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY = "link_id";
        #endregion DeletePresentationLinkRequest

        #region UpdatePresentationLinkRequest
        /// <summary>
        /// The key for sending the link id for the update presentation link request
        /// </summary>
        public static readonly string UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY = "link_id";

        /// <summary>
        /// The key for sending the link IN id for the update presentation link request
        /// </summary>
        public static readonly string UPDATE_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY = "link_in_element_id";

        /// <summary>
        /// The key for sending the link OUT id for the update presentation link request
        /// </summary>
        public static readonly string UPDATE_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY = "link_out_element_id";

        /// <summary>
        /// The key for sending the parent collection id for the update presentation link request
        /// </summary>
        public static readonly string UPDATE_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY = "parent_collection_id";

        /// <summary>
        /// The key for sending the annotation for the update presentation link request
        /// </summary>
        public static readonly string UPDATE_PRESENTATION_LINK_REQUEST_ANNOTATION_KEY = "annotation";

        /// <summary>
        /// The key for sending the annotation for the update presentation link request
        /// </summary>
        public static readonly string UPDATE_PRESENTATION_LINK_REQUEST_RETURNED_PRESENTATION_LINK_MODEL_KEY = "returned_presentation_link_model";
        #endregion UpdatePresentationLinkRequest
        

        #region ChatRequest

        /// <summary>
        /// Key in message for sending user id in chat requests
        /// </summary>
        public static readonly string CHAT_REQUEST_USER_ID_KEY="user";

            /// <summary>
            /// Key in message for sending chat messages in chat requests
            /// </summary>
            public static readonly string CHAT_REQUEST_CHAT_MESSAGE_KEY = "chat_message";
            #endregion

        #endregion RequestKeys

        #region SQLColumnNames

            #region alias

        /// <summary>
        /// 32 character string, aka an ID.  
        /// </summary>
        public static readonly string ALIAS_ID_KEY = "alias_id";

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
            /// the id of the user who created the alias
            /// </summary>
            public static readonly string ALIAS_PARENT_COLLECTION_ID_KEY = "parent_collection_id";

        /// <summary>
        /// 32 character string, aka an ID.  
        /// the id of the collection that this alias belongs inside of
        /// </summary>
        public static readonly string ALIAS_CREATOR_ID_KEY = "creator_user_id";

        /// <summary>
        /// 32 character string  
        /// Represents the level of access.
        /// </summary>
        public static readonly string ALIAS_ACCESS_KEY = "access";

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
                {  ALIAS_CREATOR_ID_KEY, typeof(string) },
            {ALIAS_ACCESS_KEY, typeof(string) }
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
        /// 32 character string  
        /// Represents the level of access.
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_ACCESS_KEY = "access";

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
                { LIBRARY_ELEMENT_ACCESS_KEY,typeof(string)},
            };
            #endregion libraryElementModel

            #region metadata
            /// <summary>
            /// 32 character ID of the library element this metadata entry belongs to
            /// </summary>
            public static readonly string METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY = "metadata_library_id";

            /// <summary>
            /// the string used as the name of the 'key' column for metadata.  
            /// approximately 512 characters max
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// </summary>
            public static readonly string METADATA_KEY_COLUMN_KEY = "metadata_key_string";

            /// <summary>
            /// the string used as the name of the 'value' column for metadata.  
            /// approximately 2048 characters max
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// </summary>
            public static readonly string METADATA_VALUE_COLUMN_KEY = "metadata_value_string";

        /// <summary>
        /// the string used as the name of the 'mutability' column for metadata.  
        /// approximately 2048 characters max
        /// PROBABLY ONLY FOR SERVER-SIDE USE
        /// </summary>
        public static readonly string METADATA_MUTABILITY_COLUMN_KEY = "metadata_mutability_string";

        /// <summary>
        /// the list of all the column names for the metadata table.
        /// Should be populated by other constants as strings
        /// </summary>
        public static readonly HashSet<string> ACCEPTED_METADATA_TABLE_KEYS = new HashSet<string>()
            {
                METADATA_LIBRARY_ELEMENT_ID_COLUMN_KEY,
                METADATA_KEY_COLUMN_KEY,
                METADATA_VALUE_COLUMN_KEY
            };
            #endregion metadata

            #region propertiesTable

            /// <summary>
            /// 32 character ID of the library element or alias that this property belongs to
            /// </summary>
            public static readonly string PROPERTIES_LIBRARY_OR_ALIAS_ID_KEY = "library_or_alias_id";

            /// <summary>
            /// the string used as the name of the 'key' column for unique properties.  
            /// PROBABLY ONLY FOR SERVER-SIDE USE
            /// approximately 512 characters max
            /// IF YOU CHANGE THIS STRING TO JUST 'key' IT WILL BREAK THE SQL STATEMENTS
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
                RETURN_AWAITABLE_REQUEST_ID_STRING,
                REQUEST_TYPE_STRING_KEY,
                ELEMENT_UPDATE_REQUEST_ELEMENT_ID_KEY
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
            /// varchar url of the content. MAX
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

            #region PresentationLinks

            /// <summary>
            /// 32 character Id of the presentation link.
            /// </summary>
            public static readonly string PRESENTATION_LINKS_TABLE_LINK_ID_KEY = "link_id";

            /// <summary>
            ///  32 character Id of the presentation link's In-Element ElementId;
            /// </summary>
            public static readonly string PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY = "link_in_element_id";

            /// <summary>
            ///  32 character Id of the presentation link's Out-Element ElementId;
            /// </summary>
            public static readonly string PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY = "link_out_element_id";

            /// <summary>
            ///  32 character Id of the presentation link's parent collection Id.
            /// </summary>
            public static readonly string PRESENTATION_LINKS_TABLE_PARENT_COLLECTION_LIBRARY_ID_KEY = "parent_collection_id";

            /// <summary>
            ///  max 2048 character string that will be the presentation link's annotation string.
            /// </summary>
            public static readonly string PRESENTATION_LINKS_TABLE_ANNOTATION_TEXT_KEY = "annotation";

            /// <summary>
            /// the list of keys that will safely be entered into the contents table.  
            /// Use this to make sure that you're entering correct keys into the database
            /// </summary>
            public static readonly HashSet<string> ACCEPTED_PRESENTATION_LINKS_TABLE_KEYS = new HashSet<string>()
            {
                PRESENTATION_LINKS_TABLE_LINK_ID_KEY,
                PRESENTATION_LINKS_TABLE_IN_ELEMENT_ID_KEY,
                PRESENTATION_LINKS_TABLE_OUT_ELEMENT_ID_KEY,
                PRESENTATION_LINKS_TABLE_PARENT_COLLECTION_LIBRARY_ID_KEY,
                PRESENTATION_LINKS_TABLE_ANNOTATION_TEXT_KEY
            };

        #endregion PresentationLinks

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

        /// <summary>
        /// The string name of the presentation links SQL table in our database
        /// </summary>
        public static readonly string PRESENTATION_LINKS_SQL_TABLE_NAME = "presentation_link";

        #endregion SQLTableNames

        #region UnPackingModelKeys

        #region LibraryElementModel

        /// <summary>
        /// This key is used to hold the metadata for library element models when represented in message form.
        /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
        /// </summary>
        public static readonly string LIBRARY_ELEMENT_METADATA_KEY = "library_element_metadata";

            /// <summary>
            /// This key is used to hold the clipping parent's library element Id.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string REGION_CLIPPING_PARENT_ID_KEY = "clipping_parent_id_key";

            #region RectangleRegion
            /// <summary>
            /// This key is used to hold the top left point of the rectangular region when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string RECTANGLE_REGION_TOP_LEFT_POINT_KEY = "top_left_point";

            /// <summary>
            /// This key is used to hold the width of the rectangular region when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string RECTANGLE_REGION_WIDTH_KEY = "width";

            /// <summary>
            /// This key is used to hold the height of the rectangular region when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string RECTANGLE_REGION_HEIGHT_KEY = "height";
            #endregion RectangleRegion

            #region AudioRegion
            /// <summary>
            /// This key is used to hold the start time of a audio region when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string TIMESPAN_REGION_START_KEY = "start";

            /// <summary>
            /// This key is used to hold the end time of a audio region when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string TIMESPAN_REGION_END_KEY = "end";


            #endregion AudioRegion

            #region PdfRegion
            /// <summary>
            /// This key is used to hold the page of a pdf region when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string PDF_REGION_PAGE_NUMBER_KEY = "page_number";
            #endregion PdfRegion 

            #region Collection
            /// <summary>
            /// This key is used to hold the list of children library element id strings.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string COLLECTION_CHILDREN_KEY = "children";
            #endregion Collection 

            #region LinkLibraryElementModel

            /// <summary>
            /// The key that will hold the LibraryElementId for the IN libary element
            /// </summary>
            public static readonly string LINK_LIBRARY_ELEMENT_IN_ID_KEY = "link_in_id";

                /// <summary>
                /// The key that will hold the LibraryElementId for the OUT libary element
                /// </summary>
                public static readonly string LINK_LIBRARY_ELEMENT_OUT_ID_KEY = "link_out_id";

            #endregion LinkLibraryElementModel

            #endregion LibraryElementModel

            #region ContentDataModel

            /// <summary>
            /// The key that will hold the actual string data for the library element model.  Used in the factory class
            /// </summary>
            public static readonly string CONTENT_DATA_MODEL_DATA_STRING_KEY = "data_string";

            #endregion ContentDataModel

            #region ElementModel
            #region AreaModel
            /// <summary>
            /// This key is used to hold the points for the area models when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string AREA_MODEL_POINTS_KEY = "points";
            #endregion AreaModel

            #region CollectionElement
            /// <summary>
            /// This key is used to hold the the collection view type for collection element models when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string COLLECTION_ELEMENT_COLLECTION_VIEW_KEY = "collectionview";
            #endregion CollectionElement

            #region VideoElement

            /// <summary>
            /// This key is used to hold the X resolution for the video element models when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string VIDEO_ELEMENT_RESOLUTION_X_KEY = "resolutionX";

            /// <summary>
            /// This key is used to hold the Y resolution for the video element models when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string VIDEO_ELEMENT_RESOLUTION_Y_KEY = "resolutionY";

            /// <summary>
            /// This key is used to hold the data bytes for the video element models when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string VIDEO_ELEMENT_VIDEO_DATA_BYTES = "video";


            #endregion VideoElement

            #region AudioElement
            /// <summary>
            /// This key is used to hold the the audio element file name when represented in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string AUDIO_ELEMENT_FILE_NAME_KEY = "fileName";
            #endregion AudioElement

            #region PdfElement
            /// <summary>
            /// This key is used to hold the the PDF'S page location in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string PDF_ELEMENT_PAGE_LOCATION_KEY = "page_location";
            #endregion PdfElement

            #region ImageElement
            /// <summary>
            /// This key is used to hold the the image element's file path in message form.
            /// This key SHOULD NOT BE A COLUMN IN ANY DATABASE.  
            /// </summary>
            public static readonly string IMAGE_ELEMENT_FILE_PATH_KEY = "filepath";
            #endregion ImageElement

            #endregion ElementModel



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
            Audio,
            Word
        }

        /// <summary>
        /// The access type for library elements and elements.
        /// Public - everyone can see it
        /// Private - only creator can see it
        /// ReadOnly - only creator can edit it. 
        /// </summary>
        public enum AccessType
        {
            /// <summary>
            /// This accesstype means that everyone on your server can see and edit this object
            /// </summary>
            Public, 

            /// <summary>
            /// This accesstype means that only the creator of the object can see or edit this object.
            /// </summary>
            Private,

            /// <summary>
            /// this accesstype is (as of 8/12/16) limited to just Collections.  
            /// It is meant that everyone on your server can see this object, but only the creator can edit it.  
            /// </summary>
            ReadOnly
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
            NewThumbnailRequest,
            UpdateLibraryElementModelRequest,
            SetTagsRequest,
            ChatDialogRequest,
            SubscribeToCollectionRequest,
            UnsubscribeFromCollectionRequest,
            AddInkRequest,
            RemoveInkRequest,
            ChatRequest,
            //below this line should be the new-server-approved requests
            GetContentDataModelRequest,
            CreateNewLibraryElementRequest,
            GetEntireWorkspaceRequest,
            NewElementRequest,
            DeleteLibraryElementRequest,
            DeleteElementRequest,
            CreateNewMetadataRequest,
            DeleteMetadataRequest,
            CreateNewPresentationLinkRequest,
            DeletePresentationLinkRequest,
            UpdatePresentationLinkRequest,
            UpdateMetadataEntryRequest,
            
            /// <summary>
            /// this request type is used to create a search over the library elements.  
            /// </summary>
            SearchRequest,

            /// <summary>
            /// used to update a node.  Should be able to take arbitrary database property keys for updating
            /// </summary>
            ElementUpdateRequest,

            /// <summary>
            /// This request will create a new content AND a default new library element for that content
            /// Therefore this request should be called when someone uploads a new content to the library
            /// </summary>
            CreateNewContentRequest,
            /// <summary>
            /// this request will get you all of the library elements that exist.  
            /// However, no contents will be loaded with this request
            /// </summary>
            GetAllLibraryElementsRequest
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

        /// <summary>
        /// the enum to represent the size of a thumbnail
        /// </summary>
        public enum ThumbnailSize
        {
            Small, Medium, Large
        }

        #endregion Enums

        #region Misc

        /// <summary>
        /// the default file extension as a string for all thumbnails.  
        /// As of 8/8/16, this should work for all thumbnails.
        /// </summary>
        public static readonly string DEFAULT_THUMBNAIL_FILE_EXTENSION = ".jpg";

        /// <summary>
        /// the default file extension for pdf page images.  
        /// </summary>
        public static readonly string DEFAULT_PDF_PAGE_IMAGE_EXTENSION = ".jpg";
        #endregion Misc

        #region staticMethods

        /// <summary>
        /// returns whether the element Type is a region
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsRegionType(NusysConstants.ElementType type)
        {
            return type == NusysConstants.ElementType.AudioRegion || type == NusysConstants.ElementType.ImageRegion || type == NusysConstants.ElementType.VideoRegion ||
                   type == NusysConstants.ElementType.PdfRegion;
        }

        /// <summary>
        /// returns a GUID string.  Should be used when creating ID's on the client or the server side.  
        /// </summary>
        /// <returns></returns>
        public static string GenerateId()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// returns the correct ContentType for a given elementType;
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static ContentType ElementTypeToContentType(ElementType type)
        {
            switch (type)
            {
                case ElementType.Image:
                case ElementType.ImageRegion:
                    return ContentType.Image;
                case ElementType.Video:
                case ElementType.VideoRegion:
                    return ContentType.Video;
                case ElementType.Audio:
                case ElementType.AudioRegion:
                    return ContentType.Audio;
                case ElementType.PDF:
                case ElementType.PdfRegion:
                    return ContentType.PDF;
                case ElementType.Word:
                    return ContentType.Word;
                default:
                    return ContentType.Text;
            }
        }

        /// <summary>
        /// returns the default thumbnail for a given libraryElementModel as a string.  
        /// Pass in the libraryElementModel's library ID and the size of the thumbnail you want.  
        /// Cannot directly be made into a Uri.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetDefaultThumbnailFileName(string libraryElementModelId, ThumbnailSize size)
        {
            return libraryElementModelId + "_" + size.ToString() + "_thumbnail";
        }

        #endregion staticMethods
    }
}