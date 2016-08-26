using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the debouncing dictionary sub class should be used for all content data model updates.
    /// This class should only be used in the content data controller.
    /// For more information about debouncing dictionaries in general, check out the description of the base class.
    /// </summary>
    public class ContentDebouncingDictionary : DebouncingDictionary
    {
        /// <summary>
        /// private instance variable for storing the type of content we are updating;
        /// </summary>
        private NusysConstants.ContentType _contentType;

        /// <summary>
        /// constructor takes in the  Id of the contentDataModel that will be getting updated from this debouncing dictionary.  
        /// THe constructor also takes in the ContentType of the content data model that this debouncing dictinary will be updating. 
        /// This class overrides the usual DebouncingDictionary "Add" method. Instead, you can use 'AddLatestContent'.
        /// </summary>
        /// <param name="contentDataModelId"></param>
        /// <param name="contentType"></param>
        public ContentDebouncingDictionary(string contentDataModelId, NusysConstants.ContentType contentType) : base(contentDataModelId)
        {
            _contentType = contentType;
        }

        /// <summary>
        /// this override simply updates the server with the latest values. 
        /// The values are stored in the message argument.  
        /// This method will just send a UpdateContentRequest, used to update the content Data model;
        /// </summary>
        /// <param name="message"></param>
        /// <param name="shouldSave"></param>
        /// <param name="objectId"></param>
        /// <returns></returns>
        protected override async Task SendToServer(Message message, bool shouldSave, string objectId)
        {
            var args = new UpdateContentRequestArgs();
            args.ContentId = objectId;
            args.ContentType = _contentType;
            args.UpdatedContent = message.GetString(NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY);
            var request = new UpdateContentRequest(args);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }

        /// <summary>
        /// this method is an wrapper for the base class's 'Add' method.
        /// This should be used to update the debouncing dictionary with the latest string content of the contentDataModel;
        /// This will call that base method and add the latest content String with the NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY key.
        /// This or the add method will work, but this method is much easier. 
        /// </summary>
        /// <param name="latestContent"></param>
        public void AddLatestContent(string latestContent)
        {
            base.Add(NusysConstants.CONTENT_DATA_MODEL_DATA_STRING_KEY, latestContent);
        }
    }
}
