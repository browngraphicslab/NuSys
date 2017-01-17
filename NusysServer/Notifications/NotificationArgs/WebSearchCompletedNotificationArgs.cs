using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NusysServer
{
    /// <summary>
    /// Notification args class for when the web search has completed.  
    /// Should contain  the original search term and a list of found library element model ID's
    /// </summary>
    public class WebSearchCompletedNotificationArgs : INotificationArgumentable
    {
        /// <summary>
        /// The origin search that this is notiying completion of
        /// </summary>
        public string OriginalSearch { get; set; }

        /// <summary>
        /// list of library element ids made from this search
        /// </summary>
        public List<string> LibraryElementIds { get; set; }

        /// <summary>
        /// This just packs the outgoing message
        /// </summary>
        /// <returns></returns>
        public Message PackToNotificationKeys()
        {
            var m = new Message();

            m[NusysConstants.WEB_SEARCH_COMPLETED_NOTIFICATION_SEARCH_STRING_KEY] = OriginalSearch;
            if (LibraryElementIds != null)
            {
                m[NusysConstants.WEB_SEARCH_COMPLETED_NOTIFICATION_LIBRARY_IDS_KEY] = JsonConvert.SerializeObject(LibraryElementIds);
            }
            else
            {
                m[NusysConstants.WEB_SEARCH_COMPLETED_NOTIFICATION_LIBRARY_IDS_KEY] = JsonConvert.SerializeObject(new List<string>());
            }
            return m;
        }
    }
}