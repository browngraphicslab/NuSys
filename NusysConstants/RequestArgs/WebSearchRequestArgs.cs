using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Request args class used to create a web-search request
    /// </summary>
    public class WebSearchRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// The search string for the web search
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// Parameterless constructor 
        /// </summary>
        public WebSearchRequestArgs() : base(NusysConstants.RequestType.WebSearchRequest){}

        /// <summary>
        /// this should just check to see if the search string is null or empty
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return !string.IsNullOrEmpty(SearchString);
        }
    }
}
