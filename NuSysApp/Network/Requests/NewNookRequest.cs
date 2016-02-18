using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml.Media.Animation;

namespace NuSysApp
{
    public class NewNookRequest : Request
    {
        public NewNookRequest(Message message) : base(RequestType.NewNookRequest, message){}
        public async override Task ExecuteRequestFunction()
        {
            var childrenIds = _message.GetList("enclosedAtoms", new List<string>());

            if (childrenIds.Count == 0)
                return;

            var msg = new Message();
            msg["id"] = SessionController.Instance.GenerateId();
            msg["nodeType"] = NodeType.Group.ToString();
            msg["points"] = _message.GetList("points", new List<Point2d>());
            msg["width"] = _message.GetDouble("width");
            msg["height"] = _message.GetDouble("height");
            msg["x"] = _message.GetDouble("x");
            msg["y"] = _message.GetDouble("y");
            msg["autoCreate"] = true;
            msg["creators"] = new List<string>() {SessionController.Instance.ActiveWorkspace.Id};

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewNodeRequest(msg));

           
            foreach (var childId in childrenIds)
            {
                var child = (AtomModel) SessionController.Instance.IdToSendables[childId];

                child.X -= _message.GetDouble("x");
                child.Y -= _message.GetDouble("y");
                
                child.Creators.Remove(SessionController.Instance.ActiveWorkspace.Id);
                child.Creators.Add(msg["id"].ToString());
                await SessionController.Instance.RecursiveCreate(child);
            }


            /*

                var group = SessionController.Instance.IdToSendables[msg.GetString("id")];
                
                node1.Creators.Add(group.Id);
                var prevGroups1 = (List<string>)node1.GetMetaData("groups");
                prevGroups1.Add(group.Id);
                node1.SetMetaData("groups", prevGroups1);
                node1.Creators.Remove(SessionController.Instance.ActiveWorkspace.Id);

                node2.Creators.Add(group.Id);
                var prevGroups2 = (List<string>)node2.GetMetaData("groups");
                prevGroups2.Add(group.Id);
                node2.SetMetaData("groups", prevGroups2);
                node2.Creators.Remove(SessionController.Instance.ActiveWorkspace.Id);
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
            */


        }
    }
}
