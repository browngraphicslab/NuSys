using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class LockController : INuSysDisposable
    {

        /// <summary>
        /// Dictionary mapping from the ILockable id to the Lock object
        /// </summary>
        private Dictionary<string, Lock> _locksDictionary;

        /// <summary>
        /// Event fired whenever this locksController disposes.
        /// </summary>
        public event EventHandler Disposed;

        /// <summary>
        /// Event fired whenever the entire lockController is cleared of ALL locks being listened to
        /// </summary>
        public event EventHandler Cleared;

        /// <summary>
        /// private hashset for tracking which lockable instances are currently registered.
        /// The count of items in this hashset should always be equal to the toal number of listeners in the _registeredLockables values combines.
        /// </summary>
        private HashSet<ILockable> _registeredLockables;

        /// <summary>
        /// Constructor simply instantiates the private variables, no parameters.
        /// </summary>
        public LockController()
        {
            _locksDictionary = new Dictionary<string, Lock>();
            _registeredLockables = new HashSet<ILockable>();
        }

        /// <summary>
        /// Method to clear this controller of ALL lock tracking.
        /// This will return all locks and stop tracking any locks we are subscribed to.
        /// Returns whether the clear was successful.
        /// Setting force to be true will always clear the local locks and fire the cleared event, even if the server request fails.
        /// </summary>
        /// <returns></returns>
        public async Task<bool> Clear(bool force = false)
        {
            var success = await PrivateSenderUnSubscribeRequest(_locksDictionary.Keys);
            if (success != true)
            {
                if (force)
                {
                    PrivateClear();
                }
                return false;
            }
            PrivateClear();
            return true;
        }

        /// <summary>
        /// method to send request to unsubscribe from multiple lock ids.
        /// Returns the WasSuccesful() result
        /// </summary>
        /// <param name="idsToUnsubscribeFrom"></param>
        /// <returns></returns>
        private async Task<bool?> PrivateSenderUnSubscribeRequest(IEnumerable<string> idsToUnsubscribeFrom)
        {
            var request = new UnSubscribeToLockRequest(new UnSubscribeToLockRequestArgs()
            {
                LockableIds = new HashSet<string>(idsToUnsubscribeFrom)
            });
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            return request.WasSuccessful();
        }

        /// <summary>
        /// private clear all variables and fire the clear Event.
        /// </summary>
        private void PrivateClear()
        {
            _locksDictionary.Clear();
            _registeredLockables.Clear();
            Cleared?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Dispose method simply removes itself from _serverClient events.
        /// </summary>
        public void Dispose()
        {
            Clear();
            Disposed?.Invoke(this,EventArgs.Empty);
        }

        /// <summary>
        /// Method to return the user Id of the person who has the lock for the specified lockable object.
        /// Returns null if the lock isn't known about or if the lock isn't currenly held by anybody.
        /// </summary>
        /// <param name="lockId"></param>
        /// <returns></returns>
        public string GetUserIdOfLockHolder(string lockId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockId));
            if (!_locksDictionary.ContainsKey(lockId))
            {
                return null;
            }
            return _locksDictionary[lockId].LockHolderId;
        }

        /// <summary>
        /// private method to asynchronously send a reuqest to the server for subscribing to lock updates.
        /// this will request a lock if the requestLock bool is true.
        /// This should also parse the returned request and update the local Lock accordingly.
        /// </summary>
        /// <param name="lockId"></param>
        /// <param name="requestLock"></param>
        /// <returns></returns>
        private async Task SendSubscribeRequestAsync(string lockId, bool requestLock = true)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockId));

            var request = new SubscribeToLockRequest(new SubscribeToLockRequestArgs()
            {
                LockableId =  lockId,
                RequestLock =  requestLock
            });
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            var success = request.WasSuccessful();
            Debug.Assert(success == true);
            var currentHolderId = request.LockHolderUserId();
            UpdateLock(lockId, currentHolderId);
        }

        /// <summary>
        /// Method to be called whenever a lockable is registered which will add it and its listeners to the private tracking variables
        /// </summary>
        private void PrivateRegister(ILockable lockable)
        {
            Debug.Assert(lockable != null);
            Debug.Assert(!_registeredLockables.Contains(lockable));
            Debug.Assert(!string.IsNullOrEmpty(lockable.Id));

            _registeredLockables.Add(lockable);
            _locksDictionary[lockable.Id].Listeners.Add(lockable.LockChanged);
        }

        /// <summary>
        /// public method to return whether this controller is tracking the passed in lockable.
        /// </summary>
        /// <param name="lockable"></param>
        /// <returns></returns>
        public bool IsRegistered(ILockable lockable)
        {
            Debug.Assert(lockable != null);
            Debug.Assert(!string.IsNullOrEmpty(lockable.Id));
            return _registeredLockables.Contains(lockable);
        }

        /// <summary>
        /// static method to get the network user associated with a userId.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static NetworkUser GetNetworkUser(string id)
        {
            Debug.Assert(!string.IsNullOrEmpty(id));
            if (SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(id))
            {
                return SessionController.Instance.NuSysNetworkSession.NetworkMembers[id];
            }
            return null;
        }

        /// <summary>
        /// Method to register a lockable and have its event handlers set up to listen to updates.
        /// This will request a lock if the requestLock is true.
        /// This should only be called from the ILockable extension to register.
        /// Can return null network user if nobody has the lock after this returns.
        /// This will fully await for the requset to register to be confirmed, adn you will get ALL updates IMMEDIATELY after this method ends
        /// </summary>
        /// <param name="lockable"></param>
        /// <param name="requestLock"></param>
        /// <returns></returns>
        public async Task<NetworkUser> RegisterAsync(ILockable lockable, bool requestLock = true)
        {
            Debug.Assert(lockable != null);
            Debug.Assert(!string.IsNullOrEmpty(lockable.Id));

            if (IsRegistered(lockable))
            {
                return GetNetworkUser(_locksDictionary[lockable.Id].LockHolderId);
            }


            if (!_locksDictionary.ContainsKey(lockable.Id))
            {
                _locksDictionary.Add(lockable.Id, new Lock());
                await SendSubscribeRequestAsync(lockable.Id, requestLock);
            }
            PrivateRegister(lockable);
            return GetNetworkUser(_locksDictionary[lockable.Id].LockHolderId);
        }

        /// <summary>
        /// Method to register a lockable and have its event handlers set up to listen to updates.
        /// This will request a lock if the requestLock is true.
        /// This should only be called from the ILockable extension to register.
        /// This method has once synchronization important feature, that even though you are registered after this method is called, 
        /// you may not get updates if they happen RIGHT AFTER you register since the server request may not have been processed yet.
        /// If you truly want no sync issues, call the async version.
        /// </summary>
        /// <param name="lockable"></param>
        /// <param name="requestLock"></param>
        /// <returns></returns>
        public bool Register(ILockable lockable, bool requestLock = true)
        {
            Debug.Assert(lockable != null);
            Debug.Assert(!string.IsNullOrEmpty(lockable.Id));

            if (IsRegistered(lockable))
            {
                return false;
            }

            if (!_locksDictionary.ContainsKey(lockable.Id))
            {
                _locksDictionary.Add(lockable.Id, new Lock());
                SendSubscribeRequestAsync(lockable.Id, requestLock);
            }
            PrivateRegister(lockable);
            return true;
        }
        
        /// <summary>
        /// Method to call to remove a lockable from this controller.
        /// THis will automatically unsubscribe us from getting updates.
        /// This should only be called from the lockable extensions class method
        /// </summary>
        /// <param name="lockable"></param>
        /// <returns></returns>
        public bool UnRegister(ILockable lockable)
        {
            Debug.Assert(lockable != null);
            Debug.Assert(!string.IsNullOrEmpty(lockable.Id));

            if (!IsRegistered(lockable))
            {
                return false;
            }

            Debug.Assert(_locksDictionary.ContainsKey(lockable.Id));
            Debug.Assert(_locksDictionary[lockable.Id].Listeners.Contains(lockable.LockChanged));

            _registeredLockables.Remove(lockable);
            _locksDictionary[lockable.Id].Listeners.Remove(lockable.LockChanged);
            if (_locksDictionary[lockable.Id].ListenerCount == 0)
            {
                _locksDictionary.Remove(lockable.Id);
                PrivateSenderUnSubscribeRequest(new List<string>() {lockable.Id});
            }
            return true;
        }

        /// <summary>
        /// Public method to call to request a lock.  
        /// This won't return anything, but rather asynchronously fetch the lock.
        /// If you are registered to listen and the lock changes, you will get the update in your LockChanged handler;
        /// </summary>
        /// <param name="lockable"></param>
        public void GetLock(ILockable lockable)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockable.Id));
            if (lockable.HasLock() ||
                (_locksDictionary.ContainsKey(lockable.Id) && _locksDictionary[lockable.Id].LockHolderId != null))
            {
                return;
            }
            PrivateGetLock(lockable.Id);
        }

        /// <summary>
        /// public method to return a lock for a lockable that has the lock locally.
        /// Will return false if the lock isn't held locally.
        /// </summary>
        /// <param name="lockable"></param>
        /// <returns></returns>
        public bool ReturnLock(ILockable lockable)
        {
            Debug.Assert(lockable != null);
            Debug.Assert(!string.IsNullOrEmpty(lockable.Id));
            if (!lockable.HasLock())
            {
                return false;
            }
            PrivateReturnLock(lockable.Id);
            return true;
        }

        /// <summary>
        /// This method will make a get lock request and will appropriotely assign the lock to us if we get it.
        /// </summary>
        /// <param name="lockId"></param>
        /// <returns></returns>
        private async Task PrivateGetLock(string lockId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockId));
            Debug.Assert(!(_locksDictionary.ContainsKey(lockId) && _locksDictionary[lockId].LockHolderId == WaitingRoomView.UserID));
            Debug.Assert(!_locksDictionary.ContainsKey(lockId) || _locksDictionary[lockId].LockHolderId == null,
                "if this isn't null, we are requesting a lock that clearly is already taken.  WTF, bro?");
            var holder = await PrivateRequestGetLockAsync(lockId);
            UpdateLock(lockId,holder);
        }

        /// <summary>
        /// This private async return lock method will make the return lock request, and remoe the lock accordingly.
        /// </summary>
        /// <returns></returns>
        private async Task PrivateReturnLock(string lockId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockId));
            Debug.Assert(_locksDictionary.ContainsKey(lockId) && _locksDictionary[lockId].LockHolderId == WaitingRoomView.UserID);
            var success = await PrivateReturnLockAsync(lockId);
            Debug.Assert(success == true);
            UpdateLock(lockId,null);
        }


        /// <summary>
        /// Method that sends a request to ask for the lock for the given id.
        /// this will return the holder of the lock after the request ends
        /// </summary>
        /// <param name="lockId"></param>
        /// <returns></returns>
        private async Task<string> PrivateRequestGetLockAsync(string lockId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockId));
            Debug.Assert(!(_locksDictionary.ContainsKey(lockId) && _locksDictionary[lockId].LockHolderId == WaitingRoomView.UserID));
            Debug.Assert(!_locksDictionary.ContainsKey(lockId) || _locksDictionary[lockId].LockHolderId == null, 
                "if this isn't null, we are requesting a lock that clearly is already taken.  WTF, bro?");

            var request = new GetLockRequest(new GetLockRequestArgs()
            {
                LockableId = lockId
            });
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            if (request.WasSuccessful() == true)
            {
                return request.CurrentLockHolder();
            }
            Debug.Fail("why did it fail?!?!?!?");
            return null;
        }

        /// <summary>
        /// Private method to send a request to the server to return a lock.
        /// this will return the WasSuccessful() value after the request returns
        /// </summary>
        /// <param name="lockId"></param>
        /// <returns></returns>
        private async Task<bool?> PrivateReturnLockAsync(string lockId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockId));
            Debug.Assert(_locksDictionary.ContainsKey(lockId) && _locksDictionary[lockId].LockHolderId == WaitingRoomView.UserID);
            var request = new ReturnLockRequest(new ReturnLockRequestArgs()
            {
                LockableId = lockId
            });
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
            return request.WasSuccessful();
        }

        /// <summary>
        /// Method to call whenever there is an update to the holder of a lock.
        /// This will fire the eventHandlers listening to the updated lock
        /// </summary>
        /// <param name="lockId"></param>
        /// <param name="userId"></param>
        public void UpdateLock(string lockId, string userId)
        {
            Debug.Assert(!string.IsNullOrEmpty(lockId));

            if (_locksDictionary.ContainsKey(lockId))
            {
                var oldValue = _locksDictionary[lockId].LockHolderId;
                if (oldValue == userId)
                {
                    return;//if there isn't an actual update, ignore
                }
                _locksDictionary[lockId].LockHolderId = userId;
                var user = userId == null ? null : GetNetworkUser(userId);
                foreach (var listener in _locksDictionary[lockId].Listeners)
                {
                    listener?.Invoke(this,user);
                }
            }
        }


        /// <summary>
        /// private inner class to hold the userID of a locked object as well as the list of people listening to that lock's changes
        /// </summary>
        private class Lock
        {
            /// <summary>
            /// the string ID of the user who holds the lock, or null if nobody does
            /// </summary>
            public string LockHolderId { get; set; } = null;

            /// <summary>
            /// Hashset of listeners to the changes of this lock object
            /// </summary>
            public HashSet<EventHandler<NetworkUser>> Listeners = new HashSet<EventHandler<NetworkUser>>();

            /// <summary>
            /// The number of listeners currently attached to the lock object
            /// </summary>
            public int ListenerCount
            {
                get { return Listeners.Count; }
            }
        }
    }
}
