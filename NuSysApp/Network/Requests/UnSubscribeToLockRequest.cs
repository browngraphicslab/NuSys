using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Request class to facilitiate the stopping of listening to lock updates for a specific lock id.
    /// Executing this will unsubscribe from the lock AS WELL AS RETURN THE LOCK IF THIS CLIENT HOLDS IT.
    /// </summary>
    public class UnSubscribeToLockRequest : FullArgsRequest<UnSubscribeToLockRequestArgs, ServerReturnArgsBase>
    {
        /// <summary>
        /// To user this request, first populate a UnSubscribeToLockRequestArgs class,
        /// Then await this requests execution with the nusysNetworkSession
        /// </summary>
        /// <param name="args"></param>
        public UnSubscribeToLockRequest(UnSubscribeToLockRequestArgs args) : base(args){}

        /// <summary>
        /// This will never be called because the server shouldn't forward this request type
        /// </summary>
        /// <param name="senderArgs"></param>
        /// <param name="returnArgs"></param>
        public override void ExecuteRequestFunction(UnSubscribeToLockRequestArgs senderArgs, ServerReturnArgsBase returnArgs){}
    }
}
