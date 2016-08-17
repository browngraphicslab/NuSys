using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{

    public class CreateNewPresentationLinkRequestArgs : IRequestArgumentable
    {
        /// <summary>
        /// REQUIRED. 
        /// The id of the element view model this presentation link was dragged from
        /// </summary>
        public string ElementViewModelInId { get; set; }
        /// <summary>
        /// REQUIRED. 
        /// The id of the element view model this presentation link was dragged to
        /// </summary>
        public string ElementViewModelOutId { get; set; }
        /// <summary>
        /// A unique id for the presentation link
        /// </summary>
        public string LinkId { get; set; }
        /// <summary>
        /// REQUIRED. 
        /// The unique id for the collection the presentation link was created in
        /// </summary>
        public string ParentCollectionId { get; set; }
        /// <summary>
        /// Textual annotation for the presentation link
        /// </summary>
        public string Annotation { get; set; }
        /// <summary>
        /// This packs all of the variables into the message that is send to the server
        /// </summary>
        /// <returns></returns>
        public Message PackToRequestKeys()
        {
            Debug.Assert(ElementViewModelInId != null, "This is a required field, it should be populated when the link is created");
            Debug.Assert(ElementViewModelOutId != null, "This is a required field, it should be populated when the link is created");
            Debug.Assert(ParentCollectionId != null, "This is a required field, it should be populated when the link is created");

            LinkId = LinkId ?? SessionController.Instance.GenerateId();


            Message message = new Message();
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_PARENT_COLLECTION_ID_KEY] = ParentCollectionId;
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_IN_ID_KEY] = ElementViewModelInId;
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_OUT_ID_KEY] = ElementViewModelOutId;
            message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_LINK_ID_KEY] = LinkId;
            if (Annotation != null)
            {
                message[NusysConstants.CREATE_NEW_PRESENTATION_LINK_REQUEST_ANNOTATION_KEY] = ParentCollectionId;
            }
            return message;
        }
    }
}
