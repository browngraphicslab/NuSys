using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Args that should be populated and passes into the CreateNewLibraryElementRequest.
    /// Check each argument's comments for which fields are required.
    /// </summary>
    public class CreateNewLibraryElementRequestArgs : IRequestArgumentable
    {
        /// <summary>
        /// Empty constructor just sets nullable enums and booleans
        /// </summary>
        public CreateNewLibraryElementRequestArgs()
        {
            LibraryElementType = null;
            Favorited = null;
        }

        /// <summary>
        /// the initial access type for the default library element.  
        /// Will default to private if not set;
        /// </summary>
        public NusysConstants.AccessType? AccessType { get; set; }

        /// <summary>
        /// the Library ID of the libraryelement you are trying to create.  
        /// Will create a new one if this is not filled in.
        /// </summary>
        public string LibraryElementId { get; set; }

        /// <summary>
        /// the string title of the library element
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// any initial keywords you want the library element to have
        /// </summary>
        public HashSet<Keyword> Keywords { get; set; }

        /// <summary>
        /// whether or not the newly created library element will be favorited in the library
        /// </summary>
        public bool? Favorited { get; set; }

        /// <summary>
        /// Any initial metadata you want the library element To have
        /// </summary>
        public Dictionary<string, MetadataEntry> Metadata { get; set; } // TODO put back in

        /// <summary>
        /// The base-64 string bytes of the small thumbnail for this new libraryElement
        /// </summary>
        public string Small_Thumbnail_Bytes { get; set; }

        /// <summary>
        /// The base-64 string bytes of the medium thumbnail for this new libraryElement
        /// </summary>
        public string Medium_Thumbnail_Bytes { get; set; }

        /// <summary>
        /// The base-64 string bytes of the large thumbnail for this new libraryElement
        /// </summary>
        public string Large_Thumbnail_Bytes { get; set; }

        #region Required

        /// <summary>
        /// REQUIRED
        /// the contentID for the library element.  
        /// </summary>
        public string ContentId { get; set; }

        /// <summary>
        /// REQUIRED
        /// the type that the created library element will be
        /// </summary>
        public NusysConstants.ElementType? LibraryElementType { get; set; }

        #endregion Required

        public virtual Message PackToRequestKeys()
        {
            var message = new Message();
            

            //debug.asserts for required types
            Debug.Assert(LibraryElementType != null);

            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = LibraryElementType.ToString();

            //set the keywords
            if (Keywords != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_KEYWORDS_KEY] = Keywords;
            }

            //set the title
            if (Title != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TITLE_KEY] = Title;
            }

            //set the favorited boolean
            if (Favorited != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_FAVORITED_KEY] = Favorited.Value;
            }

            //add in thumbnail byte strings
            //small
            if (Small_Thumbnail_Bytes != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_SMALL_ICON_BYTE_STRING_KEY] = Small_Thumbnail_Bytes;
            }

            //medium
            if (Medium_Thumbnail_Bytes != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_MEDIUM_ICON_BYTE_STRING_KEY] = Medium_Thumbnail_Bytes;
            }

            //large
            if (Large_Thumbnail_Bytes != null)
            {
                message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LARGE_ICON_BYTE_STRING_KEY] = Large_Thumbnail_Bytes;
            }
            
            //set the default library element's content ID
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = ContentId ?? SessionController.Instance.GenerateId();

            //set the library element's library Id
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = LibraryElementId ?? SessionController.Instance.GenerateId();

            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_ACCESS_KEY] = (AccessType ?? NusysConstants.AccessType.Private).ToString();


            return message;
        }
    }
}
