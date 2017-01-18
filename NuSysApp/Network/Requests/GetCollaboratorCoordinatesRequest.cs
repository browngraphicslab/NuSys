using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Request used to ask for the coordinates of another user
    /// </summary>
    public class GetCollaboratorCoordinatesRequest : FullArgsRequest<GetCollaboratorCoordinatesRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// default constructor for derserializing from the server
        /// </summary>
        /// <param name="message"></param>
        public GetCollaboratorCoordinatesRequest(Message message) : base(message){}

        /// <summary>
        /// request constructor takes in a fully-populated GetCollaboratorCoordinatesRequestArgs class.
        /// After constructing this request, use the nusys network session to async await this request's execution
        /// </summary>
        /// <param name="args"></param>
        public GetCollaboratorCoordinatesRequest(GetCollaboratorCoordinatesRequestArgs args) : base(args){}

        /// <summary>
        /// The override will decide whether to respond to the original sender with this client's coordinates
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(GetCollaboratorCoordinatesRequestArgs senderArgs, ServerReturnArgsBase returnArgs)
        {
            var request = new SendCollaboratorCoordinatesRequest(new SendCollaboratorCoordinatesRequestArgs()
            {
                CollectionLibraryId = SessionController.Instance.ActiveFreeFormViewer.LibraryElementId,
                RecipientUserId = senderArgs.OriginalSenderId,
                XCoordinatePosition = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalPosition.X,
                YCoordinatePosition = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalPosition.Y,
                YLocalScaleCenter = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScaleCenter.Y,
                XLocalScaleCenter = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScaleCenter.X,
                CameraScaleX = SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScale.X
            });
            SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }
    }
}
