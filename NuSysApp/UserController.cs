using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
            Debug.Assert(SessionController.Instance.ElementModelIdToElementController.ContainsKey(controllerId));
            if (!(SessionController.Instance.ElementModelIdToElementController.ContainsKey(controllerId)))
            {
                return;
            }
            ElementController controller = SessionController.Instance.ElementModelIdToElementController[controllerId];

            controller.DropUser(userId);
            // do something to make bubble disappear on screen
            // maybe have elementrenderitem constantly updating the bubbles
        }

        /// <summary>
        /// will return IEnumerable of user ids for people viewing a certain library element;
        /// </summary>
        /// <param name="libraryElementId"></param>
        /// <returns></returns>
        public IEnumerable<string> GetUsersOfLibraryElement(string libraryElementId)
        {
            Debug.Assert(libraryElementId != null);
            return
                ControllerIdToUserIdList.Keys.Where(
                    i =>
                        SessionController.Instance.ElementModelIdToElementController.ContainsKey(i) &&
                        SessionController.Instance.ElementModelIdToElementController[i]?.Model?.LibraryId ==
                        libraryElementId).SelectMany(elementId => ControllerIdToUserIdList[elementId]).ToImmutableHashSet();
        }

        private void UserController_UserAdded(string controllerId, string userId)
        {
            NetworkUser user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            ElementController controller = SessionController.Instance.ElementModelIdToElementController[controllerId];

            controller.AddUser(userId);
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
