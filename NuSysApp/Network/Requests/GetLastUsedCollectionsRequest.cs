using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    /// <summary>
    /// Request class used to fetch all of the last used collections for a single user.
    /// To use this class, populate a GetLastUsedCollectionsServerRequestArgs class.
    /// Then instantiate this class using that args class.
    /// Then await the execution of this request.
    /// After a successful return, call GetReturnedModels() to get the returned models.
    /// </summary>
    public class GetLastUsedCollectionsRequest : ServerArgsRequest<GetLastUsedCollectionsServerRequestArgs>
    {
        /// <summary>
        /// constructor takes in the args class needed for this request.
        /// Refer to the class definition comments for more usage details.
        /// </summary>
        /// <param name="args"></param>
        public GetLastUsedCollectionsRequest(GetLastUsedCollectionsServerRequestArgs args): base(args) { }

        /// <summary>
        /// method used to get the returned LastUsedCollectionModels from the server.
        /// you must make sure that the reuqest has returned succesfully before calling this method.
        /// </summary>
        /// <returns></returns>
        public List<LastUsedCollectionModel> GetReturnedModels()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_LAST_USED_COLLECTIONS_REQUEST_RETURNED_MODELS_KEY));
            return _returnMessage.GetList<LastUsedCollectionModel>(NusysConstants.GET_LAST_USED_COLLECTIONS_REQUEST_RETURNED_MODELS_KEY);
        }
    }
}
