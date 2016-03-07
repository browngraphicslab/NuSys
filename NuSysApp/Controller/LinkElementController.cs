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

        public LinkElementController(LinkModel model) : base(model)
        {
            var inElement = SessionController.Instance.IdToControllers[model.InAtomId];
            var outElement = SessionController.Instance.IdToControllers[model.OutAtomId];
            inElement.AddLink(this);
            outElement.AddLink(this);
        }

        public void UpdateAnchor()
        {
            AnchorUpdated?.Invoke(this);
        }
    }
}
