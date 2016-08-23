using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp2
{
    public class UnsubscribeFromCollectionRequest : Request
    {
        public UnsubscribeFromCollectionRequest(string collectionID) : base(RequestType.UnsubscribeFromCollectionRequest)
        {
            _message["server_collection_to_subscribe"] = collectionID;
            SetServerEchoType(ServerEchoType.None);
            SetSubscribingToCollection(true, ServerSubscriptionType.Unsubscribe);
        }
    }
}
