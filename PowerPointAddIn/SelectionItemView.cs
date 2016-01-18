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
        public String FilePath;
        public List<String> ImageNames;
        public String Token;

        public SelectionItemView(string BookmarkId, Boolean IsExported, String RtfContent, String FilePath, List<String> ImageNames, String Token)
        {
            this.BookmarkId = BookmarkId;
            this.IsExported = IsExported;
            this.RtfContent = RtfContent;
            this.FilePath = FilePath;
            this.ImageNames = ImageNames;
            this.Token = Token;
        }
    }
}
