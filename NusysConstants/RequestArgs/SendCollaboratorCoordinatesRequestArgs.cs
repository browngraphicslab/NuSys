using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// request args class for sending your coordinates to a collaborator
    /// </summary>
    public class SendCollaboratorCoordinatesRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// REQUIRED: the string user id of the intended reciepient of this client's coordinates
        /// </summary>
        public string RecipientUserId { get; set; }

        /// <summary>
        /// the original sender id.  optional and will be set server-side
        /// </summary>
        public string OriginalSenderId { get; set; }

        /// <summary>
        /// REQUIRED: the string library ID of the collection the sender currently is in
        /// </summary>
        public string CollectionLibraryId { get; set; }

        /// <summary>
        /// REQUIRED: the X coordinate of the current collection transform
        /// </summary>
        public float? XCoordinate { get; set; }

        /// <summary>
        /// REQUIRED: the Y coordinate of the current collection transform
        /// </summary>
        public float? YCoordinate { get; set; }

        /// <summary>
        /// REQUIRED: the camera scale of the current collection transform
        /// </summary>
        public float? CameraScale { get; set; }

        /// <summary>
        /// parameterless constructor just sets the requset type in the base abstract class
        /// </summary>
        public SendCollaboratorCoordinatesRequestArgs() : base(NusysConstants.RequestType.SendCollaboratorCoordinatesRequest){}

        /// <summary>
        /// this should make sure the recipient id is set correctly as well as the necessary collection-coordinates info
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return !string.IsNullOrEmpty(RecipientUserId) && !string.IsNullOrEmpty(CollectionLibraryId) &&
                   YCoordinate != null && XCoordinate != null && CameraScale != null;
        }
    }
}
