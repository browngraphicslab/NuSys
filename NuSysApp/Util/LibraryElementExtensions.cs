using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public static class LibraryElementExtensions
    {
        public static LibraryElementController GetController(this LibraryElementModel model)
        {
            Debug.Assert(model?.LibraryElementId != null);
            return SessionController.Instance.ContentController.GetLibraryElementController(model.LibraryElementId);
        }
    }
}
