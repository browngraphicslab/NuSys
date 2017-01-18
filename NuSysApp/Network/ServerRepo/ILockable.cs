using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// Interface to extend for any object you want to be able to lock from other clients
    /// Many functions of this ILockable are in the class ILockableMethodExtensions which you should read before using this interface
    /// </summary>
    public interface ILockable
    { 
        /// <summary>
        /// The ID of this lockable element. 
        /// Can be any unique id as long as it's globally consistent with the identical locking element remotely.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Event handler fired whenever this lockable's lock holder is changed.
        /// As useful helper method for the returned Network User is User.IsLocalUser().
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="currentUser"></param>
        void LockChanged(object sender, NetworkUser currentUser);
    }
}
