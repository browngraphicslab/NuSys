using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// All undoable actions will implement this undoable interface
    /// </summary>
   public interface IUndoable
   {
        /// <summary>
        /// Returns the inverse--an action representing the "undone" version of the current action
        /// </summary>
        /// <returns></returns>
       IUndoable GetInverse();

        /// <summary>
        /// To extract the undoable action as a Request that you can use to regenerate the state of the action
        /// </summary>
        /// <returns></returns>
       
        /*
       Request ToRequest();
        /// <summary>
        /// executes the request that is created by this IUndoable
        /// </summary>
        */

        void ExecuteAction();
   }
}
