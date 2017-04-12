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
    /// this request class should be used to upload images to be used as the background of shaped collections. 
    /// In Reality, this could be used to upload any image with a random unique ID and be saved on the server.
    /// </summary>
    public class UploadCollectionBackgroundImageRequest : FullArgsRequest<UploadCollectionBackgroundImageServerRequestArgs, UploadCollectionBackgroundImageReturnArgs>
    {
        /// <summary>
        /// Constructor takes in the args class.
        /// To use this request, create the request args class, fill in the required values, and then tell the nusysNetworkSession to execute this request.
        /// </summary>
        /// <param name="args"></param>
        public UploadCollectionBackgroundImageRequest(UploadCollectionBackgroundImageServerRequestArgs args): base(args){}

        /// <summary>
        /// Method to be called AFTER A SUCCESSFULY REQUEST that returns the string url of the new image
        /// </summary>
        /// <returns></returns>
        public string GetReturnedImageUrl()
        {
            return ReturnArgs.Url;
        }

        /// <summary>
        /// This should never be called because the server shouldn't forward this request to other clients.
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(UploadCollectionBackgroundImageServerRequestArgs senderArgs, UploadCollectionBackgroundImageReturnArgs returnArgs)
        {
            Debug.Assert(false, "this shouldn't happen");
        }
    }
}
