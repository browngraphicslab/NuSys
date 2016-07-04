using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ILinkable
    {
        void AddNewLink(LinkId idToLinkTo);
        void RemoveLink(LinkId linkID);
        void ChangeLinkTitle(string linkLibraryElementID, string title);
        void ChangeLinkTags(string linkLibraryElementID, HashSet<String> tags);
        LinkId Id { get; }
        HashSet<LinkLibraryElementController> GetAllLinks();
    }
}
