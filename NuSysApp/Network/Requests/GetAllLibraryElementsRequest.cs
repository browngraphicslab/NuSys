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
    /// to get all the library elements that current exist on the server.
    /// Usage:
    ///     var request = new GetAllLibraryElementsRequest();
    ///     await SessionController.Instance.NuSysNetworkSession.ExecuteRequestAsync(request);
    ///     var libraryElementModels = request.GetReturnedLibraryElementModels();
    /// </summary>
    public class GetAllLibraryElementsRequest : Request
    {
        /// <summary>
        /// preferred constructor.
        /// Use this constructor for all uses.
        /// </summary>
        public GetAllLibraryElementsRequest() : base(NusysConstants.RequestType.GetAllLibraryElementsRequest) {}

        /// <summary>
        /// Returns the entire list of all the library element models on the server after thr request was successful.
        /// Check to make sure the request was successful before calling this method.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<LibraryElementModel> GetReturnedLibraryElementModels()
        {
            if (WasSuccessful() != true)
            {
                //If this fails here, check with .WasSuccessful() before calling this method.
                throw new Exception("The request hasn't returned yet or was unsuccessful");
            }
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.GET_ALL_LIBRARY_ELEMENTS_REQUEST_RETURNED_LIBRARY_ELEMENT_MODELS_KEY));
            var jsonStrings = _returnMessage.Get<List<string>>(NusysConstants.GET_ALL_LIBRARY_ELEMENTS_REQUEST_RETURNED_LIBRARY_ELEMENT_MODELS_KEY);
            var returnModels = new List<LibraryElementModel>();
            foreach (var json in jsonStrings)
            {
                try
                {
                    returnModels.Add(LibraryElementModelFactory.DeserializeFromString(json));
                }
                catch (Exception e)
                {
                    Debug.Fail("shouldn't reach here, do better error handling");
                }
            }
            return returnModels;
        }
    }
}
