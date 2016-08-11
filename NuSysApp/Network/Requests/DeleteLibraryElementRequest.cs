using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NusysIntermediate;

namespace NuSysApp
{
    public class DeleteLibraryElementRequest : Request
    {
        /// <summary>
        /// preferred constructor.  Should take in the LibraryElementId of the library element you wish to delete.  
        /// After executing this request, call DeleteLocally() on this request to actually perform the deletion locally.  
        /// </summary>
        /// <param name="id"></param>
        public DeleteLibraryElementRequest(string id) : base(NusysConstants.RequestType.DeleteLibraryElementRequest)
        {
            _message[NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY] = id;
        }

        /// <summary>
        /// this is the default, from-server deserializartion constructor.  
        /// </summary>
        /// <param name="m"></param>
        public DeleteLibraryElementRequest(Message m) : base(NusysConstants.RequestType.DeleteLibraryElementRequest,m) {}
        public override async Task CheckOutgoingRequest()
        {
            if (!_message.ContainsKey(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY))
            {
                throw new Exception("Library Element Delete requests must contains a library 'id' to delete");
            }
        }

        /// <summary>
        /// the method executed locally when another client deletes a library element. 
        ///  as of 8/11/16, only the requested library element is deleted locally, unlike in the DeleteLocally method. 
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));
            DeleteLibraryElementWithId(_message.GetString(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_LIBRARY_ID_KEY));
        }

        /// <summary>
        /// the method to be called after the request is successful.  
        /// This method must be called from the original client if they want to delete the item locally.
        /// It will return true if the element was removed, false otherwise.  
        /// </summary>
        /// <returns></returns>
        public bool DeleteLocally()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_RETURNED_DELETED_LIBRARY_IDS_KEY));

            //get all the to-delete libraryElementIds as a list
            var idsToDelete = _returnMessage.GetList<string>(NusysConstants.DELETE_LIBRARY_ELEMENT_REQUEST_RETURNED_DELETED_LIBRARY_IDS_KEY);

            //return whether the number of succesful deletes was the same as the number of ids
            return idsToDelete.Count(id => DeleteLibraryElementWithId(id)) == idsToDelete.Count();
        }

        /// <summary>
        /// the private method to be executed by either the ExecuteRequestFunction() or the explicit DeleteLocally().  
        /// This method actually deleted the library element on the client side.
        /// </summary>
        /// <param name="libraryId"></param>
        /// <returns></returns>
        private bool DeleteLibraryElementWithId(string libraryId)
        {
            var libraryElementController = SessionController.Instance.ContentController.GetLibraryElementController(libraryId);
            if (libraryElementController == null)
            {
                return false;
            }
            // This checks if this LibraryElementRequest is a region and if so then call the regionscontroller remove region method

            if (libraryElementController is RegionLibraryElementController)
            {
                SessionController.Instance.RegionsController.RemoveRegion(libraryElementController.LibraryElementModel as Region);
            }
            SessionController.Instance.LinksController.RemoveContent(libraryElementController);
            UITask.Run(delegate {
                libraryElementController.Delete();
            });
            return true;
        }
    }
}
