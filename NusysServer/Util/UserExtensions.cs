using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Static extensions class for changes to simple classes
    /// </summary>
    public static class UserExtensions
    {
        /// <summary>
        /// Method to get the user id from a NuWebSocketHandler.
        /// The handler must exist in the IdToUsers dict or else this will debug.assert fail
        /// </summary>
        /// <param name="handler"></param>
        /// <returns></returns>
        public static string GetUserId(this NuWebSocketHandler handler)
        {
            if (handler == null || !NusysClient.IDtoUsers.ContainsKey(handler))
            {
                return null;
            }
            return NusysClient.IDtoUsers[handler].UserID;
        }
    }
}