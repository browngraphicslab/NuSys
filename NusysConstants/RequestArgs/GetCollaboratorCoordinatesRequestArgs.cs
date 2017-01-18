using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NusysIntermediate
{
    /// <summary>
    /// Request args class used to fetch the coordinates of a collaborator
    /// </summary>
    public class GetCollaboratorCoordinatesRequestArgs : ServerRequestArgsBase
    {
        /// <summary>
        /// RREQUIRED:The string user id of the collaborator you wish to fetch the coordinates of
        /// </summary>
        public string UserId { get; set; }

        /// <summary>
        /// optional, this will be populated on the server if not locally. 
        /// It will be the user id of the person asking for coordinates.
        /// </summary>
        public string OriginalSenderId { get; set; }

        /// <summary>
        /// parameterless constructor just sets the request type in teh base abstract class
        /// </summary>
        /// <param name="requestType"></param>
        public GetCollaboratorCoordinatesRequestArgs() : base(NusysConstants.RequestType.GetCollaboratorCoordinatesRequest){}

        /// <summary>
        /// this checker just makes sure the user id has been set
        /// </summary>
        /// <returns></returns>
        protected override bool CheckArgsAreComplete()
        {
            return !string.IsNullOrEmpty(UserId);
        }
    }
}
