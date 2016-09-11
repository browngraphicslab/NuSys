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
    /// this class is used to make a server request to delete an Element (node).  
    /// If the request is successful, it means that the element has been deleted on the server 
    /// AND that the other clients have been notified about the deletion.
    /// 
    /// IN ORDER TO DELETE THE NODE LOCALLY, CALL THE RemoveNodeLocally FUNCTION.
    /// </summary>
    public class DeleteElementRequest : Request
    {
        /// <summary>
        /// this is the preferred constructor.  Pass in the ID of the element model that you wish to delete.
        /// After the request returns, use RemoveNodeLocally to actually execute the removal function locally.
        /// </summary>
        /// <param name="elementId"></param>
        public DeleteElementRequest(string elementId) : base(NusysConstants.RequestType.DeleteElementRequest)//maybe make an abstract delete sendable class and have this extend that
        {
            _message[NusysConstants.DELETE_ELEMENT_REQUEST_ELEMENT_ID] = elementId;
        }

        /// <summary>
        /// default constructor for server deserialization
        /// </summary>
        /// <param name="message"></param>
        public DeleteElementRequest(Message message) : base(message) {}

        /// <summary>
        /// this gets called whenever another client calls the delete node request.  
        /// This method should simply get the ID of the element and delete it locally
        /// </summary>
        /// <returns></returns>
        public override async Task ExecuteRequestFunction()
        {
            Debug.Assert(_message.ContainsKey(NusysConstants.DELETE_ELEMENT_REQUEST_ELEMENT_ID));

            var id = _message.GetString(NusysConstants.DELETE_ELEMENT_REQUEST_ELEMENT_ID);
            await UITask.Run(async delegate
            {
                RemoveElementWithId(id);
            });
        }


        /// <summary>
        /// This method can be called to remove the node locally after the request has executed.  
        /// 
        /// It will throw exceptions if the 
        /// </summary>
        /// <returns></returns>
        public bool RemoveNodeLocally()
        {
            CheckWasSuccessfull();
            Debug.Assert(_returnMessage.ContainsKey(NusysConstants.DELETE_ELEMENT_REQUEST_RETURNED_DELETED_ELEMENT_ID));
            var elementId = _returnMessage.GetString(NusysConstants.DELETE_ELEMENT_REQUEST_RETURNED_DELETED_ELEMENT_ID);
            return RemoveElementWithId(elementId);
        }

        /// <summary>
        /// Private method to remove the element on the local client.  
        /// Will be called when another client removes and node and CAN be called from the original sender as well.
        /// 
        /// returns whether the removal was successfull
        /// </summary>
        /// <param name="elementId"></param>
        /// <returns></returns>
        private bool RemoveElementWithId(string elementId)
        {
            if (!SessionController.Instance.IdToControllers.ContainsKey(elementId))
            {
                return false;
            }

            var controller = SessionController.Instance.IdToControllers[elementId];
            if (controller.Model != null)
            {
                var parent = SessionController.Instance.ContentController.GetLibraryElementModel(controller.Model.ParentCollectionId) as CollectionLibraryElementModel;
                parent?.Children.Remove(elementId);
            }
            controller.Delete(this);
            ElementController removed;
            SessionController.Instance.IdToControllers.TryRemove(elementId, out removed);
            return true;
        }
    }
    public class DeleteSendableRequestException : Exception
    {
        public DeleteSendableRequestException(string message) : base(message) { }
        public DeleteSendableRequestException() : base("There was an error in the Delete Node Request") { }
    }
}
