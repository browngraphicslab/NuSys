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
        //private Message _message;

        /// <summary>
        ///  Message must contain fields for id, data, small_thumbnail, medium_thumbnail, large_thumbnail,
        ///  type, title, server_url, creation_timestamp, last_edited_timestamp
        /// </summary>
        /// <param name="m"></param>
        
        public CreateLibraryElementAction()
        {
            //TODO 817 fix this
            //_message = m;
        }

        /// <summary>
        /// Executes a CreateLibraryElementRequest based on the message contained in the CreateLibraryElementAction
        /// </summary>
        public async void ExecuteAction()
        {
            //TODO fix this 817
            //var request = new CreateNewLibraryElementRequest(_message);
            //await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }

        /// <summary>
        /// Returns a DeleteLibraryElementAction, which is the logical inverse of a CreateLibraryElementAction
        /// </summary>
        /// <returns></returns>
        public IUndoable GetInverse()
        {
            //fix this 817
            return null;
            //return new DeleteLibraryElementAction(_message);
        }
    }
}
