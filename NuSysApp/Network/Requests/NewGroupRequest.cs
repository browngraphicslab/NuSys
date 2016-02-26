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
            
            var eic1 = SessionController.Instance.IdToControllers[props.GetString("id1")];
            var eic2 = SessionController.Instance.IdToControllers[props.GetString("id2")];
            var eic1Parent = SessionController.Instance.IdToControllers[eic1.Model.Creator];
            var eic2Parent = SessionController.Instance.IdToControllers[eic2.Model.Creator];

            if (eic2 is ElementCollectionInstanceController)
            {
                (eic1Parent as ElementCollectionInstanceController).RemoveChild(eic1);
                (eic2 as ElementCollectionInstanceController).AddChild(eic1);
                eic1.SetCreator(eic2.Model.Id);
            }
            else
            {
                var msg = new Message();
                msg["id"] = SessionController.Instance.GenerateId();
                msg["nodeType"] = ElementType.Group.ToString();
                msg["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;
                msg["width"] = _message.GetDouble("width");
                msg["height"] = _message.GetDouble("height");
                msg["x"] = _message.GetDouble("x");
                msg["y"] = _message.GetDouble("y");
                msg["autoCreate"] = true;
                msg["creator"] = SessionController.Instance.ActiveFreeFormViewer.Id;

                await SessionController.Instance.NuSysNetworkSession.ExecuteRequestLocally(new NewElementInstanceRequest(msg));

                var group = SessionController.Instance.IdToControllers[msg.GetString("id")];
                
                //TODO: refactor
                /*
                //node1.Creator = group.Id;
                var prevGroups1 = (List<string>)eic1.GetMetaData("groups");
                prevGroups1.Add(group.Model.Id);
                eic1.SetMetaData("groups", prevGroups1);

              //  node2.Creator = group.Id;
                var prevGroups2 = (List<string>)eic2.GetMetaData("groups");
                prevGroups2.Add(group.Model.Id);
                eic2.SetMetaData("groups", prevGroups2);
                var found = wvm.AtomViewList.Where(a => (a.DataContext as ElementInstanceViewModel).Id == group.Model.Id);
                
                NodeContainerModel groupModel;
                if (!found.Any())
                {
                    groupModel = (NodeContainerModel)eic2;
                    await groupModel.AddChild(eic1);
                    wvm.RemoveChild(eic1.Id);
                }
                else
                {
                    groupModel = (NodeContainerModel)SessionController.Instance.IdToControllers[group.Model.Id].Model;

                    await groupModel.AddChild(eic1);
                    wvm.RemoveChild(eic1.Id);
                    await groupModel.AddChild(eic2);
                    wvm.RemoveChild(eic2.Id);
                }

                if (!found.Any())
                    return;

                var groupView = found.First() as AnimatableUserControl;
                groupView.RenderTransformOrigin = new Point(0.5, 0.5);

                Anim.FromTo(groupView, "Alpha", 0, 1, 600, new BackEase());
                Anim.FromTo(groupView, "ScaleY", 0, 1, 600, new BackEase());
                Anim.FromTo(groupView, "ScaleX", 0, 1, 600, new BackEase());
                */
            }
        }
    }
}
