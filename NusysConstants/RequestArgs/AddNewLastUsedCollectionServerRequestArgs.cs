using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// request args class used for creating a new entry in the LastUsedCollections table.
    /// This should simply be used to hold a user id and a collection id.
    /// </summary>
    public class AddNewLastUsedCollectionServerRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// default constructor.  just sets the base's request type
        /// </summary>
        public AddNewLastUsedCollectionServerRequestArgs() : base(NusysConstants.RequestType.AddNewLastUsedCollectionRequest){}

        /// <summary>
        /// this should just check to make sure both ids are set correctly. 
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            if (string.IsNullOrEmpty(CollectionLibraryId) || string.IsNullOrEmpty(UserId))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// The library element id of the collection being used.
        /// REQUIRED
        /// </summary>
        public string CollectionLibraryId { get; set; }
        
        /// <summary>
        /// The id of the user that used the collection. 
        /// REQUIRED
        /// </summary>
        public string UserId { get; set; }
    }
}
