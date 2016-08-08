using System;
using System.Collections.Generic;
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
    public class CreateNewLibraryElementRequestArgs
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
        //public Dictionary<string, MetadataEntry> Metadata { get; set; } // TODO put back in

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
    }
}
