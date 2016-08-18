using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// Describes a DeleteLibraryElementAction, which is to be instantiated when the client deletes a library element
    /// </summary>
    public class DeleteLibraryElementAction : IUndoable
    {

        private CreateNewLibraryElementRequestArgs _args;

        /// <summary>
        /// Args must contain fields for id, data, small_thumbnail, medium_thumbnail, large_thumbnail,
        /// type, title, server_url, creation_timestamp, last_edited_timestamp
        /// </summary>
        /// <param name="args"></param>
        public DeleteLibraryElementAction(CreateNewLibraryElementRequestArgs args)
        {
            _args = args;
        }

        /// <summary>
        /// Returns a CreateLibraryElementAction, the inverse of a DeleteLibraryElementAction
        /// </summary>
        /// <returns></returns>
        public IUndoable GetInverse()
        {
            return new CreateLibraryElementAction(_args);
        }

        /// <summary>
        /// Executes the DeleteLibraryAction by creating and executing a request based on the action's properties
        /// </summary>
        public void ExecuteAction()
        {
            var request = new DeleteLibraryElementRequest(_args.LibraryElementId);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }
    }
}
