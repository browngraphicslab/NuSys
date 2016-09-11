using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// request args class used to get the last used collctions for a certain user.
    /// This class should simply hold a single user id;
    /// </summary>
    public class GetLastUsedCollectionsServerRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// default constructor just sets the requets type
        /// </summary>
        public GetLastUsedCollectionsServerRequestArgs() : base(NusysConstants.RequestType.GetLastUsedCollectionsRequest)  {}

        /// <summary>
        /// this override checking method returns whether the User Id has been set.
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return !string.IsNullOrEmpty(UserId);
        }

        /// <summary>
        /// The string user Id that you are trying to get the last used collctions of.
        /// REQUIRED
        /// </summary>
        public string UserId { get; set; }
    }
}
