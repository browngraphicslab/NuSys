using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class BaseClient
    {
        /// <summary>
        /// the double hashed username of this user.
        /// </summary>
        public string UserID { get; set; }

        /// <summary>
        /// the list of ten or fewer most recently visited collection Id's
        /// </summary>
        public List<string> LastVisitedCollections { get; set; }

        /// <summary> 
        /// the display name of the current user
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// creates and returns a BaseClient from the database keys in a message. 
        /// 
        /// Static method.
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public static BaseClient CreateFromDatabaseMessage(Message databaseMessage)
        {
            //create the new user based off of database keys
            var user = new BaseClient()
            {
                DisplayName = databaseMessage.GetString(NusysConstants.USERS_TABLE_USER_DISPLAY_NAME_KEY),
                LastVisitedCollections = databaseMessage.GetList<string>(NusysConstants.USERS_TABLE_LAST_TEN_COLLECTIONS_USED_KEY),
                UserID = databaseMessage.GetString(NusysConstants.USERS_TABLE_HASHED_USER_ID_KEY),
            };
            return user;
        }

    }
}
