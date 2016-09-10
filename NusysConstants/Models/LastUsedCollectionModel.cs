using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// the model class used to represent a last used collection when fetching them from the server.
    /// </summary>
    public class LastUsedCollectionModel
    {
        /// <summary>
        /// the id of the user who last used the collection that this model represents.
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// the stringified date time in terms of server time for when this collection was last used.
        /// </summary>
        public string DateTime { get; set; }

        /// <summary>
        /// the string library element ID of the collection that this model represents.
        /// </summary>
        public string CollectionId { get; set; }


        /// <summary>
        /// Method used to populate this model class directly from the message returned from the database.
        /// </summary>
        /// <param name="message"></param>
        public void UnPackFromDatabaseMessage(Message message)
        {
            UserId = message.GetString(NusysConstants.LAST_USED_COLLECTIONS_TABLE_USER_ID, UserId);
            DateTime = message.GetString(NusysConstants.LAST_USED_COLLECTIONS_TABLE_LAST_USED_DATE, DateTime);
            CollectionId = message.GetString(NusysConstants.LAST_USED_COLLECTIONS_TABLE_COLLECTION_LIBRARY_ID, CollectionId);
        }
    }
}
