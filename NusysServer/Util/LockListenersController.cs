using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Class used to track which clients are listening to what lock changes
    /// </summary>
    public class LockListenersController
    {
        /// <summary>
        /// Event fired when this class is disposed
        /// </summary>
        public event EventHandler Disposed;  

        /// <summary>
        /// the private instance variable for the _lockController that this is coupled with.
        /// </summary>
        private LocksController _lockController;

        /// <summary>
        /// Private dictionary mapping the lockable id of a lock to the list of users listening to its changes.
        /// The inner concurrentDictionary should be thought of as a hashSet, in that only the keys are actually important.
        /// </summary>
        private ConcurrentDictionary<string, ConcurrentDictionary<NuWebSocketHandler, byte>> _lockToListeningUsers;

        /// <summary>
        /// private dictinary mapping a user to all the locks they are listening to.
        /// The inner concurrentDictionary should be thought of as a hashSet, in that only the keys are actually important.
        /// </summary>
        private ConcurrentDictionary<NuWebSocketHandler, ConcurrentDictionary<string, byte>> _userToLocksWatching;

        /// <summary>
        /// Constructor takes in a LocksController to listen to
        /// </summary>
        /// <param name="lockController"></param>
        public LockListenersController(LocksController lockController)
        {
            Debug.Assert(lockController != null);
            _lockController = lockController;

            _lockToListeningUsers = new ConcurrentDictionary<string, ConcurrentDictionary<NuWebSocketHandler, byte>>();
            _userToLocksWatching = new ConcurrentDictionary<NuWebSocketHandler, ConcurrentDictionary<string, byte>>();

            _lockController.Disposed += LockControllerOnDisposed;
            _lockController.LockAdded += LockControllerOnLockAdded;
            _lockController.LockRemoved += LockControllerOnLockRemoved;
            NuWebSocketHandler.ClientDropped += NuWebSocketHandlerOnClientDropped;
        }

        /// <summary>
        /// private method to actually remove a listened-to lock.
        /// Will fire an exception if the lock is null, empty, or if that lock isn't being listened to by that user
        /// </summary>
        /// <param name="lockableId"></param>
        private void RemoveInternalListenedToLock(string lockableId, NuWebSocketHandler handler)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockableId) && handler != null);
            if (string.IsNullOrEmpty(lockableId) || handler == null)
            {
                throw new Exception("Requested lock listen deletion had null string or handler");
            }
            if (!_lockToListeningUsers.ContainsKey(lockableId) ||
                !_lockToListeningUsers[lockableId].ContainsKey(handler))
            {
                throw new Exception("The requested lock listen deletion wasn't valid");
            }

            //at this point we know the id is in the _lockToListeningUsers dict and it contains the handler
            if (_lockToListeningUsers[lockableId].Count() > 1)
            {
                byte outbyte;
                _lockToListeningUsers[lockableId].TryRemove(handler, out outbyte);
            }
            else
            {
                ConcurrentDictionary<NuWebSocketHandler, byte> outDict;
                _lockToListeningUsers.TryRemove(lockableId,out outDict);
            }
            Debug.Assert(_userToLocksWatching.ContainsKey(handler) && _userToLocksWatching[handler].ContainsKey(lockableId));
            if (_userToLocksWatching.ContainsKey(handler) && _userToLocksWatching[handler].ContainsKey(lockableId))
            {
                if (_userToLocksWatching[handler].Keys.Count() > 1)
                {
                    byte outByte;
                    _userToLocksWatching[handler].TryRemove(lockableId, out outByte);
                }
                else
                {
                    ConcurrentDictionary<string, byte> outDict;
                    _userToLocksWatching.TryRemove(handler, out outDict);
                }
            }
        }

        /// <summary>
        /// public method to remove a lock and the listening user.  Will return whether it was a successful stop to the listening.
        /// </summary>
        /// <param name="lockableId"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool RemoveListeningLock(string lockableId, NuWebSocketHandler handler)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockableId) && handler != null);
            if (string.IsNullOrEmpty(lockableId) || handler == null)
            {
                throw new Exception("Requested lock listen removal had null string or handler");
            }
            if (!_lockToListeningUsers.ContainsKey(lockableId) || !_lockToListeningUsers[lockableId].ContainsKey(handler))
            {
                return false;
            }
            RemoveInternalListenedToLock(lockableId, handler);
            return true;
        }

        /// <summary>
        /// Method to call to add a user to be listening to a lock change
        /// </summary>
        /// <param name="lockableId"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool AddUserToListenToLock(string lockableId, NuWebSocketHandler handler)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockableId) && handler != null);
            if (string.IsNullOrEmpty(lockableId) || handler == null)
            {
                throw new Exception("Requested lock listen addition had null string or handler");
            }
            if (_lockToListeningUsers.ContainsKey(lockableId) && _lockToListeningUsers[lockableId].ContainsKey(handler))
            {
                return false;
            }
            if (!_lockToListeningUsers.ContainsKey(lockableId))
            {
                _lockToListeningUsers.TryAdd(lockableId, new ConcurrentDictionary<NuWebSocketHandler, byte>());
            }
            _lockToListeningUsers[lockableId].TryAdd(handler, 0);
            if (!_userToLocksWatching.ContainsKey(handler))
            {
                _userToLocksWatching.TryAdd(handler, new ConcurrentDictionary<string, byte>());
            }
            _userToLocksWatching[handler].TryAdd(lockableId, 0);
            return true;
        }

        /// <summary>
        /// Event handler called whenever a client dropps
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NuWebSocketHandlerOnClientDropped(object sender, NuWebSocketHandler handler)
        {
            if (_userToLocksWatching.ContainsKey(handler))
            {
                foreach (var lockId in _userToLocksWatching[handler].Keys.ToArray())
                {
                    RemoveInternalListenedToLock(lockId,handler);
                }
            }
        }
        
        /// <summary>
        /// private method to update those listening to the changes of the specified lock using a notification.
        /// Shouldn't send the notification to the person who hold the lock, since they should already know
        /// </summary>
        /// <param name="lockableId"></param>
        private void NotifyUsersOfLockChange(string lockableId)
        {
            if (_lockToListeningUsers.ContainsKey(lockableId))
            {
                foreach (var user in _lockToListeningUsers[lockableId].Keys.ToArray())
                {
                    var currentLockHolder = _lockController.GetUserFromLock(lockableId);
                    Debug.Assert(user != null);

                    //If the user that has the lock is the one we are about to notify, dont notify him since he would already know
                    if (user == currentLockHolder)
                    {
                        continue;
                    }
                    
                    var notification = new LockHolderChangedNotification(new LockHolderChangedNotificationArgs()
                    {
                        HolderUserId = currentLockHolder?.GetUserId(),
                        LockableId = lockableId
                    });
                    user.Notify(notification);
                }
            }
        }

        /// <summary>
        /// Event handler called whenever the locksController removes a lock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="lockableId"></param>
        /// <param name="handler"></param>
        private void LockControllerOnLockRemoved(object sender, string lockableId, NuWebSocketHandler handler)
        {
            NotifyUsersOfLockChange(lockableId);
        }

        /// <summary>
        /// Eventhandler called whenever the lockController adds a lock.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="lockableId"></param>
        /// <param name="handler"></param>
        private void LockControllerOnLockAdded(object sender, string lockableId, NuWebSocketHandler handler)
        {
            NotifyUsersOfLockChange(lockableId);
        }

        /// <summary>
        /// Event handler fired whenever the _lockController disposes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void LockControllerOnDisposed(object sender, EventArgs eventArgs)
        {
            Dispose();
        }

        /// <summary>
        /// Dispose method to set big data structures to null;
        /// </summary>
        public void Dispose()
        {
            _lockController.Disposed -= LockControllerOnDisposed;
            _lockController.LockAdded -= LockControllerOnLockAdded;
            _lockController.LockRemoved -= LockControllerOnLockRemoved;
            NuWebSocketHandler.ClientDropped -= NuWebSocketHandlerOnClientDropped;
            _lockController = null;
            Disposed?.Invoke(this,EventArgs.Empty);
        }
    }
}