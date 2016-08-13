using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class DeleteMetadataRequestArgs : IRequestArgumentable
    {
        public MetadataEntry Entry { get; set; }
        public string LibraryElementId { get; set; }
        public Message PackToRequestKeys()
        {
            Message message = new Message();
            message[NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY] = LibraryElementId;
            message[NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY] = Entry.Key;
            return message;
        }
    }
}
