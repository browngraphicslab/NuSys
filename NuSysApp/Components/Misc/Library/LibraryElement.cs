using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class LibraryElement
    {
        public string ContentID { get; set; }
        public string Title { get; set; }
        public NodeType NodeType { get; set; }

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

        public LibraryElement(Dictionary<string, string> dict)
        {
            //id, data, type, title
            ContentID = dict["id"];
            if (dict.ContainsKey("title"))
            {
                Title = dict["title"];
            }
            try
            {
                if (dict.ContainsKey("type"))
                {
                    NodeType = (NodeType)Enum.Parse(typeof(NodeType), dict["type"]);
                }
            }
            catch (Exception e) { }
        }

        public bool InSearch(string s)
        {
            var title = Title?.ToLower() ?? "";
            var type = NodeType.ToString().ToLower();
            if (title.Contains(s) || type.Contains(s))
            {
                return true;
            }
            return false;
        }
    }
}
