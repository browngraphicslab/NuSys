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
        public Boolean IsExported;
        public String RtfContent;
        public String FilePath;
        public String DateTimeExported;
        public List<String> ImageNames;
        public String Token;

        public SelectionItemView(Boolean IsExported, String RtfContent, String FilePath, String DateTimeExported, List<String> ImageNames, String Token)
        {
            this.IsExported = IsExported;
            this.RtfContent = RtfContent;
            this.FilePath = FilePath;
            this.DateTimeExported = DateTimeExported;
            this.ImageNames = ImageNames;
            this.Token = Token;
        }
    }
}
