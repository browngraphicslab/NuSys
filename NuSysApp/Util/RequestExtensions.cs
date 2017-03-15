using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public static class RequestExtensions
    {
        public static void Execute<S, T>(this CallbackRequest<S, T> request) where T : ServerReturnArgsBase, new() where S : ServerRequestArgsBase
        {
            SessionController.Instance.NuSysNetworkSession.ExecuteCallbackRequest(request);
        }
    }
}
