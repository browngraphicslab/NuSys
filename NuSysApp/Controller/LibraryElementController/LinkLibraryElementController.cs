using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkLibraryElementController : LibraryElementController
    {
        public LinkLibraryElementModel LinkLibraryElementModel { get; private set; }
        public LinkLibraryElementController(LinkLibraryElementModel model) : base(model)
        {
            Debug.Assert(model != null);
            LinkLibraryElementModel = model;
        }

        public void DeleteLink()
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteRequest(new DeleteLibraryElementRequest(LinkLibraryElementModel.LibraryElementId));
          //  SessionController.Instance.ActiveFreeFormViewer.AllContent.First().Controller.RequestVisualLinkTo();
            Dispose();
        }
    }
}
