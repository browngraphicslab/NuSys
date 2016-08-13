using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NusysIntermediate;

namespace NuSysApp
{
    public class UpdateMetadataEntryRequestArgs : IRequestArgumentable
    {
        public MetadataEntry Entry { get; set; }
        public string LibraryElementId { get; set; }
        public List<string> NewValues { get; set; }
        public Message PackToRequestKeys()
        {
            Message message = new Message();
            message[NusysConstants.UPDATE_METADATA_REQUEST_LIBRARY_ID_KEY] = LibraryElementId;
            message[NusysConstants.UPDATE_METADATA_REQUEST_METADATA_KEY] = Entry.Key;
            message[NusysConstants.UPDATE_METADATA_REQUEST_METADATA_VALUE] = JsonConvert.SerializeObject(NewValues);
            return message;
        }
    }
}
