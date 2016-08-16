using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// The request needs to know what key or what library element we are attempting to remove.
    /// </summary>
    public class DeleteMetadataRequestArgs : IRequestArgumentable
    {
        public string Key { get; set; }
        public string LibraryElementId { get; set; }
        public Message PackToRequestKeys()
        {
            Message message = new Message();
            message[NusysConstants.DELETE_METADATA_REQUEST_LIBRARY_ID_KEY] = LibraryElementId;
            message[NusysConstants.DELETE_METADATA_REQUEST_METADATA_KEY] = Key;
            return message;
        }
    }
}
