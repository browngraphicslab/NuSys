using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LibraryElementControllerFactory
    {
        public static LibraryElementController CreateFromModel(LibraryElementModel model)
        {
            LibraryElementController controller;
            switch (model.Type)
            {
                case ElementType.Word:
                    //Do debug.asserts above the controller instantiation to make sure the model types are correct
                    controller = new WordNodeLibraryElementController(model);
                    SessionController.Instance.NuSysNetworkSession.LockController.AddLockable((ILockable)controller);
                    break;
                case ElementType.Link:
                    Debug.Assert(model is LinkLibraryElementModel);
                    controller = new LinkLibraryElementController(model as LinkLibraryElementModel);
                    break;
                default:
                    controller = new LibraryElementController(model);
                    break;
            }
            return controller;
        }
    }
}
