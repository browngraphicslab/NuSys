using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public interface ILinkable
    {
        void AddNewLink(string idToLinkTo);
        void RemoveLink(string linkID);
        void ChangeLinkTitle();
        void ChangeLinkTags();
        List<string> GetAllLinks();
    }
}
