using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    public class QueryArgs
    {
        public QueryArgs()
        {
            SearchText = new List<string>();
            Keywords = new List<string>();
            Metadata = new List<string>();
            ElementTypes = new List<string>();
            CreatorUserIds = new List<string>();
        }

        /// <summary>
        /// the string to search by.  Not required
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// A general list of all things the user may have searched for.
        /// Cleaned of category names
        /// </summary>
        public List<string> SearchText { get; set; }

        /// <summary>
        /// the list of keywords for the query.  
        /// An object will be returned if it contains ANY of these keywords. 
        ///  aka an 'OR' operator
        /// </summary>
        public List<string> Keywords { get; set; }

        /// <summary>
        /// the metadata needed for the query.  
        /// An object will be returned if it contains ANY of these metadata key value pairs.
        /// </summary>
        public List<string> Metadata { get; set; }

        /// <summary>
        /// the list of element types for the query.  
        /// An object will be returned if it is any of these element types
        /// </summary>
        public List<string> ElementTypes { get; set; }

        /// <summary>
        /// the list of creator user ids for the query.  
        /// An object will be returned if it is created by any of these creator user ids
        /// </summary>
        public List<string> CreatorUserIds { get; set; }
    }
}
