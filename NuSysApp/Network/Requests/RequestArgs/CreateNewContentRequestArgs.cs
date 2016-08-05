using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class CreateNewContentRequestArgs
    {
        /// <summary>
        /// the string title
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// REQUIRED EXCEPT FOR COLLECTIONS OR TEXTS.  
        /// the base64 representation of the data
        /// </summary>
        public string DataBytes { get; set; }

        /// <summary>
        /// REQUIRED when type is AUIDO, VIDEO, PDF, OR IMAGE
        /// the file extension to save on the server for mime-type mapping
        /// </summary>
        public string FileExtensions { get; set; }

        /// <summary>
        /// REQUIRED
        /// the type that the created library element will be
        /// </summary>
        public NusysConstants.ElementType LibraryElementType { get; set; }
    }
}
