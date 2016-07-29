using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class DeleteLibraryElementRequest : Request
    {
        public DeleteLibraryElementRequest(string id) : base(NusysConstants.RequestType.DeleteLibraryElementRequest)
        {
            _message["id"] = id;
        }
        public DeleteLibraryElementRequest(Message m) : base(NusysConstants.RequestType.DeleteLibraryElementRequest,m)
        {
        }
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id"))
            {
                throw new Exception("Library Element Delete requests must contains a library 'id' to delete");
            }
            var id = _message.GetString("id");
            await UITask.Run(async delegate
            {
                var linkModels =
                    SessionController.Instance.ContentController.ContentValues.Where(
                        item => item is LinkLibraryElementModel);
                foreach (var linkModel in new HashSet<LinkLibraryElementModel>(linkModels.Select(item => item as LinkLibraryElementModel)))
                {
                    if (linkModel == null)
                    {
                        continue;
                    }
                    if (linkModel.InAtomId == id || linkModel.OutAtomId == id)
                    {
                        await Task.Run(async delegate
                        {
                            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(
                                new DeleteLibraryElementRequest(linkModel.LibraryElementId));
                        });
                    }
                }
            });
        }
        public override async Task ExecuteRequestFunction()
        {
            var libraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(_message.GetString("id"));
            if (libraryElementController == null)
            {
                return;
            }
            SessionController.Instance.LinksController.RemoveContent(libraryElementController);
            libraryElementController.Delete();
        }
    }
}
