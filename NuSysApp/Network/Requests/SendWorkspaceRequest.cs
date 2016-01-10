using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp.Network.Requests
{
    public class SendWorkspaceRequest : SystemRequest
    {
        public SendWorkspaceRequest(Message m) : base(m){}

        public SendWorkspaceRequest() : base(SystemRequestType.SendWorkspace){}

        public override async Task CheckOutgoingRequest()
        {
            await Task.Run(async delegate
            {
                var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
                _message["nodelines"] = new List<string>(await FileIO.ReadLinesAsync(file));
            });
            await base.CheckOutgoingRequest();
        }

        public virtual async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session,
            string senderIP)
        {
            await Task.Run(async delegate
            {
                var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
                var lines = _message.GetList<string>("nodelines");
                await FileIO.WriteLinesAsync(file, lines);
                await SessionController.Instance.LoadWorkspace();
            });
        }

    }
}
