using System;
using System.Collections.Generic;
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
    public class CreateNewContentRequestArgs
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
        public CreateNewLibraryElementRequestArgs LibraryElementArgs { get; private set; }

        #region Required


        /// <summary>
        /// REQUIRED EXCEPT FOR COLLECTIONS OR TEXTS.  
        /// the base64 representation of the data
        /// </summary>
        public string DataBytes { get; set; }

        /// <summary>
        /// REQUIRED when type is AUIDO, VIDEO, PDF, OR IMAGE
        /// the file extension to save on the server for mime-type mapping
        /// </summary>
        public string FileExtension { get; set; }

        #endregion Required

    }
}
