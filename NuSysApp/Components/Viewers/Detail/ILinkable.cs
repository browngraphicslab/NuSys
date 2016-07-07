using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ILinkable
    {

        Task RequestAddNewLink(LinkId idToLinkTo, string title);
        void RequestRemoveLink(LinkId linkID);
        void ChangeLinkTitle(string linkLibraryElementID, string title);
        void ChangeLinkTags(string linkLibraryElementID, HashSet<String> tags);
        LinkId Id { get; }
        event EventHandler<LinkLibraryElementController> LinkAdded;
        event EventHandler<string> LinkRemoved;
        HashSet<LinkLibraryElementController> GetAllLinks();
    }
}
