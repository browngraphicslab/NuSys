using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp.Controller
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

        public void UpdateAnchor()
        {
            AnchorUpdated?.Invoke(this);
        }
    }
}
