using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class UnsubscribeFromCollectionRequest : Request
    {
        public UnsubscribeFromCollectionRequest(string collectionID) : base(NusysConstants.RequestType.UnsubscribeFromCollectionRequest)
        {
            _message["server_collection_to_subscribe"] = collectionID;
        }
    }
}
