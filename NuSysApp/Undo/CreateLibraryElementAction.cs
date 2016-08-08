using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class CreateLibraryElementAction : IUndoable
    {
        private Message _message;

        /// <summary>
        ///  Message must contain fields for id, data, small_thumbnail, medium_thumbnail, large_thumbnail,
        ///  type, title, server_url, creation_timestamp, last_edited_timestamp
        /// </summary>
        /// <param name="m"></param>
        public CreateLibraryElementAction(Message m)
        {
            _message = m;
        }

        public async void ExecuteAction()
        {
            var request = new CreateNewLibraryElementRequest(_message);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }

        public IUndoable GetInverse()
        {
            return new RemoveLibraryElementAction(_message);
        }
    }
}
