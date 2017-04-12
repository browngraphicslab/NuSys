using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Args class used to represent the callback funcitonality of a request.
    /// You should define what function gets called when the request is successful
    /// </summary>
    /// <typeparam name="returnType"></typeparam>
    public class CallbackArgs<returnType>
    {
        /// <summary>
        /// Fucntion called when the request succesfully returns.  Can't be null;
        /// </summary>
        public Func<returnType,bool> SuccessFunction { get; set; }

        /// <summary>
        /// Function called when the request fails. Can be null
        /// </summary>
        public Func<returnType, bool> FailureFunction { get; set; }
    }
}
