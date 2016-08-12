using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp.Network.Requests.RequestArgs
{
    class CreateNewMetadataRequestArgs : IRequestArgumentable
    {
        public MetadataEntry Entry { get; set; }

        public string LibraryElementId { get; set; }
        public Message PackToRequestKeys()
        {
            
        }
    }
}
