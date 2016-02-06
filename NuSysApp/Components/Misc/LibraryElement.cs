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
        public NodeType NodeType;

        public LibraryElement(string id, string title, NodeType type)
        {
            ContentID = id;
            Title = title;
            NodeType = type;
        }

        public LibraryElement(string id)
        {
            ContentID = id;
        }
    }
}
