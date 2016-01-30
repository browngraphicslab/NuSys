using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace NuSysApp
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
                _message["nodeLines"] = new List<string>(lines);

                var contentsFile = await NuSysStorages.SaveFolder.GetFileAsync("_contents.nusys");
                var contentLines = await FileIO.ReadLinesAsync(contentsFile);
                _message["contentLines"] = new List<string>(contentLines);
            });
            await base.CheckOutgoingRequest();
        }

        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, ServerClient serverClient, string senderIP)
        {
            await Task.Run(async delegate
            {
                var file = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "workspace.nusys");
                var lines = _message.GetList<string>("nodeLines");
                await FileIO.WriteLinesAsync(file, lines);

                var contentFile = await StorageUtil.CreateFileIfNotExists(NuSysStorages.SaveFolder, "_contents.nusys");
                var contentLines = _message.GetList<string>("contentLines");
                await FileIO.WriteLinesAsync(contentFile, contentLines);
            });
            await UITask.Run(async delegate {
                await SessionController.Instance.LoadWorkspace();
            });
        }

    }
}
