using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parser
{
    public class VideoDataHolder : DataHolder
    {

        public Uri Uri { get; set; }
        public VideoDataHolder(Uri uri,string title)  : base(DataType.Video,title)
        {
            Uri = uri;
        }
    }
}
