using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LibraryElement
    {
        public string ContentID;
        public string Title;
        public string NodeType;

        public LibraryElement(string id, string title, string type)
        {
            ContentID = id;
            Title = title;
            NodeType = type;
        }
    }
}
