using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class NewGroupRequest : Request
    {
        
        public NewGroupRequest(Message message) : base(RequestType.NewGroupRequest, message){}
        public async override Task ExecuteRequestFunction()
        {
            var props = _message;
            
            var node1 = (ElementInstanceModel)SessionController.Instance.IdToSendables[props.GetString("id1")];
            var node2 = (ElementInstanceModel)SessionController.Instance.IdToSendables[props.GetString("id2")];
            var groupNodeId = props.GetString("groupNodeId");

            var wvm = SessionController.Instance.ActiveWorkspace;

            if (node2 is NodeContainerModel)
            {

                //TODO: refactor
                //node1.Creator = node2.Id;
                var prevGroups1 = (List<string>)node1.GetMetaData("groups");
                prevGroups1.Add(node2.Id);
                node1.SetMetaData("groups", prevGroups1);
                await (node2 as NodeContainerModel).AddChild(node1);
                wvm.RemoveChild(node1.Id);
            }
            else
            {
                var msg = new Message();
                msg["id"] = groupNodeId;
                msg["nodeType"] = ElementType.Group.ToString();
                msg["width"] = _message.GetDouble("width");
                msg["height"] = _message.GetDouble("height");
                msg["x"] = _message.GetDouble("x");
                msg["y"] = _message.GetDouble("y");
                msg["autoCreate"] = true;
                msg["creator"] = SessionController.Instance.ActiveWorkspace.Id;

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewNodeRequest(msg));

                var group = SessionController.Instance.IdToSendables[msg.GetString("id")];
                
                //node1.Creator = group.Id;
                var prevGroups1 = (List<string>)node1.GetMetaData("groups");
                prevGroups1.Add(group.Id);
                node1.SetMetaData("groups", prevGroups1);

              //  node2.Creator = group.Id;
                var prevGroups2 = (List<string>)node2.GetMetaData("groups");
                prevGroups2.Add(group.Id);
                node2.SetMetaData("groups", prevGroups2);
                var found = wvm.AtomViewList.Where(a => (a.DataContext as AtomViewModel).Id == group.Id);
                
                NodeContainerModel groupModel;
                if (!found.Any())
                {
                    groupModel = (NodeContainerModel)node2;
                    await groupModel.AddChild(node1);
                    wvm.RemoveChild(node1.Id);
                }
                else
                {
                    groupModel = (NodeContainerModel)SessionController.Instance.IdToSendables[group.Id];

                    await groupModel.AddChild(node1);
                    wvm.RemoveChild(node1.Id);
                    await groupModel.AddChild(node2);
                    wvm.RemoveChild(node2.Id);
                }

                if (!found.Any())
                    return;

                var groupView = found.First() as AnimatableUserControl;
                groupView.RenderTransformOrigin = new Point(0.5, 0.5);

                Anim.FromTo(groupView, "Alpha", 0, 1, 600, new BackEase());
                Anim.FromTo(groupView, "ScaleY", 0, 1, 600, new BackEase());
                Anim.FromTo(groupView, "ScaleX", 0, 1, 600, new BackEase());
            }
        }
    }
}
