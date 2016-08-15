using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    class UpdatePresentationLinkRequestArgs : IRequestArgumentable
    {
        /// <summary>
        /// Updated in element id of the presentation link
        /// </summary>
        public string InId { get; set; }

        /// <summary>
        /// Updated out element id of the presentation link
        /// </summary>
        public string OutId { get; set; }

        /// <summary>
        /// ID of the linke to update
        /// </summary>
        public string LinkId { get; set; }

        /// <summary>
        /// Updated annotation the presentation link
        /// </summary>
        public string Annotation { get; set; }

        /// <summary>
        /// Returns message based on the variables
        /// </summary>
        /// <returns></returns>
        public Message PackToRequestKeys()
        {
            Message message = new Message();
            message[NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_ID_KEY] = LinkId;
            if (OutId == null)
            {
                message[NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY] = InId;
            }
            if (InId == null)
            {
                message[NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY] = OutId;
            }
            if (Annotation != null)
            {
                message[NusysConstants.UPDATE_PRESENTATION_LINK_REQUEST_ANNOTATION_KEY] = Annotation;
            }
            return message;
        }
    }
}
