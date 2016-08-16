using System.Diagnostics;
using NusysIntermediate;

namespace NuSysApp
{
    public class UpdateContentRequestArgs : IRequestArgumentable
    {
        /// <summary>
        /// REQUIRED. Holds the content id of the content to be updated
        /// </summary>
        public string ContentId { get; set; }

        /// <summary>
        /// REQUIRED. Holds the content type of the content to be updated
        /// </summary>
        public NusysConstants.ContentType ContentType { get; set; }

        /// <summary>
        /// REQUIRED. Holds the updated content that will replace the old content.
        /// </summary>
        public string UpdatedContent { get; set; }

        /// <summary>
        /// Returns a message populated with the data held in this instance. Also checks that the message has everything it needs
        /// </summary>
        /// <returns></returns>
        public Message PackToRequestKeys()
        {
            Debug.Assert(ContentId != null);
            Debug.Assert(ContentType != null);
            Debug.Assert(UpdatedContent != null);
            Message message = new Message();
            message[NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_ID_KEY] = ContentId;
            message[NusysConstants.UPDATE_CONTENT_REQUEST_CONTENT_TYPE_KEY] = ContentType.ToString();
            message[NusysConstants.UPDATE_CONTENT_REQUEST_UPDATED_CONTENT_KEY] = UpdatedContent;
            return message;
        }
    }
}