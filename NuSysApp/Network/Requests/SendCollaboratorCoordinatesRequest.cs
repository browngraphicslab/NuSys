using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// The request for sending coordinates of a client to another client.  
    /// To create, first fill in a SendCollaboratorCoordinatesRequestArgs class.
    /// </summary>
    public class SendCollaboratorCoordinatesRequest : FullArgsRequest<SendCollaboratorCoordinatesRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// default, from-server-deserializing constructor.  Do not user this constructor.
        /// </summary>
        /// <param name="message"></param>
        public SendCollaboratorCoordinatesRequest(Message message) : base(message) { }

        /// <summary>
        /// intended constructor.  Use by first filling in a SendCollaboratorCoordinatesRequestArgs class.
        /// Then use the nusys network session to await executing this request.
        /// </summary>
        /// <param name="args"></param>
        public SendCollaboratorCoordinatesRequest(SendCollaboratorCoordinatesRequestArgs args) : base(args) { }

        /// <summary>
        /// This will only be called when another client has forwarded this request to me.
        /// This should react accoridngly locally and probably notify the user of this option
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(SendCollaboratorCoordinatesRequestArgs senderArgs, ServerReturnArgsBase returnArgs)
        {
            var collectionLibraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(senderArgs.CollectionLibraryId) as CollectionLibraryElementController;
            if (collectionLibraryElementController == null || senderArgs.CameraScaleX == null ||
                senderArgs.XLocalScaleCenter == null || senderArgs.YLocalScaleCenter == null ||
                senderArgs.YCoordinatePosition == null || senderArgs.XCoordinatePosition == null || senderArgs.CameraScaleY == null)
            {
                //todo alert the user that the collection was invalid, probably because of a ACL's issue.
                return;
            }
            JoinCollection(senderArgs, collectionLibraryElementController);
        }

        /// <summary>
        /// private method to actually join a collection at a specific point
        /// </summary>
        /// <param name="senderArgs"></param>
        private async Task JoinCollection(SendCollaboratorCoordinatesRequestArgs senderArgs, CollectionLibraryElementController collectionLibraryElementController)
        {
            if(collectionLibraryElementController.LibraryElementModel.LibraryElementId != SessionController.Instance.ActiveFreeFormViewer.LibraryElementId) { 
                await SessionController.Instance.EnterCollection(collectionLibraryElementController.LibraryElementModel.LibraryElementId);
            }

            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalPosition = new Vector2(senderArgs.XCoordinatePosition.Value, senderArgs.YCoordinatePosition.Value);
            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScaleCenter = new Vector2((float)senderArgs.XLocalScaleCenter.Value, (float)senderArgs.YLocalScaleCenter.Value);
            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScale = new Vector2((float)senderArgs.CameraScaleX.Value, (float)senderArgs.CameraScaleY.Value);
            SessionController.Instance.SessionView.FreeFormViewer.InvalidateMinimap();
        }
    }
}
