using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    class CreateNewMetadataRequestArgs : IRequestArgumentable
    {
        public MetadataEntry Entry { get; set; }

        public string LibraryElementId { get; set; }
        public Message PackToRequestKeys()
        {
            Message message = new Message();
            message[NusysConstants.CREATE_NEW_METADATA_REQUEST_LIBRARY_ID_KEY] = LibraryElementId;
            message[NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_KEY_KEY] = Entry.Key;
            message[NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_VALUE_KEY] = Entry.Values;
            message[NusysConstants.CREATE_NEW_METADATA_REQUEST_METADATA_MUTABILITY_KEY] = Entry.Mutability;
            return message;



        }
    }
}
