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
    /// the arguments class that should be populated when trying to create a new content.
    /// Along with the new content, a default library element will be created encapsulating the entire newly-created content.
    /// Many of the properties you fill in may be going towards that default library element rather than the content.
    /// </summary>
    public class CreateNewContentRequestArgs : IRequestArgumentable
    {
        /// <summary>
        /// empty constructor just sets the CreateNewLibraryElementRequestArgs
        /// </summary>
        public CreateNewContentRequestArgs()
        {
            LibraryElementArgs = new CreateNewLibraryElementRequestArgs();
        }

        /// <summary>
        /// the id for the content you want to create.  
        /// Will create a new one if this is not set
        /// </summary>
        public string ContentId { get; set; }

        /// <summary>
        /// the args to use to fill in the properties of the default library element;  
        /// THE CONTENT ID OF THE LIBRARY ELEMENT ARGS WILL BE IGNORED
        /// </summary>
        public CreateNewLibraryElementRequestArgs LibraryElementArgs { get; set; }

        #region Required


        /// <summary>
        /// REQUIRED EXCEPT FOR COLLECTIONS OR TEXTS OR LINKS.  
        /// the base64 representation of the data
        /// </summary>
        public string DataBytes { get; set; }

        /// <summary>
        /// REQUIRED when type is AUIDO, VIDEO, PDF, OR IMAGE
        /// the file extension to save on the server for mime-type mapping
        /// </summary>
        public string FileExtension { get; set; }

        #endregion Required

        /// <summary>
        /// packs all the required keys for creating a new ContentRequest
        /// </summary>
        /// <returns></returns>
        public virtual Message PackToRequestKeys()
        {
            var message = LibraryElementArgs.PackToRequestKeys();//important to have this on top to override its contetn id

            Debug.Assert(LibraryElementArgs.LibraryElementType != null);

            if (LibraryElementArgs.LibraryElementType != NusysConstants.ElementType.Collection &&
                LibraryElementArgs.LibraryElementType != NusysConstants.ElementType.Text &&
                LibraryElementArgs.LibraryElementType != NusysConstants.ElementType.Link)
            {
                Debug.Assert(DataBytes != null);
                message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = DataBytes;
            }
            else
            {
                message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_DATA_BYTES] = null;
            }

            //if the requsted content will need a specific filetype server-side, require the file extension
            if (LibraryElementArgs.LibraryElementType == NusysConstants.ElementType.Audio ||
                LibraryElementArgs.LibraryElementType == NusysConstants.ElementType.Video ||
                LibraryElementArgs.LibraryElementType == NusysConstants.ElementType.PDF ||
                LibraryElementArgs.LibraryElementType == NusysConstants.ElementType.Image)
            {
                Debug.Assert(FileExtension != null);
                message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_FILE_EXTENTION] = FileExtension;
            }

            //Set the contentType based on the ElementType of the default library element
            message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_TYPE_KEY] = NusysConstants.ElementTypeToContentType(LibraryElementArgs.LibraryElementType.Value).ToString();

            //set the libraryElementType
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_TYPE_KEY] = LibraryElementArgs.LibraryElementType.ToString();

            //Set the contentId based on the given id, or create one 
            message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY] = ContentId ?? SessionController.Instance.GenerateId();

            //set the default library element's content ID and override the libraryElementArgs's content id
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_CONTENT_ID_KEY] = message[NusysConstants.CREATE_NEW_CONTENT_REQUEST_CONTENT_ID_KEY];

            //set the library element's library Id
            message[NusysConstants.NEW_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = LibraryElementArgs.LibraryElementId ?? SessionController.Instance.GenerateId();

            return message;
        }

    }
}
