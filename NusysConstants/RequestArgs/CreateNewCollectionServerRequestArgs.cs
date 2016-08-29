using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// the args class used whenever you are creating a new collection request. 
    /// Populate this class and then pass it into a CreateNewCollectionRequest.
    /// </summary>
    public class CreateNewCollectionServerRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// parameterless constructor just tells the base class what request type this request args is for.
        /// </summary>
        public CreateNewCollectionServerRequestArgs() : base(NusysConstants.RequestType.CreateNewCollectionRequest) { }
        
        /// <summary>
        /// override checking method makes sure theres a dictionary present for both of the parameters.  
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return CreateNewContentRequestDictionary != null && CreateNewContentRequestDictionary.Count() > 0 && NewElementRequestDictionaries != null;
        }

        public Dictionary<string,object> CreateNewContentRequestDictionary { get; set; }

        public List<Dictionary<string,object>> NewElementRequestDictionaries { get; set; }
    }
}
