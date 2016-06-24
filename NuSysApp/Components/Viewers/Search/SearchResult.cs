using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class SearchResult
    {
        // Properties Returned By Server
        public string ContentID { get; set; }
        public ElementType ElementType { get; set; }
        public string ExtraInfo { get; set; }

        public SearchResult(string contentId, ElementType type)
        {
            ContentID = contentId;
            ElementType = type;
            ExtraInfo = string.Empty;
        }

        public SearchResult(string contentId, ElementType type, string info) : this(contentId, type)
        {
            ExtraInfo = info;
        }

        //todo add methods for displaying the result based on given info
    }
}
