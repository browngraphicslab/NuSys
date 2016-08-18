using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    /// <summary>
    /// Describes a CreateLibraryAction, which is instantiated when DeleteLibraryActions are undone
    /// </summary>
    public class CreateLibraryElementAction : IUndoable
    {
        private CreateNewLibraryElementRequestArgs _args;

        /// <summary>
        ///  Message must contain fields for id, data, small_thumbnail, medium_thumbnail, large_thumbnail,
        ///  type, title, server_url, creation_timestamp, last_edited_timestamp
        /// </summary>
        /// <param name="m"></param>
        
        public CreateLibraryElementAction(CreateNewLibraryElementRequestArgs args)
        {
            _args = args;
        }

        /// <summary>
        /// Executes a CreateLibraryElementRequest based on the message contained in the CreateLibraryElementAction
        /// </summary>
        public async void ExecuteAction()
        {
            var request = new CreateNewLibraryElementRequest(_args);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
        }

        /// <summary>
        /// Returns a DeleteLibraryElementAction, which is the logical inverse of a CreateLibraryElementAction
        /// </summary>
        /// <returns></returns>
        public IUndoable GetInverse()
        {
            //fix this 817
            return new DeleteLibraryElementAction(_args);
        }
    }
}
