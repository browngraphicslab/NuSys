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
                await SessionController.Instance.SaveWorkspace();
                var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
                var lines = await FileIO.ReadLinesAsync(file);
                _message["nodelines"] = new List<string>(lines);
            });
            await base.CheckOutgoingRequest();
        }

        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session,
            string senderIP)
        {
            await Task.Run(async delegate
            {
                var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
                var lines = _message.GetList<string>("nodelines");
                await FileIO.WriteLinesAsync(file, lines);
            });
            await UITask.Run(async delegate {
                await SessionController.Instance.LoadWorkspace();
            });
        }

    }
}
