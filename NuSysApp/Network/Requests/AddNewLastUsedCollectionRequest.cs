using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Request class used to update the server sql table for last used collections. 
    /// To use this, populate an AddNewLastUsedCollectionServerRequestArgs class and then instatiate this class with it.
    /// Then await execution of this request.
    /// </summary>
    public class AddNewLastUsedCollectionRequest : ServerArgsRequest<AddNewLastUsedCollectionServerRequestArgs>
    {

        /// <summary>
        /// The default constructor takes in an AddNewLastUsedCollectionServerRequestArgs.
        /// For more usage info, refer to the comments in the class definition.
        /// </summary>
        /// <param name="args"></param>
        public AddNewLastUsedCollectionRequest(AddNewLastUsedCollectionServerRequestArgs args) : base(args) { }
    }
}
