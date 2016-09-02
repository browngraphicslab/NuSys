using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the Request args class that should be used to create a new image library element.
    /// This request args class should only add an aspect ratio property to the request.
    /// </summary>
    public class CreateNewVideoLibraryElementRequestArgs : CreateNewLibraryElementRequestArgs
    {
        /// <summary>
        /// The nullable double aspect ratio of the video library element you are creating. 
        /// This is not required, but should probably be set if you are using this request args at all.
        /// will not send the double ot the server if it is null.
        /// </summary>
        public double? AspectRatio { get; set; }

        /// <summary>
        /// the override packing to request keys method for this reuqest args.
        /// Should simply add the aspect ratio double if it is not null.  
        /// </summary>
        /// <returns></returns>
        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();
            if (AspectRatio != null)
            {
                message[NusysConstants.NEW_VIDEO_LIBRARY_ELEMENT_REQUEST_ASPECT_RATIO_KEY] = AspectRatio.Value;
            }
            return message;
        }
    }
}
