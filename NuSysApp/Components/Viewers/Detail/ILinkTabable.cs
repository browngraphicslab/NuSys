using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ILinkTabable
    {

        Task RequestAddNewLink(string idToLinkTo, string title);
        void RequestRemoveLink(string linkID);
        string ContentId { get; }
        event EventHandler<LinkLibraryElementController> LinkAdded;
        event EventHandler<string> LinkRemoved;
        HashSet<LinkLibraryElementController> GetAllLinks();
    }
}
