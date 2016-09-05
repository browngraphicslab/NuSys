using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the request args class that should be used to create ever new image library element.
    /// It simply adds an aspect ratio option to the request to save the aspect ration of the image library element.
    /// </summary>
    public class CreateNewImageLibraryElementRequestArgs : CreateNewLibraryElementRequestArgs
    {
        public CreateNewImageLibraryElementRequestArgs()
        {
            LibraryElementType = NusysConstants.ElementType.Image;
        }

        /// <summary>
        /// Nullable double aspect ratio of the image element.
        /// Will not send the aspect ratio with the request if this is null.
        /// This aspect ratio is not required but should be filled in otherwise using this reuqest args class is kinda pointless.
        /// </summary>
        public double? AspectRatio { get; set; }

        /// <summary>
        /// the nullable double for the normalized x coordinate of the image.
        /// Will default to zero if null.
        /// </summary>
        public double? NormalizedX { get; set; }

        /// <summary>
        /// the nullable double for the normalized y coordinate of the image.
        /// Will default to zero if null.
        /// </summary>
        public double? NormalizedY { get; set; }

        /// <summary>
        /// the nullable double for the normalized width of the image.
        /// Will default to 1 if null.
        /// </summary>
        public double? NormalizedWidth { get; set; }

        /// <summary>
        /// the nullable double for the normalized height of the image.
        /// Will default to 1 if null.
        /// </summary>
        public double? NormalizedHeight { get; set; }

        /// <summary>
        /// this override PackToRequestKeys method will add the aspect ratio double if it is not null.
        /// </summary>
        /// <returns></returns>
        public override Message PackToRequestKeys()
        {
            var message = base.PackToRequestKeys();
            if (AspectRatio != null)
            {
                message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_ASPECT_RATIO_KEY] = AspectRatio.Value;
            }
            message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_HEIGHT] = NormalizedHeight ?? 1;
            message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_NORMALIZED_WIDTH] = NormalizedWidth ?? 1;
            message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_X] = NormalizedX ?? 0;
            message[NusysConstants.NEW_IMAGE_LIBRARY_ELEMENT_REQUEST_TOP_LEFT_Y] = NormalizedY ?? 0;
            return message;
        }
    }
}
