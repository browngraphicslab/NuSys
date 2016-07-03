using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ILinkable
    {
        void RequestAddNewLink(string idToLinkTo);
        void RequestRemoveLink(string linkID);
        void ChangeLinkTitle(string linkLibraryElementID, string title);
        void ChangeLinkTags(string linkLibraryElementID, HashSet<String> tags);
        string Id { get; }
        event EventHandler<LinkLibraryElementController> LinkAdded;
        event EventHandler<string> LinkRemoved;
        HashSet<LinkLibraryElementController> GetAllLinks();
    }
}
