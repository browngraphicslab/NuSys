using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class SubscribeToCollectionRequest : Request
    {
        public SubscribeToCollectionRequest(string collectionID) : base(NusysConstants.RequestType.SubscribeToCollectionRequest)
        {
            _message["server_collection_to_subscribe"] = collectionID;
        }
    }
}
