using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class GetFileBytesRequestReturnArgs : ServerReturnArgsBase
    {
        public byte[] Bytes { get; set; }
    }
}
