using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace NuSysApp
{
    public class DuplicateNodeRequest : Request
    {
        public DuplicateNodeRequest(Message message) : base(Request.RequestType.DuplicateNodeRequest, message){}

        public override async Task ExecuteRequestFunction()
        {
            var id = _message.GetString("id");
            var model = (ElementInstanceModel)SessionController.Instance.IdToSendables[id];
            
            ElementType type = model.ElementType;

            if (type == ElementType.Group)
            {
                var childList = _message.GetList<string>("groupChildren");
                foreach (var childId in childList)
                {
                    var childModel = (ElementInstanceModel)SessionController.Instance.IdToSendables[childId];
                    var groups = (List<string>)childModel.GetMetaData("groups");

                    //TODO: refactor
                   // childModel.Creator = id;
                    groups.Add(id);
                }
            }

            var modelToDuplicate = await model.Pack();
            modelToDuplicate.Remove("id");
            modelToDuplicate["x"] = _message.GetDouble("targetX");
            modelToDuplicate["y"] = _message.GetDouble("targetY");
            modelToDuplicate["autoCreate"] = true;

            var msg = new Message( modelToDuplicate );
            var request = new NewNodeRequest(msg);
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(request);

            var duplicateModel = (ElementInstanceModel)SessionController.Instance.IdToSendables[msg.GetString("id")];

            if (!(duplicateModel is NodeContainerModel))
                return;


            //TODO: refactor
            /*
            foreach (var child in SessionController.Instance.IdToSendables.Values.Where(s => (s as ElementInstanceModel).Creator.Contains(id)))
            {
                ((NodeContainerModel)duplicateModel).AddChild(child);
            }
            */
        }
    }
}
