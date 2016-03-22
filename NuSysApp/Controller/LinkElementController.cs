using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkElementController : ElementController
    {
        public delegate void AnchorUpdatedEventHandler(object source);

        public event AnchorUpdatedEventHandler AnchorUpdated;

        public ElementController InElement { get; set; }
        public ElementController OutElement { get; set; }

        public LinkElementController(LinkModel model) : base(model)
        {
            InElement = SessionController.Instance.IdToControllers[model.InAtomId];
            OutElement = SessionController.Instance.IdToControllers[model.OutAtomId];
            InElement.AddLink(this);
            OutElement.AddLink(this);
        }

        public override async Task RequestMoveToCollection(string newCollectionContentID)
        {
            var metadata = new Dictionary<string, object>();
            var link = Model as LinkModel;

            var m1 = new Message();
            m1["metadata"] = metadata;
            m1["contentId"] = Model.LibraryId;
            m1["nodeType"] = Model.ElementType;
            m1["x"] = 50000;
            m1["y"] = 50000;
            m1["width"] = 200;
            m1["height"] = 200;
            m1["autoCreate"] = true;
            m1["creator"] = newCollectionContentID;
            m1["id1"] = link.InAtomId;
            m1["id2"] = link.OutAtomId;

            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteSendableRequest(Model.Id));
            await SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new NewLinkRequest(m1));
        }

        public void UpdateAnchor()
        {
            AnchorUpdated?.Invoke(this);
        }
    }
}
