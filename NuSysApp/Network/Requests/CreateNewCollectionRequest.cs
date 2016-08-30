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
    /// Request used to create a new collection content data model (and default collection library element model) with a preset list of elements.  
    /// Populate a new CreateNewCollectionServerRequestArgs and then pass them into this constructor.  
    /// </summary>
    public class CreateNewCollectionRequest : ServerArgsRequest<CreateNewCollectionServerRequestArgs>
    {
        /// <summary>
        /// constructor takes in a request args class.  
        /// After calling this, await execution via SessionController's nusys network sesion. 
        /// After it has executed, call AddReturnedLibraryElementToLibrary to add the returned library element locally.
        /// </summary>
        /// <param name="requestArgs"></param>
        /// <param name="requestType"></param>
        public CreateNewCollectionRequest(CreateNewCollectionServerRequestArgs requestArgs) : base(requestArgs){}


        /// <summary>
        /// returns the CreateNewCollectionServerRequestArgs for this request.
        /// Should only be called server side.
        /// Will return null if it doesn't exist in the protected _message;
        /// </summary>
        /// <returns></returns>
        public override CreateNewCollectionServerRequestArgs GetArgs()
        {
            return base.GetArgsClassFromMessage();
        }

        /// <summary>
        /// call this method after the request has successfully returned to add the returned library element locally.
        /// You must make sure that the request has returned succesfullu before calling this method, however.
        /// Returned whether the adding of the library element was successfull;
        /// </summary>
        public bool AddReturnedLibraryElementToLibrary()
        {
            CheckWasSuccessfull();

            //make sure the returned model is present
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.NEW_CONTENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY));
            var modelString = _returnMessage.GetString(NusysConstants.NEW_CONTENT_REQUEST_RETURNED_LIBRARY_ELEMENT_MODEL_KEY);

            var libraryElement = LibraryElementModelFactory.DeserializeFromString(modelString);
            return SessionController.Instance.ContentController.Add(libraryElement) != null;
        }
    }
}
