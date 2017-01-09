using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// the request handler for the UploadCollectionBackgroundImage request. 
    /// </summary>
    public class UploadCollectionBackgroundImageRequestHandler : FullArgsRequestHandler<UploadCollectionBackgroundImageServerRequestArgs, UploadCollectionBackgroundImageReturnArgs>
    {
        /// <summary>
        /// This request handler will NOT forward to any other clients.  
        /// It should simply create the uploaded image and return the url it used.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="senderHandler"></param>
        /// <returns></returns>
        protected override UploadCollectionBackgroundImageReturnArgs HandleArgsRequest(UploadCollectionBackgroundImageServerRequestArgs args, NuWebSocketHandler senderHandler)
        {
            var returnArgs = new UploadCollectionBackgroundImageReturnArgs();
            FileHelper.SaveFileToRoot(args.ImageBytes,args.UniqueId,args.FileExtension);
            returnArgs.Url = Constants.SERVER_ADDRESS + args.UniqueId + args.FileExtension;
            returnArgs.WasSuccessful = true;
            return returnArgs;
        }
    }
}