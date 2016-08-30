using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NusysIntermediate
{
    /// <summary>
    /// A base class for all the request args used for ArgsRequests. 
    /// All the subclasses of this class should be data hodlers that are simply serializable objects.
    /// This class simply provides an extensible way to serialize objects.
    /// </summary>
    public abstract class ServerRequestArgsBase
    {
        /// <summary>
        /// the request type that this request args base corresponds to.  
        /// Should only be set in the constructor.
        /// </summary>
        public NusysConstants.RequestType RequestType { get; private set; }

        /// <summary>
        /// base constructor. Takes in the request type that this request args class corresponds to. 
        /// </summary>
        /// <param name="requestType"></param>
        public ServerRequestArgsBase(NusysConstants.RequestType requestType)
        {
            RequestType = requestType;
        }

        /// <summary>
        /// this virtual method should return the json of the this args class.
        /// Its virtual in case you want to check anything before serializing the args.
        /// </summary>
        /// <returns></returns>
        public virtual string GetSerialized()
        {
            return JsonConvert.SerializeObject(this, new JsonSerializerSettings() {StringEscapeHandling = StringEscapeHandling.EscapeNonAscii});
        }

        /// <summary>
        /// method to make sure the args class has been properly created.  
        /// THis should be overriden in every sub class to make sure that they check their individual, required properties
        /// </summary>
        /// <returns></returns>
        protected abstract bool CheckArgsAreComplete();

        /// <summary>
        /// should be called to make sure that an args class has been properly created;
        /// </summary>
        public void CheckValidity()
        {
            var validArgs = CheckArgsAreComplete();
            Debug.Assert(validArgs);
        }
    }
}
