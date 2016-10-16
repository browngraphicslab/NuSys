using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class UserController : IDisposable
    {
        public delegate void UserHandler(String controllerId, String userId);

        public event UserHandler UserAdded;
        public event UserHandler UserRemoved;

        /// <summary>
        /// Dictionary to map all the ElementControllers to a list of the users editing the controllers
        /// key: ElementController id, val: list of NetworkUser ids -> NusysNetworkSession has dict of userid to NetworkUser
        /// </summary>
        private ConcurrentDictionary<string, List<string>> ControllerIdToUserIdList = new ConcurrentDictionary<string, List<string>>();

        public UserController()
        {
            this.UserAdded += UserController_UserAdded;
            this.UserRemoved += UserController_UserRemoved;
        }

        private void UserController_UserRemoved(string controllerId, string userId)
        {
            NetworkUser user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            ElementController controller = SessionController.Instance.IdToControllers[controllerId];

            user.SetNodeCurrentlyEditing(null);
            // do something to make bubble disappear on screen
        }

        private void UserController_UserAdded(string controllerId, string userId)
        {
            NetworkUser user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            ElementController controller = SessionController.Instance.IdToControllers[controllerId];

            user.SetNodeCurrentlyEditing(controllerId);
            // do something to make bubble appear on screen
        }

        public void AddUser(string controllerId, string userId)
        {
            NetworkUser curr = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            if (ControllerIdToUserIdList.ContainsKey(controllerId))
            {
                ControllerIdToUserIdList[controllerId].Add(userId);
            } else
            {
                ControllerIdToUserIdList.TryAdd(controllerId, new List<string>());
                ControllerIdToUserIdList[controllerId].Add(userId);
            }
            UserAdded?.Invoke(controllerId, userId);
        }

        public void RemoveUser(string controllerId, string userId)
        {
            ControllerIdToUserIdList[controllerId].Remove(userId);
            UserRemoved?.Invoke(controllerId, userId);
        }

        public void ClearUsers()
        { 
            // Do something to clear the thing
        }

        public void Dispose()
        {
            this.UserAdded -= UserController_UserAdded;
            this.UserRemoved -= UserController_UserRemoved;
            ClearUsers();
        }
    }
}
