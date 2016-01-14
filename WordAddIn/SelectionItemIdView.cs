using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WordAddIn
{
    public class SelectionItemIdView
    {
        public String BookmarkId;
        public Boolean IsExported;
        public String DateTimeExported;

        public SelectionItemIdView(string BookmarkId, Boolean IsExported, String DateTimeExported)
        {
            this.BookmarkId = BookmarkId;
            this.IsExported = IsExported;
            this.DateTimeExported = DateTimeExported;
        }
    }
}
