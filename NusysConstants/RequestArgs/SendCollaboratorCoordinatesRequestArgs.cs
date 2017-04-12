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
        /// public bool used to indicate whether the chatbot should ask the reciever before joining the collection defined by these args
        /// </summary>
        public bool AskBeforeJoining { get; set; } = false;

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
        public float? XCoordinatePosition { get; set; }

        /// <summary>
        /// REQUIRED: the Y coordinate of the current collection transform
        /// </summary>
        public float? YCoordinatePosition { get; set; }

        /// <summary>
        /// REQUIRED: the X coordinate of the current collection scale center
        /// </summary>
        public float? XLocalScaleCenter { get; set; }


        /// <summary>
        /// REQUIRED: the Y coordinate of the current collection scale center
        /// </summary>
        public float? YLocalScaleCenter { get; set; }

        /// <summary>
        /// REQUIRED: the camera X component scale of the current collection transform
        /// </summary>
        public float? CameraScaleX { get; set; }

        /// <summary>
        /// REQUIRED: the camera Y component scale of the current collection transform
        /// </summary>
        public float? CameraScaleY { get; set; }

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
                   YCoordinatePosition != null && XCoordinatePosition != null 
                   && CameraScaleX != null && CameraScaleY != null && YLocalScaleCenter != null && XLocalScaleCenter != null;
        }
    }
}
