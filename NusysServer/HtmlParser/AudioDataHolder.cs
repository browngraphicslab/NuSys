using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysServer
{
    public class AudioDataHolder : DataHolder
    {
        public Uri Uri { get; set; }
        public AudioDataHolder(Uri uri, string title) : base(DataType.Audio, title)
        {
            Uri = uri;
        }
    }
}
