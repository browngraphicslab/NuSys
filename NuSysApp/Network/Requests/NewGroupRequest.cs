using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NuSysApp
{
    public class NewGroupRequest : Request
    {
        public NewGroupRequest(Message message) : base(RequestType.NewGroupRequest, message){}
    }
}
