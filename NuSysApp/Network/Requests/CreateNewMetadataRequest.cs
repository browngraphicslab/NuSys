using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    class CreateNewMetadataRequest : Request
    {
        public CreateNewMetadataRequest(NusysConstants.RequestType requestType, Message message = null) : base(requestType, message)
        {

        }

        public CreateNewMetadataRequest(Message message) : base(message)
        {

        }

        public CreateNewMetadataRequest(IRequestArgumentable requestArgs, NusysConstants.RequestType requestType) : base(requestArgs, requestType)
        {

        }

    }
}
