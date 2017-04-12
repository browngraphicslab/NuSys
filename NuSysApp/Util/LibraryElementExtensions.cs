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

        /// <summary>
        /// this static extension will return true if this client is supposed to have access to this library element.  
        /// False will be returned if the model is private and not owned by this person.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool AllowedToSee(this LibraryElementModel model)
        {
            return !(model.AccessType == NusysConstants.AccessType.Private && model.Creator != WaitingRoomView.UserID);
        }

        /// <summary>
        /// static method to see if we need to view this library element model in read-only mode.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool ViewInReadOnly(this LibraryElementModel model)
        {
            Debug.Assert(model != null);
            if (model.Type == NusysConstants.ElementType.Link)
            {
                var link = model as LinkLibraryElementModel;
                Debug.Assert(link != null);
                if (link == null)
                {
                    return true;
                }
                return
                    !(SessionController.Instance.ContentController.GetLibraryElementController(link.OutAtomId) != null &&
                      SessionController.Instance.ContentController.GetLibraryElementController(link.InAtomId) != null);
            }
            else
            {
                if (model.Creator == WaitingRoomView.UserID)
                {
                    return false;
                }
                if (model.AccessType == NusysConstants.AccessType.ReadOnly)
                {
                    return true;
                }
            }
            return false;
        }
    }
}
