using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{

    /// <summary>
    /// Note: this class is no longer active. Functionality has been incorporated into NodeContentModel
    /// </summary>
    public class LibraryElement
    {
        public string ContentID { get; set; }
        public string Title { get; set; }
        public ElementType ElementType { get; set; }

        public LibraryElement(string id, string title, ElementType type)
        {
            ContentID = id;
            Title = title;
            ElementType = type;
        }

        public LibraryElement(string id)
        {
            ContentID = id;
        }

        public LibraryElement(Dictionary<string, object> dict)
        {
            //id, data, type, title
            ContentID = (string)dict["id"];
            if (dict.ContainsKey("title"))
            {
                Title = (string)dict["title"];
            }
            if (dict.ContainsKey("type"))
            {
                ElementType = (ElementType)Enum.Parse(typeof(ElementType), (string)dict["type"], true);
            }
        }
        public LibraryElement(Dictionary<string, string> dict)
        {
            //id, data, type, title
            ContentID = (string)dict["id"];
            if (dict.ContainsKey("title"))
            {
                Title = (string)dict["title"];
            }
            if (dict.ContainsKey("type"))
            {
                ElementType = (ElementType)Enum.Parse(typeof(ElementType), (string)dict["type"], true);
            }
        }

        public bool InSearch(string s)
        {
            var title = Title?.ToLower() ?? "";
            var type = ElementType.ToString().ToLower();
            if (title.Contains(s) || type.Contains(s))
            {
                return true;
            }
            return false;
        }
    }
}
