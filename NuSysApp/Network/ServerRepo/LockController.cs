using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LockController
    {
        private ServerClient _serverClient;
        private Dictionary<string, ILockable> _lockables;
        public LockController(ServerClient serverClient)
        {
            _serverClient = serverClient;
            _lockables = new Dictionary<string, ILockable>();
            serverClient.OnLockAdded += LockAdded;
            serverClient.OnLockRemoved += LockRemoved;
        }

        /// <summary>
        /// Will be called whenever the server notifies of a new lock
        /// Will automatically set the locked property of the ILockable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="id"></param>
        /// <param name="userId"></param>
        private void LockAdded(object sender, string id, string userId)
        {
            if (_lockables.ContainsKey(id))
            {
                var lockable = _lockables[id];
                lockable.IsLocked = true;
                lockable.Lock(userId);
            }
        }

        /// <summary>
        /// Will be called whenever the server notifies of a lock being removed
        /// Will automatically set the locked property of the ILockable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="id"></param>
        private void LockRemoved(object sender, string id)
        {
            if (_lockables.ContainsKey(id))
            {
                var lockable = _lockables[id];
                lockable.IsLocked = false;
                lockable.UnLock();
            }
        }

        /// <summary>
        /// Used to add an item to the lockable dictionary
        /// </summary>
        /// <param name="lockable"></param>
        public void AddLockable(ILockable lockable)
        {
            Debug.Assert(lockable != null);
            _lockables[lockable.Id] = lockable;
        }
    }
}
