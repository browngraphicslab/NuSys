using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using NuSysApp.Network.Requests.SystemRequests;

namespace NuSysApp
{/*
    public class NewContentSystemRequest : SystemRequest
    {
        public NewContentSystemRequest(Message m) : base(SystemRequestType.NewContent, m) { }

        public NewContentSystemRequest(string id, string data) : base(SystemRequestType.NewContent)
        {
            _message["id"] = id;
            _message["data"] = data;
        }
        public async override Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey("id") || !_message.ContainsKey("data"))
            {
                throw new Exception("New Content requests require at least 'id' and 'data'");
            }
            await base.CheckOutgoingRequest();
        }
        public override async Task ExecuteSystemRequestFunction(NuSysNetworkSession nusysSession, NetworkSession session, ServerClient serverClient, string senderIP)
        {
            await UITask.Run(async delegate
            {
                var data = _message.GetString("data");
                var id = _message.GetString("id");
                SessionController.Instance.ContentController.Add(data, id);
                if (SessionController.Instance.LoadingNodeDictionary.ContainsKey(id))
                {
                    var tuple = SessionController.Instance.LoadingNodeDictionary[id];
                    LoadNodeView view = tuple.Item2;
                    AtomModel model = tuple.Item1;
                    var factory = new FreeFormNodeViewFactory();
                    FrameworkElement newView;
                    newView = await factory.CreateFromSendable(model, null);
                    SessionController.Instance.ActiveWorkspace.Children.Remove(model.Id);
                    SessionController.Instance.ActiveWorkspace.Children.Add(model.Id, newView);
                    SessionController.Instance.ActiveWorkspace.AtomViewList.Remove(view);
                    SessionController.Instance.ActiveWorkspace.AtomViewList.Add(newView);
                    if (nusysSession.IsHostMachine)
                    {
                        await nusysSession.ExecuteSystemRequest(new ContentAvailableNotificationSystemRequest(id));
                    }
                }
            });
        }
    }*/
}
