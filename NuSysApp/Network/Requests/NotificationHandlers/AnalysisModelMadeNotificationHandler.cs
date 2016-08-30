using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// the notification handler for when analysis model is made.  
    /// Should alert the user and allow the analysis model to be fetched if needed.
    /// </summary>
    public class AnalysisModelMadeNotificationHandler : NotificationHandler
    {
        /// <summary>
        /// this handle notifiation should alert the user via chatbot and should remove any current entries stored locally for the analysis model
        /// </summary>
        /// <param name="message"></param>
        public override void HandleNotification(Message message)
        {
            Debug.Assert(message.ContainsKey(NusysConstants.ANALYSIS_MODEL_MADE_NOTIFICATION_CONTENT_ID_KEY));

            var contentId = message.GetString(NusysConstants.ANALYSIS_MODEL_MADE_NOTIFICATION_CONTENT_ID_KEY);

            SessionController.Instance.ContentController.RemoveAnalysisModel(contentId);

            UITask.Run(delegate
            {
                // Obtains the chatbox
                var cBox = SessionController.Instance.SessionView?.GetChatBox();

                // if the chat box is null, the sessionview hasn't been instantiated yet 
                if (cBox != null)
                {

                    var elements = SessionController.Instance.ContentController.IdList.Where(id =>SessionController.Instance.ContentController.GetLibraryElementModel(id)?
                                    .ContentDataModelId == contentId).Select(id => SessionController.Instance.ContentController.GetLibraryElementModel(id));
                    var titles = SessionController.Instance.ContentController.IdList.Where(id => SessionController.Instance.ContentController.GetLibraryElementModel(id)?
                                    .ContentDataModelId == contentId).Select(id => SessionController.Instance.ContentController.GetLibraryElementModel(id).Title);

                    if (elements.Any())
                    {

                        var chatText =$"Your {NusysConstants.ElementTypeToContentType(elements.First().Type).ToString()} library element{(elements.Count() > 1 ? "s" : "")} {string.Join(", ", titles)} {(elements.Count() > 1 ? "have" : "has")} more information available!";

                        // update the text in the chat box
                        cBox.AppendText(new NetworkUser("ChatBot") {DisplayName = "ChatBot"}, chatText);
                    }
                }
            });
        }
    }
}
