using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Serializable request arguments class used to send an image to be saved on the server for a collection background.
    /// </summary>
    public class UploadCollectionBackgroundImageServerRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// constructor just sets the request type.
        /// </summary>
        public UploadCollectionBackgroundImageServerRequestArgs() : base(NusysConstants.RequestType.UploadCollectionImageRequest) { }

        /// <summary>
        /// REQUIRED: The file extension for the image to be saved.  Example: ".jpg"
        /// </summary>
        public string FileExtension = null;

        /// <summary>
        /// This will be used as part of the Url for the image.  
        /// This will almost definitely NOT need to be set.
        /// </summary>
        public string UniqueId = NusysConstants.GenerateId();

        /// <summary>
        /// REQUIRED: The byte array for the actual image.
        /// </summary>
        public byte[] ImageBytes = null;

        /// <summary>
        /// This overrided method just returns whether the File Extension and the Image Bytes are not null;
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return ImageBytes != null && FileExtension != null;
        }
    }
}
