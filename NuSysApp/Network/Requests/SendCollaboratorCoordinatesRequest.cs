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
        /// static bool for knowing if we are currently already joining a collection.
        /// This is needed in case multiple requests happen too quickly
        /// </summary>
        public static bool _currentlyJoining;

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
                SessionController.Instance.NuSessionView.Chatbox.AddChat(NetworkUser.ChatBot, 
                    "Due to an access issue, you are unable to join " + SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[senderArgs.OriginalSenderId]+"'s current workspace.");
                return;
            }

            if (senderArgs.AskBeforeJoining)
            {
                AddChatbotQuery(senderArgs, collectionLibraryElementController);
            }
            else
            {
                JoinCollection(senderArgs, collectionLibraryElementController);
            }
        }

        /// <summary>
        /// private method to have the chatbot ask the user before joining
        /// </summary>
        /// <param name="senderArgs"></param>
        private void AddChatbotQuery(SendCollaboratorCoordinatesRequestArgs senderArgs, CollectionLibraryElementController collectionLibraryElementController)
        {
            SessionController.Instance.NuSessionView.Chatbox.AddFunctionalChat(NetworkUser.ChatBot, SessionController.Instance.NuSysNetworkSession.UserIdToDisplayNameDictionary[senderArgs.OriginalSenderId] + 
                " has invited you to join the collection "+ collectionLibraryElementController.CollectionModel.Title+". Click this message to accept. ",
                (item, pointer) =>
                {
                    JoinCollection(senderArgs, collectionLibraryElementController);
                });
            SessionController.Instance.NuSessionView.IncrementChatNotifications();
        }

        /// <summary>
        /// private method to actually join a collection at a specific point
        /// </summary>
        /// <param name="senderArgs"></param>
        private static async Task JoinCollection(SendCollaboratorCoordinatesRequestArgs senderArgs, CollectionLibraryElementController collectionLibraryElementController)
        {
            if (_currentlyJoining)
            {
                return;
            }
            _currentlyJoining = true;
            if (collectionLibraryElementController.LibraryElementModel.LibraryElementId != SessionController.Instance.ActiveFreeFormViewer.LibraryElementId) { 
                await SessionController.Instance.EnterCollection(collectionLibraryElementController.LibraryElementModel.LibraryElementId);
            }

            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalPosition = new Vector2(senderArgs.XCoordinatePosition.Value, senderArgs.YCoordinatePosition.Value);
            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScaleCenter = new Vector2(senderArgs.XLocalScaleCenter.Value, senderArgs.YLocalScaleCenter.Value);
            SessionController.Instance.SessionView.FreeFormViewer.CurrentCollection.Camera.LocalScale = new Vector2(senderArgs.CameraScaleX.Value, senderArgs.CameraScaleY.Value);
            SessionController.Instance.SessionView.FreeFormViewer.InvalidateMinimap();
            _currentlyJoining = false;
        }
    }
}
