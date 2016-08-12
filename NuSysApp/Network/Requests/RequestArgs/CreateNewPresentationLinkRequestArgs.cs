using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp.Network.Requests.RequestArgs
{
    class CreateNewPresentationLinkRequestArgs : IRequestArgumentable
    {
        public string InId { get; set; }
        public string OutId { get; set; }
        public string LinkId { get; set; }
        public string ParentCollectionId { get; set; }
        public string Annotation { get; set; }
        public Message PackToRequestKeys()
        {
            Message message = new Message();
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY] = ParentCollectionId;
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY] = ParentCollectionId;
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY] = ParentCollectionId;
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_ID_KEY] = ParentCollectionId;
            if (Annotation != null)
            {
                message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_ANNOTATION_KEY] = ParentCollectionId;
            }
            return message;
        }
    }
}
