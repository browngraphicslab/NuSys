using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LinkEditorTabViewModel
    {

        public LinkEditorTabViewModel(ILinkable linkable)
        {
            List<LibraryElementController> controllers = new List<LibraryElementController>();
            foreach (var linkLibraryElementModelId in linkable.GetAllLinks())
            {
                var controller = SessionController.Instance.ContentController.GetLibraryElementController(linkLibraryElementModelId);
                Debug.Assert(controller != null);
                controllers.Add(controller);
            }
        }

        
    }
}
