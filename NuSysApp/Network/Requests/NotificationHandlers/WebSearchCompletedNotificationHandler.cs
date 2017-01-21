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
    /// Notification handler class for when a web search has completed
    /// </summary>
    public class WebSearchCompletedNotificationHandler : NotificationHandler
    {
        /// <summary>
        /// the notifications list of library element ids after being selected and changed to library element controllers
        /// </summary>
        private IEnumerable<LibraryElementController> libraryElements;

        /// <summary>
        /// string of search term
        /// </summary>
        private string _searchTerm;

        /// <summary>
        /// This handle notication method should simpy create a chatbot chat telling you of the completed search request
        /// </summary>
        /// <param name="message"></param>
        public override void HandleNotification(Message message)
        {
            Debug.Assert(message.ContainsKey(NusysConstants.WEB_SEARCH_COMPLETED_NOTIFICATION_SEARCH_STRING_KEY));
            Debug.Assert(message.ContainsKey(NusysConstants.WEB_SEARCH_COMPLETED_NOTIFICATION_LIBRARY_IDS_KEY));

            var originalSearch = message.GetString(NusysConstants.WEB_SEARCH_COMPLETED_NOTIFICATION_SEARCH_STRING_KEY);
            _searchTerm = originalSearch;
            var libraryElementIds = message.GetList<string>(NusysConstants.WEB_SEARCH_COMPLETED_NOTIFICATION_LIBRARY_IDS_KEY);

            libraryElements =
                libraryElementIds.Select(
                        id => SessionController.Instance.ContentController.GetLibraryElementController(id))
                    .Where(i => i != null);

            SessionController.Instance.NuSessionView.Chatbox.AddFunctionalChat(NetworkUser.ChatBot, 
                $"Your web search for '{originalSearch}' has completed.  Click on this message for more information!", ChatClickedCallback);

            SessionController.Instance.NuSessionView.IncrementChatNotifications();
        }

        /// <summary>
        /// private method to make a pop up appear after chat is clicked
        /// </summary>
        /// <param name="item"></param>
        /// <param name="pointer"></param>
        private void ChatClickedCallback(InteractiveBaseRenderItem item, CanvasPointer pointer)
        {
            //miranda put your function here and use libraryElements
            SessionController.Instance.NuSessionView.ShowSearchResultPopup(libraryElements.ToList(), _searchTerm);
        }
    }
}
