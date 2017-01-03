using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class PdfDataHolder : DataHolder
    {
        public Uri Uri { get; set; }
        public PdfDataHolder(Uri uri,String title) : base(DataType.Pdf,title)
        {
            this.Uri = uri;
        }
    }
}
