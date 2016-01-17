using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PowerPointAddIn
{
    public class SelectionItemView
    {
        public String BookmarkId;
        public Boolean IsExported;
        public String RtfContent;
        public String DocPath;
        public List<String> ImageNames;

        public SelectionItemView(string BookmarkId, Boolean IsExported, String RtfContent, String DocPath, List<String> ImageNames)
        {
            this.BookmarkId = BookmarkId;
            this.IsExported = IsExported;
            this.RtfContent = RtfContent;
            this.DocPath = DocPath;
            this.ImageNames = ImageNames;
        }
    }
}
