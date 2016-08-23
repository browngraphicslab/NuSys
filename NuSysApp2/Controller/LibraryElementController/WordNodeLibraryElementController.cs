using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Windows.UI;

namespace NuSysApp2
{
    public class WordNodeLibraryElementController : LibraryElementController, ILockable
    {
        public delegate void LockedEventHandler(object sender, NetworkUser user);
        public delegate void UnLockedEventHandler(object sender);

        public event LockedEventHandler Locked;
        public event UnLockedEventHandler UnLocked;
        /// <summary>
        ///  returns the libraryElementId so the lockcontroller can identify this object
        /// </summary>
        public string Id
        {
            get { return LibraryElementModel.LibraryElementId; }
        }

        /// <summary>
        /// kept track of by the lockcontroller, 
        /// should be treated as read-only, and only being set by the lcok controller
        /// </summary>
        public bool IsLocked { get; set; }

        public WordNodeLibraryElementController(LibraryElementModel model) : base(model)
        {
            Debug.Assert(model.Type == ElementType.Word);
        }

        /// <summary>
        /// Called by the lock controller when the server locks this item
        /// </summary>
        /// <param name="userId"></param>
        public void Lock(string userId)
        {
            NetworkUser user = null;
            if (SessionController.Instance.NuSysNetworkSession.NetworkMembers.ContainsKey(userId))
            {
                user = SessionController.Instance.NuSysNetworkSession.NetworkMembers[userId];
            }
            Locked?.Invoke(this, user);
        }

        /// <summary>
        /// called by the lock controller when the server unlocks this item
        /// </summary>
        public void UnLock()
        {
            UnLocked?.Invoke(this);
        }
    }
}
