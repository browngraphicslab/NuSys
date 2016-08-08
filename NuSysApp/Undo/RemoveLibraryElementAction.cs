using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class RemoveLibraryElementAction : IUndoable
    {

        private Message _message;

        /// <summary>
        /// Message must contain fields for id, data, small_thumbnail, medium_thumbnail, large_thumbnail,
        /// type, title, server_url, creation_timestamp, last_edited_timestamp
        /// </summary>
        /// <param name="m"></param>
        public RemoveLibraryElementAction(Message m)
        {
            _message = m;
        }

        public IUndoable GetInverse()
        {
            return new CreateLibraryElementAction(_message);
        }

        public void ExecuteAction()
        {
            var request = new DeleteLibraryElementRequest(_message);
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);
        }
    }
}
