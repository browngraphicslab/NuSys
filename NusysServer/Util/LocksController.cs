using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;

namespace NusysServer
{
    /// <summary>
    /// Class for keeping track of what users have what locks.
    /// This should be used to resolve locking conflicts
    /// </summary>
    public class LocksController
    {
        /// <summary>
        /// Event called when this class is disposed
        /// </summary>
        public event EventHandler Disposed; 

        public delegate void LockAddedEventHandler(object sender, string lockableId, NuWebSocketHandler handler);
        public delegate void LockRemovedEventHandler(object sender, string lockableId, NuWebSocketHandler handler);

        /// <summary>
        /// event fired whenever a lock is added.  The lock id and user handler is passed.
        /// The user handler is the handler of the person who now has the lock
        /// </summary>
        public event LockAddedEventHandler LockAdded;

        /// <summary>
        /// Event fired whenever a lock is removed.The passed in handler is the person who gave up the lock.
        /// </summary>
        public event LockRemovedEventHandler LockRemoved;

        /// <summary>
        /// Dictionary that maps a user to all the locks they hold.
        /// The nested concurrent dictionary should be used like a hashset.  The values are 100% meaningless
        /// </summary>
        private ConcurrentDictionary<NuWebSocketHandler, ConcurrentDictionary<string,byte>> _usersToLocks;

        /// <summary>
        /// dictionary mapping the id of a locked item to the network user that holds it.
        /// </summary>
        private ConcurrentDictionary<string, NuWebSocketHandler> _lockToUser;

        /// <summary>
        /// Constructor just listens to events and instantiates the dictionaries
        /// </summary>
        public LocksController()
        {
            _usersToLocks = new ConcurrentDictionary<NuWebSocketHandler, ConcurrentDictionary<string,byte>>();
            _lockToUser = new ConcurrentDictionary<string, NuWebSocketHandler>();
            NuWebSocketHandler.ClientDropped += NuWebSocketHandlerOnClientDropped;
        }

        /// <summary>
        /// Event handler called whenever a client drops
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="nuWebSocketHandler"></param>
        private void NuWebSocketHandlerOnClientDropped(object sender, NuWebSocketHandler nuWebSocketHandler)
        {
            if (_usersToLocks.ContainsKey(nuWebSocketHandler))
            {
                foreach (var lockId in _usersToLocks[nuWebSocketHandler].Keys.ToArray())
                {
                    RemoveLockInternally(lockId);
                }
            }
        }

        /// <summary>
        /// dispose should set the big dictionaries in this class to null and stopped listening to the nuwebsockethandler's events
        /// </summary>
        public void Dispose()
        {
            _usersToLocks = null;
            _lockToUser = null;
            NuWebSocketHandler.ClientDropped -= NuWebSocketHandlerOnClientDropped;
            Disposed?.Invoke(this,EventArgs.Empty);
        }

        /// <summary>
        /// Method to call to remove a lock from the lsit.  
        /// This will throw an exception if the lock id isn't present or if the data structures are corrupted
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private void RemoveLockInternally(string id)
        {
            Debug.Assert(_lockToUser.ContainsKey(id));
            if (_lockToUser.ContainsKey(id))
            {
                throw new Exception("Lock wasn't found for given lockable id");
            }
            NuWebSocketHandler user;
            _lockToUser.TryRemove(id, out user);
            Debug.Assert(user != null);
            if (user == null)
            {
                throw new Exception("User for given lock was null!");
            }
            Debug.Assert(_usersToLocks.ContainsKey(user) && _usersToLocks[user] != null && _usersToLocks[user].ContainsKey(id));
            if(!(_usersToLocks.ContainsKey(user) && _usersToLocks[user] != null && _usersToLocks[user].ContainsKey(id)))
            {
                throw new Exception("Id wasn't found in user's locks when it should've been!");
            }

            //At this point we know at least one id is present, and that it is the id we wish to remove
            if (_usersToLocks[user].Count() > 1)
            {
                byte outbyte;
                _usersToLocks[user].TryRemove(id, out outbyte);
            }
            else
            {
                ConcurrentDictionary<string, byte> outDict;
                _usersToLocks.TryRemove(user, out outDict);
            }
            LockRemoved?.Invoke(this,id,user);
        }

        /// <summary>
        /// Method to tell if a certain lock is being held by anyone.
        /// Will throw an exception if lockableId is null or empty.
        /// </summary>
        /// <param name="lockableId"></param>
        /// <returns></returns>
        public bool LockHeld(string lockableId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockableId));
            if (string.IsNullOrEmpty(lockableId))
            {
                throw new Exception("passed in lock id was null or empty");
            }
            return _lockToUser.ContainsKey(lockableId);
        }

        /// <summary>
        /// Method to get the user who currently holds a lock.
        /// Returns null if nobody holds the lock.
        /// Will throw an exception if lockableId is null or empty.
        /// </summary>
        /// <param name="lockableId"></param>
        /// <returns></returns>
        public NuWebSocketHandler GetUserFromLock(string lockableId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockableId));
            if (string.IsNullOrEmpty(lockableId))
            {
                throw new Exception("passed in lock id  for getting the holder of a lock was null or empty");
            }
            if (!LockHeld(lockableId))
            {
                return null;
            }
            return _lockToUser[lockableId];
        }


        /// <summary>
        /// Public method to add a lock to this controller.
        /// Returns whether the user has the lock when the method returns.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="handler"></param>
        /// <returns></returns>
        public bool AddLock(string id, NuWebSocketHandler handler)
        {
            Debug.Assert(id != null && handler != null);
            if (id == null || handler == null)
            {
                throw new Exception("Add lock method was given a null string or user websockethandler");
            }
            if (_lockToUser.ContainsKey(id))
            {
                if (_lockToUser[id] == handler)
                {
                    Debug.Fail("We probably shouldn't be sending the request if this ");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            _lockToUser.TryAdd(id, handler);
            if (!_usersToLocks.ContainsKey(handler))
            {
                _usersToLocks.TryAdd(handler, new ConcurrentDictionary<string, byte>());
            }
            _usersToLocks[handler].TryAdd(id, 0);
            LockAdded?.Invoke(this,id,handler);
            return true;
        }

        /// <summary>
        /// public method to remove a lock from this controller.  
        /// Returns whether the removal was successful
        /// </summary>
        /// <param name="lockId"></param>
        /// <returns></returns>
        public bool RemoveLock(string lockId)
        {
            if (_lockToUser.ContainsKey(lockId))
            {
                RemoveLockInternally(lockId);
                return true;
            }
            return false;
        }
    }
}