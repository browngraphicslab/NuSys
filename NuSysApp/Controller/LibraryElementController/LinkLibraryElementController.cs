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

        //public void DeleteLink(string linkLibraryElementID)
        //{
        //    // get all instances of the link
        //    Debug.Assert(SessionController.Instance.LinksController.GetLinkableIdsOfContentIdInstances(linkLibraryElementID).Count() != 0);

        //    var linkableIds =
        //        SessionController.Instance.LinksController.GetLinkableIdsOfContentIdInstances(linkLibraryElementID]);

        //    //Removes the link from the content at the other end of the Link
        //    var linkModel = SessionController.Instance.ContentController.GetContent(linkLibraryElementID) as LinkLibraryElementModel;
        //    if (linkModel?.InAtomId == _linkTabable.ContentId)
        //    {
        //        var otherController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel?.OutAtomId);
        //        otherController.RequestRemoveLink(linkLibraryElementID);
        //    }
        //    else if (linkModel?.OutAtomId == _linkTabable.ContentId)
        //    {
        //        var otherController = SessionController.Instance.ContentController.GetLibraryElementController(linkModel?.InAtomId);
        //        otherController.RequestRemoveLink(linkLibraryElementID);
        //    }

        //    //Create templates to display in the list view
        //    foreach (var template in LinkTemplates)
        //    {
        //        if (template.ID == linkLibraryElementID)
        //        {
        //            LinkTemplates.Remove(template);
        //            break;
        //        }
        //    }
        //}
    }
}
